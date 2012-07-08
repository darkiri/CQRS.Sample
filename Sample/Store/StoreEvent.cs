using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    public class StoreEvent : IMessage, IEquatable<StoreEvent>
    {
        public Guid Id { get; set; }
        public int StreamRevision { get; set; }
        public bool IsDispatched { get; set; }
        
        public object Body { get; set; }

        public bool Equals(StoreEvent other)
        {
            return Id == other.Id;
        }
    }
}