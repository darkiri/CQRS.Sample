using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Events;

namespace CQRS.Sample.Store
{
    /// <summary>
    /// Oversimplified version of the  Jonathan Oliver's EventStore
    /// No snapshotting, no transactions 
    /// </summary>
    public class EventStore
    {
        readonly IPersister _persister;
        readonly ICommitDispatcher _commitDispatcher;

        public EventStore(IPersister persister, ICommitDispatcher commitDispatcher)
        {
            _persister = persister;
            _commitDispatcher = commitDispatcher;
        }

        public IEventStream OpenStream(Guid streamId)
        {
            return new EventStream(_persister, _commitDispatcher, streamId, 0);
        }
   }
}