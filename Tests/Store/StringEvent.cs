using CQRS.Sample.Store;

namespace CQRS.Sample.Tests.Store {
    public class StringEvent : StoreEvent
    {
        public string Payload { get; set; }

        public override bool Equals(object obj) {
            return obj is StringEvent && base.Equals(obj) && ((StringEvent)obj).Payload == Payload;
        }
    }
}