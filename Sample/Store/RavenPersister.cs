using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Linq;
using Raven.Client.Indexes;

namespace CQRS.Sample.Store
{
    public class RavenPersister : IPersister
    {
        private readonly IDocumentStore _store;

        public RavenPersister(IDocumentStore store)
        {
            _store = store;
        }

        public void PersistEvents(Guid streamId, IEnumerable<StoreEvent> events)
        {
            using (var session = _store.OpenSession())
            {
                var attempt = PrepareCommit(session, streamId, events);
                EnsureEnabled(session, attempt);
            }
        }

        private static Commit PrepareCommit(IDocumentSession session, Guid streamId, IEnumerable<StoreEvent> events)
        {
            var attempt = new Commit
                          {
                              Revision = events.Select(e => e.StreamRevision).Max(),
                              Stream = streamId,
                              Events = events.ToArray(),
                          };
            attempt.Id = String.Format("{0}/{1}", attempt.Stream, attempt.Revision);
            //session.Advanced.UseOptimisticConcurrency = true;
            session.Store(attempt);
            session.SaveChanges();

            return attempt;
        }

        private static void EnsureEnabled(IDocumentSession session, Commit attempt)
        {
            var minRevision = attempt.Events.Select(e => e.StreamRevision).Min();
            var concurrentCommits = session
                .Query<Commit, CommitsByStreamRevision>()
                .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                .Where(c => c.Stream == attempt.Stream && c.Revision >= minRevision)
                .ToArray();
            if (concurrentCommits.Any(c => c.Enabled))
            {
                throw new OptimisticConcurrencyException();
            }
            var winner = concurrentCommits.OrderBy(c => LastModified(session, c)).First();
            if (attempt == winner)
            {
                attempt.Enabled = true;
                session.SaveChanges();
            }
            else
            {
                throw new OptimisticConcurrencyException();
            }
        }

        private static DateTime LastModified(IDocumentSession session, Commit commit)
        {
            var metadata = session.Advanced.GetMetadataFor(commit);
            return metadata.Value<DateTime>("Last-Modified");
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
                    .Query<Commit, CommitByEventId>()
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
            public bool Enabled { get; set; }
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