using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Events;

namespace CQRS.Sample.Store
{
    public class EventStream : IEventStream
    {
        private readonly IPersister _persister;
        private readonly ICommitDispatcher _dispatcher;
        private readonly List<StoreEvent> _pendingEvents = new List<StoreEvent>();
        private readonly List<StoreEvent> _committedEvents = new List<StoreEvent>();

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

        public IEnumerable<StoreEvent> UncommittedEvents
        {
            get { return _pendingEvents; }
        }

        public IEnumerable<StoreEvent> CommittedEvents
        {
            get { return _committedEvents; }
        }

        public void Append(IEvent evt)
        {
            var revision = _pendingEvents.Any() ? _pendingEvents.Last().StreamRevision : Revision;
            _pendingEvents.Add(new StoreEvent
            {
                IsDispatched = false,
                StreamRevision = revision + 1,
                Body = evt,
            });
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

        private void PopulateStream(IEnumerable<StoreEvent> events)
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