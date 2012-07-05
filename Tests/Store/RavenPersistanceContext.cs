using System.Collections.Generic;
using System.Linq;
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
                                        Register.Indexes(Store);
                                        Persister = new RavenPersister(Store);
                                    };

        private Cleanup all = () => Store.Dispose();

        protected static void AssertEventInStore(TestEvent expected)
        {
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(), Is.EqualTo(1));
                var evt = events.First();
                Assert.That(evt.StreamId, Is.EqualTo(expected.StreamId));
                Assert.That(evt.StreamRevision, Is.EqualTo(expected.StreamRevision));
                Assert.That(evt.Body, Is.EqualTo(expected.Body));
            }
        }

        protected static void AssertEventNotInStore(TestEvent expected)
        {
            using (var session = Store.OpenSession())
            {
                var events = GetAllPersistedEvents(session);
                Assert.That(events.Count(e => e.Id == expected.Id), Is.EqualTo(0));
            }
        }

        private static IEnumerable<IEvent> GetAllPersistedEvents(IDocumentSession session)
        {
            var events = session
                .Query<RavenPersister.Commit>()
                .Where(c => c.Enabled)
                .ToArray()
                .SelectMany(c => c.Events);
            return events;
        }
    }
}