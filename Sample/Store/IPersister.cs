using System;
using System.Collections.Generic;

namespace CQRS.Sample.Store
{
    public interface IPersister
    {
        void PersistEvents(Guid streamId, IEnumerable<IEvent> events);
        IEnumerable<IEvent> GetEvents(Guid streamId, int minRevision, int maxRevision);
        IEnumerable<IEvent> GetUndispatchedEvents();
        void MarkAsDispatched(IEvent evt);
    }

    public class OptimisticConcurrencyException : Exception { }
}