using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Store;
using Machine.Specifications;
using NUnit.Framework;
using Raven.Client;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class raven_persistence_context
    {
        protected static readonly Guid StreamId = Guid.Parse("90AEA96E-C0A5-4CDF-9272-8A22986AC738");

        protected static IDocumentStore Store;
        protected static DocumentStoreConfiguration StoreConfig;
        protected static RavenPersister Persister;

        private Establish context =()=>
        {
            StoreConfig = Bootstrapper.InMemory();
            Store = StoreConfig.EventStore;
            Persister = new RavenPersister(StoreConfig);
        };

        private Cleanup all =()=> StoreConfig.Dispose();

        protected static StoreEvent Event(string payload, int revision) {
            var evt = new StringEvent {
                Payload = payload,
                StreamRevision = revision
            };
            return evt;
        }

        protected static void PersistSomeEvents(IEnumerable<StoreEvent> events)
        {
            Persister.PersistCommit(StreamId, events);
            WaitForNonStaleResults();
        }

        protected static void PersistSomeEvents(params StoreEvent[] events)
        {
            Persister.PersistCommit(StreamId, events);
            WaitForNonStaleResults();
        }
        
        private static void WaitForNonStaleResults()
        {
            while (Store.DatabaseCommands.GetStatistics() .StaleIndexes.Length != 0)
            {
                Thread.Sleep(10);
            }
        }

        protected static void AssertEventInStore(StoreEvent expected)
        {
            WaitForNonStaleResults();
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(), Is.EqualTo(1));
                var evt = events.First();
                Assert.That(evt.Id, Is.EqualTo(expected.Id));
                Assert.That(evt.StreamId, Is.EqualTo(expected.StreamId));
            }
        }

        protected static void AssertEquivalent(Func<IEnumerable<Object>> persisted, params object[] expected)
        {
            WaitForNonStaleResults();
            Assert.That(persisted().ToArray(), Is.EquivalentTo(expected));
        }

        protected static void AssertEventNotInStore(StoreEvent expected)
        {
            WaitForNonStaleResults();
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(e => e.Equals(expected)), Is.EqualTo(0));
            }
        }

        private static StoreEvent[] GetAllPersistedEvents(IDocumentSession session) 
        {
            return session.Query<Commit>()
                          .ToArray()
                          .SelectMany(c => c.Events)
                          .ToArray();
        }

        protected static Commit LoadCommit(int revision) 
        {
            using (var session = Store.OpenSession()) 
            {
                return session.Load<Commit>(Commit.BuildStableId(StreamId, revision));
            }
        }
    }
}