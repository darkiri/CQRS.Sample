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
        static TestEvent SomeEvent = Event(2, new {A = "123", B = new {B = 456}});

        Because of = () => Persister.PersistEvents(StreamId, new[] {SomeEvent});
        It should_store_events_in_the_database = () => AssertEventInStore(SomeEvent);
    }

    [Subject(typeof (RavenPersister))]
    public class when_persisting_events_for_exisitng_revision : raven_persistance_context
    {
        static TestEvent second = Event(2, "second");
        static TestEvent third = Event(3, "third");
        static TestEvent anotherSecond = Event(2, "another second");
        static Exception Exception;

        Establish context = () => Persister.PersistEvents(StreamId, new[] {second});

        Because of = () => Exception = Catch.Exception(() => Persister.PersistEvents(StreamId, new[] {anotherSecond, third}));

        It should_throw_optimistic_concurrency_exception = () => Exception.ShouldBeOfType<OptimisticConcurrencyException>();
        It should_store_second_event = () => AssertEventInStore(second);
        It should_not_store_another_second_event = () => AssertEventNotInStore(anotherSecond);
    }

    [Subject(typeof (RavenPersister))]
    public class when_loading_events : raven_persistance_context
    {
        static TestEvent FirstEvent = Event(1, "first");
        static TestEvent SecondEvent = Event(2, "second");

        static List<TestEvent> SampleEvents = new List<TestEvent>
        {
            Event(0, "zero"),
            FirstEvent,
            SecondEvent,
            Event(3, "third"),
        };
        static IEnumerable<IEvent> LoadedEvents;

        Establish context = () => Persister.PersistEvents(StreamId, SampleEvents);
        Because of = () => LoadedEvents = Persister.GetEvents(StreamId, 1, 2).ToArray();

        It should_request_events_from_the_database = () => Assert.That(LoadedEvents, Is.EquivalentTo(new[] {FirstEvent, SecondEvent}));
    }

    [Subject(typeof (RavenPersister))]
    public class when_requesting_undispatched_events : raven_persistance_context
    {
        It should_request_events_with_flag_undispatched = () => Assert.Inconclusive();
    }

    [Subject(typeof (RavenPersister))]
    public class when_marking_event_as_dispatched : raven_persistance_context
    {
        It should_reset_undispatched_flag = () => Assert.Inconclusive();
    }
}