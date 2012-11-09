using System;
using CQRS.Sample.Store;

namespace CQRS.Sample.Events
{
    public abstract class DomainEvent : IEvent
    {
        protected DomainEvent(Guid streamId)
        {
            StreamId = streamId;
        }

        public Guid StreamId { get; protected set; }
    }
}