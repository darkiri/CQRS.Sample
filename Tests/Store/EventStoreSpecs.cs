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
        private static IEventStream _stream;

        private Because of = () => _stream = Store.OpenStream(Guid.NewGuid());
        private It should_return_empty_stream = () => Assert.That(_stream.CommittedEvents.Count(), Is.EqualTo(0));
        private It should_retrurn_stream_with_revision_0 = () => Assert.That(_stream.Revision, Is.EqualTo(0));
    }

    [Subject(typeof (EventStream))]
    public class when_stream_created : event_store_context
    {
        private Establish context = () => PersisterMock
                                              .Setup(p => p.GetEvents(Any<Guid>(), 0, 1))
                                              .Returns(new[]
                                                       {
                                                           new TestEvent(Guid.NewGuid(), 0, "Definitely"),
                                                           new TestEvent(Guid.NewGuid(), 1, "Definitely"),
                                                       });

        private Because of = () => WithStream(Guid.NewGuid(), 1);
        private It should_load_persisted_events = () => Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(2));
    }

    [Subject(typeof (EventStream))]
    public class when_event_appended_to_the_stream : event_store_context
    {
        private Because of = () => WithStream(Guid.NewGuid(), 1).Append(new TestEvent(Guid.NewGuid(), 2, "Definitely"));

        private It should_not_persist_event = () => VerifyPersistEventsCall(0.Times());
        private It should_have_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        private It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_stream_committed : event_store_context
    {
        private Establish context = () => WithStream(Guid.NewGuid(), 0).Append(new TestEvent(Guid.NewGuid(), 1, "Definitely"));
        private Because of = () => Stream.Commit();

        private It should_increment_stream_revision = () => Assert.That(Stream.Revision, Is.EqualTo(1));
        private It should_have_no_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        private It should_populate_persisted_events = () => Assert.That(Stream.CommittedEvents.Count(), Is.EqualTo(1));
        private It should_persist_events = () => VerifyPersistEventsCall(1.Times());
        private It should_trigger_dispatcher = () => VerifyDispatchCall(1.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_stream_canceled : event_store_context
    {
        private static readonly IEvent Event = new TestEvent(Guid.NewGuid(), 11, "Definitely");
        private Establish context = () => WithStream(Guid.NewGuid(), 10).Append(Event);

        private Because of = () =>
                             {
                                 Stream.Cancel();
                                 Stream.Commit();
                             };

        private It should_not_increment_stream_revision = () => Assert.That(Stream.Revision, Is.EqualTo(10));
        private It should_have_no_uncommitted_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(0));
        private It should_not_persist_event = () => VerifyPersistEventsCall(0.Times());
        private It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStream))]
    public class when_persiter_throws_optimistic_concurrency_exception : event_store_context
    {
        private static IEvent Event = new TestEvent(Guid.NewGuid(), 11, "Definitely");

        private Establish context = () =>
                                    {
                                        PersisterMock
                                            .Setup(p => p.PersistEvents(Event.StreamId, Any<IEnumerable<IEvent>>()))
                                            .Throws<OptimisticConcurrencyException>();
                                        PersisterMock
                                            .Setup(p => p.GetEvents(Any<Guid>(), 10, Int32.MaxValue))
                                            .Returns(new[]
                                                     {
                                                         new TestEvent(Guid.NewGuid(), 10, "tenth"),
                                                         new TestEvent(Guid.NewGuid(), 11, "eleventh"),
                                                         new TestEvent(Guid.NewGuid(), 12, "twelwth"),
                                                     });
                                        WithStream(Event.StreamId, 10).Append(Event);
                                    };

        private Because of = () => Stream.Commit();
        private It should_refresch_revision = () => Assert.That(Stream.Revision, Is.EqualTo(12));
        private It should_not_persist_event = () => Assert.That(Stream.UncommittedEvents.Count(), Is.EqualTo(1));
        private It should_not_trigger_dispatcher = () => VerifyDispatchCall(0.Times());
    }

    [Subject(typeof (EventStore))]
    public class when_events_are_requested : event_store_context
    {
        private static Guid StreamId = Guid.NewGuid();
        private Because of = () => Store.GetEvents(StreamId, 1, 3);

        private It should_load_events_from_persistence =
            () => PersisterMock.Verify(p => p.GetEvents(StreamId, 1, 3), Times.Exactly(1));
    }


    [Subject(typeof (SimpleDispatcher))]
    public class when_event_dispatched : event_store_context
    {
        private static SimpleDispatcher _dispatcher;
        private static TestEvent _event1 = new TestEvent(Guid.NewGuid(), 10, "tenth");

        private static Mock<IServiceBus> BusMock;

        private Establish context = () =>
                                    {
                                        BusMock = new Mock<IServiceBus>();
                                        PersisterMock
                                            .Setup(p => p.GetUndispatchedEvents())
                                            .Returns(new[] {_event1});
                                        _dispatcher = new SimpleDispatcher(PersisterMock.Object, BusMock.Object);
                                    };

        private Because of = () => _dispatcher.Dispatch();
        private It should_send_event_to_bus = () => BusMock.Verify(b => b.Publish(_event1));

        private It should_request_undispatched_events =
            () => PersisterMock.Verify(p => p.GetUndispatchedEvents(), Times.Once());

        private It should_mark_events_as_dispatched =
            () => PersisterMock.Verify(p => p.MarkAsDispatched(_event1), Times.Once());
    }

    public class TestEvent : IEvent
    {
        public TestEvent(Guid id, int revision, object body)
        {
            StreamId = id;
            StreamRevision = revision;
            Body = body;
            Id = Guid.NewGuid();
        }

        public int StreamRevision { get; private set; }
        public Guid StreamId { get; private set; }
        public Guid Id { get; private set; }
        public object Body { get; private set; }
    }
}