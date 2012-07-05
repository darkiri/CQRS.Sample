using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    public class StoreEvent : IMessage, IEquatable<StoreEvent>
    {
        public int StreamRevision { get; set; }
        public Guid StreamId { get; set; }
        public Guid Id { get; set; }
        public object Body { get; set; }
        public bool IsDispatched { get; set; }

        public bool Equals(StoreEvent other)
        {
            return Id == other.Id;
        }
    }
}