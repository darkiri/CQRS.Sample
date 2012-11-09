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

        public void PersistEvents(Guid streamId, IEnumerable<StoreEvent> events)
        {
            try
            {
                using (var session = _store.OpenSession())
                {
                    var commit = new Commit
                                 {
                                     Stream = streamId,
                                     Events = events.ToArray(),
                                 };
                    session.Advanced.UseOptimisticConcurrency = true;
                    session.Store(commit);
                    session.SaveChanges();
                }
            }
            catch (ConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }
        }

        public IEnumerable<StoreEvent> GetEvents(Guid streamId, int minRevision, int maxRevision)
        {
            var commits = LoadCommits(streamId, minRevision, maxRevision);
            var events = FilterEvents(commits, minRevision, maxRevision);
            return events;
        }

        private IEnumerable<Commit> LoadCommits(Guid streamId, int minRevision, int maxRevision)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Commit, CommitsByEventRevision>()
                    .Customize(a => a.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(c => c.Stream == streamId &&
                                c.Events.Any(e => e.StreamRevision >= minRevision &&
                                                  e.StreamRevision <= maxRevision))
                    .ToArray();
            }
        }

        private static IEnumerable<StoreEvent> FilterEvents(IEnumerable<Commit> commits, int minRevision, int maxRevision)
        {
            return commits
                .SelectMany(c => c.Events)
                .Where(e => e.StreamRevision >= minRevision &&
                            e.StreamRevision <= maxRevision)
                .OrderBy(e => e.StreamRevision);
        }

        public IEnumerable<StoreEvent> GetUndispatchedEvents()
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Commit, CommitByEventDispatched>()
                    .Customize(a => a.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(c => c.Events.Any(e => !e.IsDispatched))
                    .ToArray()
                    .SelectMany(c => c.Events)
                    .Where(e => !e.IsDispatched)
                    .OrderBy(e => e.StreamRevision);
            }
        }

        public void MarkAsDispatched(StoreEvent evt)
        {
            using (var session = _store.OpenSession())
            {
                var storedEvent = session
                    .Query<Commit, CommitByEventId>()
                    .First(c => c.Events.Any(e => e.Id == evt.Id))
                    .Events
                    .Single(e => e.Id == evt.Id);
                storedEvent.IsDispatched = true;
                session.SaveChanges();
            }
        }

        public class Commit
        {
            public Guid Stream { get; set; }
            public StoreEvent[] Events { get; set; }

            public string Id
            {
                get { return String.Format("commits/{0}/{1}", Stream, Events.Select(e => e.StreamRevision).Min()); }
            }

            public int Revision
            {
                get { return Events.Select(e => e.StreamRevision).Max(); }
            }
        }
    }

    public static class Register
    {
        public static void Indexes(IDocumentStore store)
        {
            new CommitsByEventRevision().Execute(store);
            new CommitByEventDispatched().Execute(store);
            new CommitByEventId().Execute(store);
        }
    }

    public class CommitsByEventRevision : AbstractIndexCreationTask<RavenPersister.Commit, RavenPersister.Commit>
    {
        public CommitsByEventRevision()
        {
            Map = commits => from commit in commits
                             from evt in commit.Events
                             select new
                                    {
                                        commit.Stream,
                                        commit.Revision,
                                        Events_StreamRevision = evt.StreamRevision,
                                    };
            Sort(commit => commit.Revision, SortOptions.Int);
        }
    }

    public class CommitByEventDispatched : AbstractIndexCreationTask<RavenPersister.Commit, RavenPersister.Commit>
    {
        public CommitByEventDispatched()
        {
            Map = commits => from commit in commits
                             from evt in commit.Events
                             select new
                                    {
                                        Events_IsDispatched = evt.IsDispatched
                                    };
        }
    }

    public class CommitByEventId : AbstractIndexCreationTask<RavenPersister.Commit, RavenPersister.Commit>
    {
        public CommitByEventId()
        {
            Map = commits => from commit in commits
                             from evt in commit.Events
                             select new
                                    {
                                        Events_Id = evt.Id
                                    };
        }
    }
}