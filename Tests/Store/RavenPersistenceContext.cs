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
    public class raven_persistence_context : event_based_context
    {
        protected static IDocumentStore Store;
        protected static DocumentStoreConfiguration StoreConfig;
        protected static RavenPersister Persister;

        private Establish context = () =>
        {
            StoreConfig = Bootstrapper.InMemory();
            Store = StoreConfig.EventStore;
            Persister = new RavenPersister(StoreConfig);
        };

        private Cleanup all = () => StoreConfig.Dispose();

        protected static void PersistSomeEvents(IEnumerable<StoreEvent> events)
        {
            Persister.PersistEvents(StreamId, events);
            WaitForNonStaleResults();
        }

        static void WaitForNonStaleResults()
        {
            while(Store.DatabaseCommands.GetStatistics().StaleIndexes.Length != 0)
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
                Assert.That(evt.Body, Is.EqualTo(expected.Body));
            }
        }

        protected static void AssertEqual(Func<Object> persisted, object expected)
        {
            WaitForNonStaleResults();
            Assert.That(persisted(), Is.EqualTo(expected));
        }

        protected static void AssertEquivalent(Func<IEnumerable<Object>> persisted, params object[] expected)
        {
            WaitForNonStaleResults();
            Assert.That(persisted().ToArray(), Is.EquivalentTo(expected));
        }

        protected static void AssertEventNotInStore(IdentifiableEvent expected)
        {
            WaitForNonStaleResults();
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(e => ((IdentifiableEvent)e.Body).Id == expected.Id), Is.EqualTo(0));
            }
        }

        private static IEnumerable<StoreEvent> GetAllPersistedEvents(IDocumentSession session)
        {
            var events = session
                .Query<RavenPersister.Commit>()
                .ToArray()
                .SelectMany(c => c.Events);
            return events;
        }

        protected static StoreEvent AsStoreEvent(IEvent evt, int version)
        {
            return new StoreEvent
            {
                Id = Guid.NewGuid(),
                StreamRevision = version,
                Body = evt,
            };
        }
    }
}