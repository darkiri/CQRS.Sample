using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    /// <summary>
    /// Messages that can be stored as an event stream
    /// </summary>
    public class StoreEvent : IMessage
    {
        public Guid StreamId { get; set; }
        public int StreamRevision { get; set; }

        public String Id
        {
            get { return String.Format("events/{0}/{1}", StreamId, StreamRevision); }
        }

        public override bool Equals(object obj)
        {
            return obj is StoreEvent && ((StoreEvent)obj).Id == Id;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}