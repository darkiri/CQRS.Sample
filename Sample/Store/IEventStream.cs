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
        IEnumerable<IEvent> CommittedEvents { get; }
        void Append(IEvent evt);
    }
}