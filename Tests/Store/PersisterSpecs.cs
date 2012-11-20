using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Store;
using Machine.Specifications;
using NUnit.Framework;
using Raven.Abstractions.Exceptions;
using Raven.Client;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (RavenPersister))]
    public class when_persist_events : raven_persistence_context
    {
        protected static StoreEvent SomeEvent = new StringIntEvent {A = "123", B = 456};

        Because of =()=> PersistSomeEvents(SomeEvent);

        It should_store_events_in_the_database =()=> AssertEventInStore(SomeEvent);
        It should_mark_commit_as_not_dispatched =()=> Assert.False(LoadCommit(0).IsDispatched);
    }

    public class StringIntEvent : StoreEvent
    {
        public string A { get; set; }
        public int B { get; set; }
    }

    [Subject(typeof (RavenPersister))]
    public class when_persisting_events_for_exisitng_revision : raven_persistence_context
    {
        protected static StoreEvent Commit1_Event1 = Event("second", 0);
        protected static StoreEvent Commit2_Event1 = Event("another second", 0);
        protected static StoreEvent Commit2_Event2 = Event("third", 1);
        protected static Exception Exception;

        Establish context =()=> PersistSomeEvents(Commit1_Event1);
        Because of =()=> Exception = Catch.Exception(() => PersistSomeEvents(Commit2_Event1, Commit2_Event2));

        It should_throw_optimistic_concurrency_exception =()=> Exception.ShouldBeOfType<OptimisticConcurrencyException>();
        It should_store_second_event =()=> AssertEventInStore(Commit1_Event1);
        It should_not_store_first_event_from_the_second_commit =()=> AssertEventNotInStore(Commit2_Event1);
        It should_not_store_second_event_from_the_second_commit =()=> AssertEventNotInStore(Commit2_Event2);
    }

    [Subject(typeof (RavenPersister))]
    public class when_loading_events : raven_persistence_context
    {
        protected static StoreEvent FirstEvent = Event("first", 0);
        protected static StoreEvent SecondEvent = Event("second", 1);
        protected static IEnumerable<StoreEvent> LoadedEvents;
        
        Establish context =()=> PersistSomeEvents(FirstEvent, SecondEvent);
        Because of =()=> LoadedEvents = Persister.GetCommits(StreamId, 0, 1)
                                                   .SelectMany(c => c.Events);

        It should_request_events_from_the_database =()=> AssertEquivalent(() => LoadedEvents, FirstEvent, SecondEvent);
    }

    [Subject(typeof (RavenPersister))]
    public class when_marking_commit_as_dispatched : raven_persistence_context {
        protected static Commit Commit;

        Establish context = () => {
            PersistSomeEvents(Event("root", 0));
            Commit = Persister.GetUndispatchedCommits().First();
        };
        Because of =()=> Persister.MarkAsDispatched(Commit);

        It should_reset_undispatched_flag =()=> Assert.True(LoadCommit(0).IsDispatched);
    }

    [Subject(typeof (RavenPersister))]
    public class when_requesting_undispatched_commits : raven_persistence_context
    {
        protected static StoreEvent FirstEvent = Event("first", 0);
        protected static StoreEvent SecondEvent = Event("second", 1);
        protected static IEnumerable<Commit> UndispatchedCommits;

        Establish context =()=> {
            PersistSomeEvents(FirstEvent, SecondEvent);
            Persister.MarkAsDispatched(LoadCommit(0));
        };
        Because of =()=> UndispatchedCommits = Persister.GetUndispatchedCommits();

        It should_return_commits_with_undispatched_flag =()=> Assert.That(UndispatchedCommits.Count(),Is.EqualTo(0));
    }


    [Subject("Raven Experiments")]
    public class when_storing_two_objects_with_same_id_in_separate_sessions : raven_persistence_context
    {
        protected static Exception Exception;

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
        protected static Exception Exception;

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