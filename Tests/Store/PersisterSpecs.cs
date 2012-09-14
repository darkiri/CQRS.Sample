using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Events;
using CQRS.Sample.Store;
using Machine.Specifications;
using Raven.Abstractions.Exceptions;
using Raven.Client;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class when_persist_events : raven_persistence_context
    {
        static StoreEvent SomeEvent = AsStoreEvent(new StringIntEvent {A = "123", B = 456});
        Because of = () => PersistSomeEvents(new[] {SomeEvent});
        It should_store_events_in_the_database = () => AssertEventInStore(SomeEvent);
    }

        public class StringIntEvent : IEvent, IEquatable<StringIntEvent>
        {
            public int Version { get; private set; }
            public string A { get; set; }
            public int B { get; set; }

            public bool Equals(StringIntEvent other)
            {
                return Version == other.Version && string.Equals(A, other.A) && B == other.B;
            }
        }

    [Subject(typeof (RavenPersister))]
    public class when_persisting_events_for_exisitng_revision : raven_persistence_context
    {
        static StoreEvent second = AsStoreEvent(Event(2, "second"));
        static StoreEvent third = AsStoreEvent(Event(3, "third"));
        static StoreEvent anotherSecond = AsStoreEvent(Event(2, "another second"));
        static Exception Exception;

        Establish context = () => PersistSomeEvents(new[] {second});

        Because of =
            () => Exception = Catch.Exception(() => PersistSomeEvents(new[] {anotherSecond, third}));

        It should_throw_optimistic_concurrency_exception =
            () => Exception.ShouldBeOfType<OptimisticConcurrencyException>();

        It should_store_second_event = () => AssertEventInStore(second);
        It should_not_store_another_second_event = () => AssertEventNotInStore((IdentifiableEvent)anotherSecond.Body);
    }

    [Subject(typeof (RavenPersister))]
    public class when_loading_events : raven_persistence_context
    {
        static StoreEvent FirstEvent = AsStoreEvent(Event(1, "first"));
        static StoreEvent SecondEvent = AsStoreEvent(Event(2, "second"));

        static List<StoreEvent> SampleEvents = new List<StoreEvent>
                                               {
                                                   AsStoreEvent(Event(0, "zero")),
                                                   FirstEvent,
                                                   SecondEvent,
                                                   AsStoreEvent(Event(3, "third")),
                                               };

        Because of = () => PersistSomeEvents(SampleEvents);

        It should_request_events_from_the_database =
            () => AssertEquivalent(() => Persister.GetEvents(StreamId, 1, 2), FirstEvent, SecondEvent);
    }

    [Subject(typeof (RavenPersister))]
    public class when_marking_event_as_dispatched : raven_persistence_context
    {
        static StoreEvent TheEvent = AsStoreEvent(Event(0, "root"));
        Establish context = () => PersistSomeEvents(new[] {TheEvent});
        Because of = () => Persister.MarkAsDispatched(TheEvent);

        It should_reset_undispatched_flag =
            () => AssertEqual(() => Persister.GetEvents(StreamId, 0, 0).First().IsDispatched, true);
    }

    [Subject(typeof (RavenPersister))]
    public class when_requesting_undispatched_events : raven_persistence_context
    {
        static StoreEvent FirstEvent = AsStoreEvent(Event(1, "first"));
        static StoreEvent SecondEvent = AsStoreEvent(Event(2, "second"));

        static List<StoreEvent> SampleEvents = new List<StoreEvent>
                                               {
                                                   FirstEvent,
                                                   SecondEvent,
                                               };

        Establish context = () => PersistSomeEvents(SampleEvents);
        Because of = () => Persister.MarkAsDispatched(FirstEvent);

        It should_request_events_with_flag_undispatched =
            () => AssertEqual(() => Persister.GetUndispatchedEvents().First(), SecondEvent);
    }


    [Subject("Raven Experiments")]
    public class when_storing_two_objects_with_same_id_in_separate_sessions : raven_persistence_context
    {
        static Exception Exception;

        Because of = () => Exception = Catch.Exception(CreateTwoObjectsInTwoSessions);
        It should_throw_concurrency_exception = () => Exception.ShouldBeOfType<ConcurrencyException>();

        static void CreateTwoObjectsInTwoSessions()
        {
            using (var session = Store.OpenSession())
            {
                StoreOnce(session, "first");
            }
            using (var session = Store.OpenSession())
            {
                StoreOnce(session, "another first");
            }
        }

        static void StoreOnce(IDocumentSession session, string text)
        {
            session.Advanced.UseOptimisticConcurrency = true;
            session.Store(new MyEvent {Revision = 1, Payload = text});
            session.SaveChanges();
        }

        public class MyEvent
        {
            public int Id { get { return Revision; } }
            public int Revision { get; set; }
            public string Payload { get; set; }
        }
    }

    [Subject("Raven Experiments")]
    public class when_updating_same_object_in_separate_sessions : raven_persistence_context
    {
        static Exception Exception;

        Establish context = () => StoreSingleObject(new MyClass {Text = "this one"});
        Because of = () => Exception = Catch.Exception(UpdateTwoObjectsInTwoSessions);
        It should_throw_concurrency_exception = () => Exception.ShouldBeOfType<ConcurrencyException>();

        static void UpdateTwoObjectsInTwoSessions()
        {
            using (var session = Store.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                session
                    .Query<MyClass>()
                    .First()
                    .Text = "still one";

                AtTheSameTimeInAnotherSession();

                session.SaveChanges();
            }
        }

        static void AtTheSameTimeInAnotherSession()
        {
            // single thread is enough
            using (var anotherSession = Store.OpenSession())
            {
                anotherSession
                    .Query<MyClass>()
                    .First()
                    .Text = "ha ha";
                anotherSession.SaveChanges();
            }
        }

        static void StoreSingleObject(MyClass obj)
        {
            using (var session = Store.OpenSession())
            {
                session.Store(obj);
                session.SaveChanges();
            }
        }

        public class MyClass
        {
            public string Id { get; set; }
            public string Text;
        }
    }
}