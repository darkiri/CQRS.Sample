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
    [Subject(typeof (EventStore))]
    public class when_creates_stream : event_store_context
    {
        static IEventStream stream;

        Because of = () => stream = Store.OpenStream(Guid.NewGuid());
        It should_return_empty_stream = () => Assert.That(stream.CommittedEvents.Count(), Is.EqualTo(0));
        It should_retrurn_stream_with_revision_0 = () => Assert.That(stream.Revision, Is.EqualTo(0));
    }

    [Subject(typeof (EventStream))]
    public class when_stream_created : event_store_context
    {
        static void SetupPersiter()
        {
            PersisterMock
                .Setup(p => p.GetEvents(StreamId, 0, 1))
                .Returns(new[]
                {
                    Event("first"), Event("second"),
                }.ToStoreEvents(1));
        }

        Establish context = SetupPersiter;

        Because of = () => WithStream(1);
        It should_load_persisted_events = () => Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(2));
    }

    [Subject(typeof (EventStream))]
    public class when_event_appended_to_the_stream : event_store_context
    {
        Because of = () => WithStream(1).Append(Event("second"));

        It should_not_persist_event = () => VerifyPersistEventsCall(0.Times());
        It should_have_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_stream_committed : event_store_context
    {
        Establish context = () => WithStream(0).Append(Event("first"));

        Because of = () => Stream.Commit();

        It should_increment_stream_revision = () => Assert.That(Stream.Revision, Is.EqualTo(1));
        It should_have_no_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        It should_populate_persisted_events = () => Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(1));
        It should_persist_events = () => VerifyPersistEventsCall(1.Times());
        It should_trigger_dispatcher = () => VerifyDispatchCall(1.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_stream_canceled : event_store_context
    {
        Establish context = () => WithStream(10).Append(Event("eleventh"));

        Because of = () =>
        {
            Stream.Cancel();
            Stream.Commit();
        };

        It should_not_increment_stream_revision = () => Assert.That(Stream.Revision, Is.EqualTo(10));
        It should_have_no_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        It should_not_persist_event = () => VerifyPersistEventsCall(0.Times());
        It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_persiter_throws_optimistic_concurrency_exception : event_store_context
    {
        Establish context = () =>
        {
            PersisterMock
                .Setup(p => p.PersistEvents(StreamId, Any<IEnumerable<StoreEvent>>()))
                .Throws<OptimisticConcurrencyException>();
            PersisterMock
                .Setup(p => p.GetEvents(StreamId, 10, Int32.MaxValue))
                .Returns(new[]
                {
                    Event("tenth"),
                    Event("eleventh"),
                    Event("twelwth"),
                }.ToStoreEvents(12));
            WithStream(10)
                .Append(Event("another eleventh"));
        };

        Because of = () => Stream.Commit();

        It should_refresh_revision = () => Assert.That(Stream.Revision, Is.EqualTo(12));
        It should_not_persist_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_events_are_requested : event_store_context
    {
        Because of = () => WithStream(3);
        It should_load_events_from_persistence = () => VerifyLoadingEventsCall(1, 3, 1.Times());
    }


    [Subject(typeof (SimpleDispatcher))]
    public class when_event_dispatched : event_store_context
    {
        static SimpleDispatcher _dispatcher;
        static StoreEvent SomeEvent = new StoreEvent{ Id= Guid.NewGuid(), Body = Event("tenth"), StreamRevision = 10};

        static Mock<IServiceBus> BusMock;

        Establish context = () =>
        {
            BusMock = new Mock<IServiceBus>();
            PersisterMock
                .Setup(p => p.GetUndispatchedEvents())
                .Returns(new[] {SomeEvent});
            _dispatcher = new SimpleDispatcher(PersisterMock.Object, BusMock.Object);
        };

        Because of = () => _dispatcher.Dispatch();

        It should_send_event_to_bus = () => BusMock.Verify(b => b.Publish(SomeEvent.Body));

        It should_request_undispatched_events =
            () => PersisterMock.Verify(p => p.GetUndispatchedEvents(), Times.Once());

        It should_mark_events_as_dispatched = () => PersisterMock.Verify(p => p.MarkAsDispatched(SomeEvent), Times.Once());
    }    
}