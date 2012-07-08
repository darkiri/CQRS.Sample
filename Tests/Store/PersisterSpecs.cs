﻿using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Store;
using Machine.Specifications;
using Raven.Abstractions.Exceptions;
using Raven.Client;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class when_persist_events : raven_persistance_context
    {
        static StoreEvent SomeEvent = Event(2, new {A = "123", B = new {B = 456}});

        Because of = () => Persister.PersistEvents(StreamId, new[] {SomeEvent});
        It should_store_events_in_the_database = () => AssertEventInStore(SomeEvent);
    }

    [Subject(typeof (RavenPersister))]
    public class when_persisting_events_for_exisitng_revision : raven_persistance_context
    {
        static StoreEvent second = Event(2, "second");
        static StoreEvent third = Event(3, "third");
        static StoreEvent anotherSecond = Event(2, "another second");
        static Exception Exception;

        Establish context = () => Persister.PersistEvents(StreamId, new[] {second});

        Because of =
            () => Exception = Catch.Exception(() => Persister.PersistEvents(StreamId, new[] {anotherSecond, third}));

        It should_throw_optimistic_concurrency_exception =
            () => Exception.ShouldBeOfType<OptimisticConcurrencyException>();

        It should_store_second_event = () => AssertEventInStore(second);
        It should_not_store_another_second_event = () => AssertEventNotInStore(anotherSecond);
    }

    [Subject(typeof (RavenPersister))]
    public class when_loading_events : raven_persistance_context
    {
        static StoreEvent FirstEvent = Event(1, "first");
        static StoreEvent SecondEvent = Event(2, "second");

        static List<StoreEvent> SampleEvents = new List<StoreEvent>
                                               {
                                                   Event(0, "zero"),
                                                   FirstEvent,
                                                   SecondEvent,
                                                   Event(3, "third"),
                                               };

        Because of = () => Persister.PersistEvents(StreamId, SampleEvents);

        It should_request_events_from_the_database =
            () => AssertEquivalent(() => Persister.GetEvents(StreamId, 1, 2), FirstEvent, SecondEvent);
    }

    [Subject(typeof (RavenPersister))]
    public class when_marking_event_as_dispatched : raven_persistance_context
    {
        static StoreEvent TheEvent = Event(0, "root");
        Establish context = () => PersistSomeEvents(new[] {TheEvent});
        Because of = () => Persister.MarkAsDispatched(TheEvent);

        It should_reset_undispatched_flag =
            () => AssertEqual(() => Persister.GetEvents(StreamId, 0, 0).First().IsDispatched, true);
    }

    [Subject(typeof (RavenPersister))]
    public class when_requesting_undispatched_events : raven_persistance_context
    {
        static StoreEvent FirstEvent = Event(1, "first");
        static StoreEvent SecondEvent = Event(2, "second");

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
    public class when_storing_two_objects_with_same_id_in_separate_sessions : raven_persistance_context
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
            session.Store(new MyClass {Id = "1", Text = text});
            session.SaveChanges();
        }

        public class MyClass
        {
            public string Id { get; set; }
            public string Text;
        }
    }

    [Subject("Raven Experiments")]
    public class when_updating_same_object_in_separate_sessions : raven_persistance_context
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