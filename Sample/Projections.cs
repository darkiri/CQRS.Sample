using System;
using CQRS.Sample.Aggregates;
using CQRS.Sample.Commands;
using CQRS.Sample.Store;
using NLog;

namespace CQRS.Sample
{
    public class Projections
    {
        private readonly Logger _logger = LogManager.GetLogger("Projections");
        readonly EventStore _store;

        public Projections(EventStore store)
        {
            _store = store;
        }

        public void Handle(CreateAccount command)
        {
            var stream = _store.OpenStream(command.StreamId);
            var events = stream.GetEvents(0, Int32.MaxValue);
            try
            {
                var ar = new AccountAggregate(events, stream.Append);
                ar.When(command);
                stream.Commit();
            } catch(Exception e)
            {
                _logger.Error(e);
                stream.Cancel();
            }
        }
    }
}