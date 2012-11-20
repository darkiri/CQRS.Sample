using System;
using System.Collections.Generic;

namespace CQRS.Sample.Store
{
    public interface IPersister
    {
        void PersistCommit(Guid streamId, IEnumerable<StoreEvent> events);
        IEnumerable<Commit> GetCommits(Guid streamId, int minRevision, int maxRevision);
        IEnumerable<Commit> GetUndispatchedCommits();
        void MarkAsDispatched(Commit c);
    }

    public class OptimisticConcurrencyException : Exception { }
}