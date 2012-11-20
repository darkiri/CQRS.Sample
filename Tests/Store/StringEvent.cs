using System;
using CQRS.Sample.Store;

namespace CQRS.Sample.Tests.Store {
    public class StringEvent : StoreEvent
    {
        public string Payload { get; set; }

        protected bool Equals(StringEvent other)
        {
            return base.Equals(other) && string.Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) && Payload == ((StringEvent)obj).Payload;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Payload != null ? Payload.GetHashCode() : 0);
            }
        }
    }
}