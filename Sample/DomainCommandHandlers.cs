using System;
using CQRS.Sample.Aggregates;
using CQRS.Sample.Commands;
using CQRS.Sample.Store;
using NLog;

namespace CQRS.Sample
{
    public class DomainCommandHandlers
    {
        readonly Logger _logger = LogManager.GetLogger("DomainCommandHandlers");
        readonly EventStore _store;

        public DomainCommandHandlers(EventStore store)
        {
            _store = store;
        }

        public void Handle(CreateAccount command)
        {
            ProceedAccountCommand(command.StreamId, ar => ar.When(command));
        }

        public void Handle(ChangePassword command)
        {
            ProceedAccountCommand(command.StreamId, ar => ar.When(command));
        }

        void ProceedAccountCommand(Guid streamId, Action<AccountAggregate> handleCommand)
        {
            var stream = _store.OpenStream(streamId);
            try
            {
                var ar = new AccountAggregate(stream.CommittedEvents, stream.Append);
                handleCommand(ar);
                stream.Commit();
            }
            catch (Exception e)
            {
                _logger.ErrorException("Cannot proceed command", e);
                stream.Cancel();
            }
        }
    }
}