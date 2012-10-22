using System;
using System.Collections.Generic;
using CQRS.Sample.Events;

namespace CQRS.Sample.Store
{
    public interface IEventStream : IUnitOfWork
    {
        Guid StreamId { get; }
        int Revision { get; }
        IEnumerable<StoreEvent> UncommittedEvents { get; }
        IEnumerable<StoreEvent> CommittedEvents { get; }
        void Append(IEvent evt);
        IEnumerable<IEvent> GetEvents(int minRevision, int maxRevision);
    }
}