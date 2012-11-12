using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace CQRS.Sample.Store
{
    public class EventStream : IEventStream
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPersister _persister;
        private readonly ICommitDispatcher _dispatcher;
        private readonly List<StoreEvent> _pendingEvents = new List<StoreEvent>();
        private readonly List<StoreEvent> _committedEvents = new List<StoreEvent>();

        public EventStream(IPersister persister, ICommitDispatcher dispatcher, Guid streamId, int revision)
        {
            _persister = persister;
            _dispatcher = dispatcher;
            StreamId = streamId;
            
            PopulateStream(_persister.GetEvents(StreamId, 0, revision).ToList());
        }

        public Guid StreamId { get; private set; }

        public int Revision { get; private set; }

        public IEnumerable<StoreEvent> UncommittedEvents
        {
            get { return _pendingEvents; }
        }

        public IEnumerable<IEvent> CommittedEvents
        {
            get { return _committedEvents.Select(e => e.Body); }
        }

        public void Append(IEvent evt)
        {
            var revision = _pendingEvents.Any() ? _pendingEvents.Last().StreamRevision : Revision;
            _pendingEvents.Add(new StoreEvent
            {
                Id = Guid.NewGuid(),
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
                catch (OptimisticConcurrencyException e)
                {
                    _logger.WarnException("Stream has been changed since last load.", e);
                    var newEvents = _persister.GetEvents(StreamId, Revision, Int32.MaxValue);
                    PopulateStream(newEvents);
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