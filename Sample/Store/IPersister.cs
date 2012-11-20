using System;
using System.Collections.Generic;
using Raven.Abstractions.Exceptions;

namespace CQRS.Sample.Store
{
    /// <summary>
    /// Commits a logical set of events to the persistent store.
    /// No need for the <see cref="IUnitOfWork"/> here, enough that the <see cref="PersistCommit"/> is an atomic operation
    /// Cannot use <see cref="IUnitOfWork"/> here since the <see cref="Commit"/> has own additional semantic
    /// like dispatched/not dispatched
    /// </summary>
    public interface IPersister
    {
        void PersistCommit(Guid streamId, IEnumerable<StoreEvent> events);
        IEnumerable<Commit> GetCommits(Guid streamId, int minRevision, int maxRevision);
        IEnumerable<Commit> GetUndispatchedCommits();
        void MarkAsDispatched(Commit c);
    }

    public class OptimisticConcurrencyException : Exception { }
}