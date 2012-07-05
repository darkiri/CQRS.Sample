using System;
using System.Collections.Generic;

namespace CQRS.Sample.Store
{
    public interface IEventStream
    {
        Guid StreamId { get; }
        int Revision { get; }
        IEnumerable<StoreEvent> UncommittedEvents { get; }
        IEnumerable<StoreEvent> CommittedEvents { get; }
        void Append(StoreEvent evt);
        void Commit();
        void Cancel();
    }
}