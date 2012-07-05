using System;
using System.Collections.Generic;

namespace CQRS.Sample.Store
{
    public interface IPersister
    {
        void PersistEvents(Guid streamId, IEnumerable<StoreEvent> events);
        IEnumerable<StoreEvent> GetEvents(Guid streamId, int minRevision, int maxRevision);
        IEnumerable<StoreEvent> GetUndispatchedEvents();
        void MarkAsDispatched(StoreEvent evt);
    }

    public class OptimisticConcurrencyException : Exception { }
}