using System;
using System.Collections.Generic;
using CQRS.Sample.Store;
using Machine.Specifications;
using Moq;

namespace CQRS.Sample.Tests.Store
{
    [Subject(typeof (EventStore))]
    public class event_store_context
    {
        protected static Mock<ICommitDispatcher> DispatcherMock;
        protected static Mock<IPersister> PersisterMock;
        protected static EventStore Store;
        public static IEventStream Stream;

        Establish context = () =>
                            {
                                PersisterMock = new Mock<IPersister>();
                                DispatcherMock = new Mock<ICommitDispatcher>();
                                Store = new EventStore(PersisterMock.Object, DispatcherMock.Object);
                            };

        public static IEventStream WithStream(Guid streamId, int revision)
        {
            Stream = new EventStream(PersisterMock.Object, DispatcherMock.Object, streamId, revision);
            return Stream;
        }

        protected static void VerifyPersistEventsCall(int times)
        {
            PersisterMock.Verify(p => p.PersistEvents(Stream.StreamId, Moq.It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(times));
        }

        protected static void VerifyDispatchCall(int times)
        {
            DispatcherMock.Verify(d => d.Dispatch(), Times.Exactly(times));
        }

        protected static void VerifyUnitOfWork<TUnitOfWork>(Mock<TUnitOfWork> mock, int times)
            where TUnitOfWork : class, IUnitOfWork
        {
            mock.Verify(uof => uof.Commit(), Times.Exactly(times));
        }

        protected static T Any<T>()
        {
            return Moq.It.IsAny<T>();
        }
    }
}