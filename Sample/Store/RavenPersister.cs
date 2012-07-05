using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
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

        public void PersistEvents(Guid streamId, IEnumerable<IEvent> events)
        {
            using (var session = _store.OpenSession())
            {
                var attempt = PrepareCommit(session, streamId, events);
                EnsureEnabled(session, attempt);
            }
        }

        private static Commit PrepareCommit(IDocumentSession session, Guid streamId, IEnumerable<IEvent> events)
        {
            var attempt = new Commit
                          {
                              Revision = events.Select(e => e.StreamRevision).Max(),
                              Stream = streamId,
                              Events = events.ToArray(),
                          };

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

        public IEnumerable<IEvent> GetEvents(Guid streamId, int minRevision, int maxRevision)
        {
            using (var session = _store.OpenSession())
            {
                var commits = session.Query<Commit, CommitsByStreamRevision>().Where(c => c.Stream == streamId && c.Events.Any(e => e.StreamRevision >= minRevision && e.StreamRevision <= maxRevision)).ToArray();
                return commits
                    .SelectMany(c => c.Events)
                    .Where(e => e.StreamId == streamId && e.StreamRevision >= minRevision && e.StreamRevision <= maxRevision);
            }
        }

        public IEnumerable<IEvent> GetUndispatchedEvents()
        {
            throw new NotImplementedException();
        }

        public void MarkAsDispatched(IEvent evt)
        {
            throw new NotImplementedException();
        }

        public class Commit
        {
            public int Id { get; set; }
            public Guid Stream { get; set; }
            public int Revision { get; set; }
            public bool Enabled { get; set; }
            public IEvent[] Events { get; set; }
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
}