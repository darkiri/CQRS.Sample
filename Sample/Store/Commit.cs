using System;
using System.Collections.Generic;
using System.Linq;

namespace CQRS.Sample.Store {
    /// <summary>
    /// Unit of Work for Event Store as a persistance unit
    /// </summary>
    public class Commit {
        public Commit(IEnumerable<StoreEvent> events)
        {
            Events = events.ToArray();
        }

        /// <summary>
        /// Reproducible IDs are important for optimistic concurrency
        /// </summary>
        public String Id
        {
            get { return BuildStableId(Events.First().StreamId, Events.First().StreamRevision); }
        }

        public static string BuildStableId(Guid streamId, int revision) 
        {
            return String.Format("commits/{0}/{1}", streamId, revision);
        }

        public StoreEvent[] Events { get; private set; }
        public bool IsDispatched { get; set; }
    }
}