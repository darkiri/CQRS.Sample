using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Store;
using Machine.Specifications;
using NUnit.Framework;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class when_persist_events : raven_persistance_context
    {
        private static TestEvent _event = new TestEvent(Guid.NewGuid(), 2, new {A = "123", B = new {B = 456}});

        private Because of = () => Persister.PersistEvents(Guid.NewGuid(), new[] { _event });
        private It should_store_events_in_the_database = () => AssertEventInStore(_event);
    }

    [Subject(typeof (RavenPersister))]
    public class when_persistint_events_for_exisitng_revision : raven_persistance_context
    {
        private static Guid _streamId = Guid.NewGuid();
        private static TestEvent _event1 = new TestEvent(_streamId, 2, 11);
        private static TestEvent _event2 = new TestEvent(_streamId, 2, 22);
        private static TestEvent _event22 = new TestEvent(_streamId, 3, 23);
        private static Exception Exception;
        private Establish context = () => Persister.PersistEvents(_streamId, new[] {_event1});
        private Because of = () => Exception = Catch.Exception(() => Persister.PersistEvents(_streamId, new[] {_event2, _event22}));

        private It should_throw_OptimisticConcurrencyException =
            () => Exception.ShouldBeOfType<OptimisticConcurrencyException>();

        private It should_store_first_event = () => AssertEventInStore(_event1);
        private It should_not_store_second_event = () => AssertEventNotInStore(_event2);
    }

    [Subject(typeof (RavenPersister))]
    public class when_loading_events : raven_persistance_context
    {
        Establish context = () => GenerateEvents();
        Because of = () => _loadedEvents = Persister.GetEvents(_streamId, 1, 2);
        private It should_request_events_from_the_database = () => Assert.That(_loadedEvents, Is.EquivalentTo(_sampleEvents));

        private static Guid _streamId;
        private static IEnumerable<IEvent> _loadedEvents;
        private static List<TestEvent> _sampleEvents;

        private static void GenerateEvents()
        {
            _streamId = Guid.NewGuid();
            _sampleEvents = new List<TestEvent>()
                         {
                             new TestEvent(_streamId, 0, ""),
                             new TestEvent(_streamId, 1, ""),
                             new TestEvent(_streamId, 2, ""),
                             new TestEvent(_streamId, 3, ""),
                         };
            Persister.PersistEvents(_streamId, _sampleEvents);
        }
    }

    [Subject(typeof (RavenPersister))]
    public class when_requesting_undispatched_events : raven_persistance_context
    {
        private It should_request_events_with_flag_undispatched = () => Assert.Inconclusive();
    }

    [Subject(typeof (RavenPersister))]
    public class when_marking_event_as_dispatched : raven_persistance_context
    {
        private It should_reset_undispatched_flag = () => Assert.Inconclusive();
    }
}