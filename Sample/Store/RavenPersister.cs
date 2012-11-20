using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Bootstrapping;
using Raven.Abstractions.Exceptions;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace CQRS.Sample.Store
{
    public class RavenPersister : IPersister
    {
        private readonly IDocumentStore _store;

        public RavenPersister(DocumentStoreConfiguration storeConfig)
        {
            _store = storeConfig.EventStore;
            Register.Indexes(_store);
        }

        public void PersistCommit(Guid streamId, IEnumerable<StoreEvent> events)
        {
            try
            {
                using (var session = _store.OpenSession())
                {
                    session.Advanced.UseOptimisticConcurrency = true;

                    var commit = new Commit(events);
                    foreach (var evt in commit.Events)
                    {
                        evt.StreamId = streamId;
                    }
                    session.Store(commit);
                    session.SaveChanges();
                }
            }
            catch (ConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }
        }

        public IEnumerable<Commit> GetCommits(Guid streamId, int minRevision, int maxRevision)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Commit, CommitsByRevision>()
                    .Customize(a => a.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(c => c.Events.Any(e =>
                           e.StreamId == streamId && 
                           e.StreamRevision >= minRevision &&
                           e.StreamRevision <= maxRevision))
                    .ToArray();
            }
        }

        public IEnumerable<Commit> GetUndispatchedCommits()
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Commit, CommitsByIsDispatched>()
                    .Customize(a => a.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(c => !c.IsDispatched)
                    .ToArray();
            }
        }

        public void MarkAsDispatched(Commit c)
        {
            using (var session = _store.OpenSession())
            {
                var storedEvent = session.Load<Commit>(c.Id);
                storedEvent.IsDispatched = true;
                session.SaveChanges();
            }
        }
    }

    public static class Register
    {
        public static void Indexes(IDocumentStore store)
        {
            new CommitsByRevision().Execute(store);
            new CommitsByIsDispatched().Execute(store);
        }
    }

    public class CommitsByRevision : AbstractIndexCreationTask<Commit>
    {
        public CommitsByRevision()
        {
            Map = commits => from c in commits
                             from e in c.Events
                             select new
                                    {
                                        Events_StreamId = e.StreamId,
                                        Events_StreamRevision = e.StreamRevision,
                                    };
            Sort(commit => commit.Id, SortOptions.Int);
        }
    }

    public class CommitsByIsDispatched : AbstractIndexCreationTask<Commit>
    {
        public CommitsByIsDispatched()
        {
            Map = commits => from c in commits
                             select new {c.IsDispatched};
        }
    }
}