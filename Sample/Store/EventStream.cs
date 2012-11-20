using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace CQRS.Sample.Store
{
    public class EventStream : IEventStream
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPersister _persister;
        private readonly ICommitDispatcher _dispatcher;
        private readonly List<StoreEvent> _pendingEvents = new List<StoreEvent>();
        private readonly List<StoreEvent> _committedEvents = new List<StoreEvent>();

        public EventStream(IPersister persister, ICommitDispatcher dispatcher, Guid streamId, int revision)
        {
            _persister = persister;
            _dispatcher = dispatcher;
            StreamId = streamId;
            
            PopulateStream(_persister.GetCommits(StreamId, 0, revision).SelectMany(c => c.Events).ToList());
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

        public void Append(StoreEvent evt)
        {
            evt.StreamRevision = Revision + _pendingEvents.Count + 1;
            _pendingEvents.Add(evt);
        }

        public void Commit()
         {
            if (_pendingEvents.Any())
            {
                try
                {
                    _persister.PersistCommit(StreamId, _pendingEvents);
                    PopulateStream(_pendingEvents);
                    _pendingEvents.Clear();
                    _dispatcher.Dispatch();
                }
                catch (OptimisticConcurrencyException e)
                {
                    _logger.WarnException("Stream has been changed since last load.", e);
                    var newEvents = _persister
                        .GetCommits(StreamId, Revision, Int32.MaxValue)
                        .SelectMany(c => c.Events);
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