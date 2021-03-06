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
        protected static readonly Guid StreamId = Guid.Parse("90AEA96E-C0A5-4CDF-9272-8A22986AC737");
        protected static Mock<ICommitDispatcher> DispatcherMock;
        protected static Mock<IPersister> PersisterMock;
        protected static EventStore Store;
        protected static IEventStream Stream;

        Establish context = () =>
                            {
                                PersisterMock = new Mock<IPersister>();
                                DispatcherMock = new Mock<ICommitDispatcher>();
                                Store = new EventStore(PersisterMock.Object, DispatcherMock.Object);
                            };

        protected static StoreEvent Event(string payload)
        {
            return new StringEvent
                   {
                       Payload = payload,
                   };
        }

        public static IEventStream WithStream(int revision)
        {
            Stream = new EventStream(PersisterMock.Object, DispatcherMock.Object, StreamId, revision);
            return Stream;
        }

        protected static void VerifyPersistEventsCall(int times)
        {
            PersisterMock.Verify(p => p.PersistCommit(StreamId, Moq.It.IsAny<IEnumerable<StoreEvent>>()), Times.Exactly(times));
        }

        protected static void VerifyLoadingEventsCall(int minRevision, int maxRevision, int times)
        {
            PersisterMock.Verify(p => p.GetCommits(StreamId, minRevision, maxRevision), Times.Exactly(times));
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

        protected static Commit[] AsCommit(params StoreEvent[] events)
        {
            return new[] {new Commit(events)};
        }
    }
}