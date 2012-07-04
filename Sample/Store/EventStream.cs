using System;
using System.Collections.Generic;
using System.Linq;

namespace CQRS.Sample.Store
{
    public class EventStream : IEventStream
    {
        private readonly IPersister _persister;
        private readonly ICommitDispatcher _dispatcher;
        private readonly List<IEvent> _pendingEvents = new List<IEvent>();
        private readonly List<IEvent> _committedEvents = new List<IEvent>();

        public EventStream(IPersister persister, ICommitDispatcher dispatcher, Guid streamId, int revision)
        {
            _persister = persister;
            _dispatcher = dispatcher;
            StreamId = streamId;
            Revision = revision;
            
            PopulateStream(_persister.GetEvents(StreamId, 0, revision).ToList());
        }

        public Guid StreamId { get; private set; }

        public int Revision { get; private set; }

        public IEnumerable<IEvent> UncommittedEvents
        {
            get { return _pendingEvents; }
        }

        public IEnumerable<IEvent> CommittedEvents
        {
            get { return _committedEvents; }
        }

        public void Append(IEvent evt)
        {
            _pendingEvents.Add(evt);
        }

        public void Commit()
         {
            if (_pendingEvents.Any())
            {
                try
                {
                    _persister.PersistEvents(StreamId, _pendingEvents);

                    PopulateStream(_pendingEvents);
                    _pendingEvents.Clear();
                    _dispatcher.Dispatch();
                }
                catch (OptimisticConcurrencyException)
                {
                    Revision = _persister
                        .GetEvents(StreamId, Revision, Int32.MaxValue)
                        .Select(e => e.StreamRevision)
                        .Max();
                }
            }
        }

        private void PopulateStream(IEnumerable<IEvent> events)
        {
            foreach (var evt in events)
            {
                _committedEvents.Add(evt);
                Revision++;
            }
        }

        public void Cancel()
        {
            _pendingEvents.Clear();
        }
    }
}