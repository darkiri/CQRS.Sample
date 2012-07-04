using System;
using System.Collections.Generic;

namespace CQRS.Sample.Store
{
    public interface IEventStream
    {
        Guid StreamId { get; }
        int Revision { get; }
        IEnumerable<IEvent> UncommittedEvents { get; }
        IEnumerable<IEvent> CommittedEvents { get; }
        void Append(IEvent evt);
        void Commit();
        void Cancel();
    }
}