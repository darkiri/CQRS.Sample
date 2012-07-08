using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CQRS.Sample.Store;
using Machine.Specifications;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class raven_persistance_context : event_based_context
    {
        protected static IDocumentStore Store;
        protected static RavenPersister Persister;

        private Establish context = () =>
                                    {
                                        Store = new EmbeddableDocumentStore {RunInMemory = true}.Initialize();
                                        Persister = new RavenPersister(Store);
                                    };

        private Cleanup all = () => Store.Dispose();

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
                Assert.That(evt.StreamRevision, Is.EqualTo(expected.StreamRevision));
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

        protected static void AssertEventNotInStore(StoreEvent expected)
        {
            WaitForNonStaleResults();
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(e => e.Id == expected.Id), Is.EqualTo(0));
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
    }
}