using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Exceptions;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace CQRS.Sample.Store
{
    public class RavenPersister : IPersister
    {
        readonly IDocumentStore _store;

        public RavenPersister(IDocumentStore store)
        {
            _store = store;
        }

        public void PersistEvents(Guid streamId, IEnumerable<StoreEvent> events)
        {
            try
            {
                using (var session = _store.OpenSession())
                {
                    var attempt = new Commit
                    {
                        Revision = events.Select(e => e.StreamRevision).Max(),
                        Stream = streamId,
                        Events = events.ToArray(),
                    };
                    attempt.Id = String.Format("{0}/{1}", attempt.Stream, events.Select(e => e.StreamRevision).Min());
                    session.Advanced.UseOptimisticConcurrency = true;
                    session.Store(attempt);
                    session.SaveChanges();
                }
            } catch (ConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }
        }

        public IEnumerable<StoreEvent> GetEvents(Guid streamId, int minRevision, int maxRevision)
        {
            using (var session = _store.OpenSession())
            {
                var commits = session
                    .Query<Commit, CommitsByStreamRevision>()
                    .Where(
                        c =>
                        c.Stream == streamId &&
                        c.Events.Any(e => e.StreamRevision >= minRevision && e.StreamRevision <= maxRevision)).ToArray();
                return commits
                    .SelectMany(c => c.Events)
                    .Where(
                        e =>
                        e.StreamId == streamId && e.StreamRevision >= minRevision && e.StreamRevision <= maxRevision);
            }
        }

        public void MarkAsDispatched(StoreEvent evt)
        {
            using (var session = _store.OpenSession())
            {
                var storedEvent = session
                    .Query<Commit>()
                    .First(c => c.Events.Any(e => e.Id == evt.Id))
                    .Events
                    .Single(e => e.Id == evt.Id);
                storedEvent.IsDispatched = true;
                session.SaveChanges();
            }
        }

        public IEnumerable<StoreEvent> GetUndispatchedEvents()
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Commit, CommitByEventId>()
                    .Where(c => c.Events.Any(e => !e.IsDispatched))
                    .ToArray()
                    .SelectMany(c => c.Events)
                    .Where(e => !e.IsDispatched);
            }
        }

        public class Commit
        {
            public string Id { get; set; }
            public Guid Stream { get; set; }
            public int Revision { get; set; }
            public StoreEvent[] Events { get; set; }
        }
    }

    public static class Register
    {
        public static void Indexes(IDocumentStore store)
        {
            IndexCreation.CreateIndexes(typeof (RavenPersister).Assembly, store);
        }
    }

    public class CommitsByStreamRevision : AbstractIndexCreationTask<RavenPersister.Commit, RavenPersister.Commit>
    {
        public CommitsByStreamRevision()
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

    public class CommitByEventId : AbstractIndexCreationTask<RavenPersister.Commit, RavenPersister.Commit>
    {
        public CommitByEventId()
        {
            Map = commits => from commit in commits
                             from evt in commit.Events
                             select new
                             {
                                 Events_Id = evt.Id,
                                 Events_IsDispatched = evt.IsDispatched
                             };
        }
    }
}