using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Bus;
using CQRS.Sample.Store;
using CQRS.Sample.Tests.Bus;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using It = Machine.Specifications.It;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof(EventStore))]
    public class when_creates_stream : event_store_context
    {
        Because of =()=> Stream = Store.OpenStream(Guid.NewGuid());

        It should_return_empty_stream =()=> Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(0));
        It should_return_stream_with_revision_0 =()=> Assert.That(Stream.Revision, Is.EqualTo(0));
    }

    [Subject(typeof(EventStream))]
    public class when_stream_created : event_store_context
    {
        Establish context =()=> PersisterMock
                                      .Setup(p => p.GetCommits(StreamId, 0, 3))
                                      .Returns(AsCommit(Event("first"), Event("second")));
        Because of =()=> Stream = new EventStream(PersisterMock.Object, DispatcherMock.Object, StreamId, 3);

        It should_load_commited_events =()=> Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(2));
        It should_set_stream_revision =()=> Assert.That(Stream.Revision, Is.EqualTo(2));
    }

    [Subject(typeof(EventStream))]
    public class when_events_are_requested : event_store_context
    {
        Because of =()=> WithStream(3);
        It should_load_events_from_persistence =()=> VerifyLoadingEventsCall(0, 3, 1.Times());
    }

    [Subject(typeof(EventStream))]
    public class when_event_appended_to_a_stream : event_store_context 
    {

        Establish context =()=> WithStream(1);
        Because of =()=> Stream.Append(Event("second"));

        It should_not_persist_event =()=> VerifyPersistEventsCall(0.Times());
        It should_have_uncommitted_event =()=> Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        It should_not_trigger_dispatcher =()=> VerifyDispatchCall(0.Times());
    }

    [Subject(typeof(EventStream))]
    public class when_event_from_another_stream_appended : event_store_context
    {
        protected static EventStream AnotherStream;

        Establish context =() =>
        {
            Stream = WithStream(1);
            AnotherStream = new EventStream(PersisterMock.Object, DispatcherMock.Object, Guid.NewGuid(), 1);
        };
        Because of =()=> 
        {
            AnotherStream.Append(Event("second"));
            AnotherStream.Commit();
        };

        It should_not_persist_event =()=> VerifyPersistEventsCall(0.Times());
        It should_have_no_uncommitted_event =()=> Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
    }

    [Subject(typeof(EventStream))]
    public class when_stream_committed : event_store_context
    {
        Establish context =()=> WithStream(0).Append(Event("first"));

        Because of =()=> Stream.Commit();

        It should_increment_stream_revision =()=> Assert.That(Stream.Revision, Is.EqualTo(1));
        It should_have_no_uncommitted_event =()=> Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        It should_populate_committed_events =()=> Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(1));
        It should_persist_events =()=> VerifyPersistEventsCall(1.Times());
        It should_trigger_dispatcher =()=> VerifyDispatchCall(1.Times());
    }

    [Subject(typeof(EventStream))]
    public class when_stream_canceled : event_store_context 
    {
        private Establish context =()=> 
        {
            PersisterMock
                .Setup(p => p.GetCommits(Any<Guid>(), 0, 10))
                .Returns(() => AsCommit(Enumerable.Range(1, 10)
                                                  .Select(i => Event(i.ToString()))
                                                  .ToArray()));
            WithStream(10).Append(Event("eleventh"));
        };

        Because of =()=>
        {
            Stream.Cancel();
            Stream.Commit();
        };

        It should_not_increment_stream_revision =()=> Assert.That(Stream.Revision, Is.EqualTo(10));
        It should_have_no_uncommitted_event =()=> Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        It should_not_persist_event =()=> VerifyPersistEventsCall(0.Times());
        It should_not_trigger_dispatcher =()=> VerifyDispatchCall(0.Times());
    }

    [Subject(typeof(EventStream))]
    public class when_persiter_throws_optimistic_concurrency_exception : event_store_context
    {
        Establish context =()=>
        {
            PersisterMock
                .Setup(p => p.GetCommits(StreamId, 0, Int32.MaxValue))
                .Returns(AsCommit(Event("one"), Event("two"), Event("three")));
            PersisterMock
                .Setup(p => p.PersistCommit(StreamId, Any<IEnumerable<StoreEvent>>()))
                .Throws<OptimisticConcurrencyException>();
            WithStream(0).Append(Event("another one"));
        };

        Because of =()=> Stream.Commit();

        It should_refresh_revision =()=> Assert.That(Stream.Revision, Is.EqualTo(3));
        It should_not_persist_event =()=> Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        It should_not_trigger_dispatcher =()=> VerifyDispatchCall(0.Times());
    }


    [Subject(typeof(SimpleDispatcher))]
    public class when_event_dispatched : event_store_context
    {
        static SimpleDispatcher _dispatcher;
        protected static StoreEvent SomeEvent = Event("tenth");
        private static readonly Commit Commit = new Commit(new[] { SomeEvent });

        protected static Mock<IServiceBus> BusMock;

        Establish context =()=>
        {
            BusMock = new Mock<IServiceBus>();
            PersisterMock
                .Setup(p => p.GetUndispatchedCommits())
                .Returns(new []{Commit});
            _dispatcher = new SimpleDispatcher(PersisterMock.Object, BusMock.Object);
        };

        Because of =()=> _dispatcher.Dispatch();

        It should_send_event_to_bus =()=> BusMock.Verify(b => b.Publish(SomeEvent));
        It should_request_undispatched_events = () => PersisterMock.Verify(p => p.GetUndispatchedCommits(), Times.Once());

        It should_mark_events_as_dispatched =()=> PersisterMock.Verify(p => p.MarkAsDispatched(Commit), Times.Once());
    }
}