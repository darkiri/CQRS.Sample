using System;
using System.Collections.Generic;
using CQRS.Sample.Aggregates;
using CQRS.Sample.Bus;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using CQRS.Sample.Store;
using NLog;

namespace CQRS.Sample
{
    public class CommandHandlers
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EventStore _store;
        private readonly IServiceBus _bus;

        public CommandHandlers(EventStore store, IServiceBus bus)
        {
            _store = store;
            _bus = bus;
        }

        public void Handle(CreateAccount command)
        {
            ProceedWith<AccountAggregate, AccountAggregateMemento>(command.StreamId, ar => ar.CreateNew(command));
        }

        public void Handle(ChangePassword command)
        {
            ProceedWith<AccountAggregate, AccountAggregateMemento>(command.StreamId, ar => ar.ChangePassword(command));
        }

        private void ProceedWith<TAggregate, TState>(Guid streamId, Action<TAggregate> handleCommand)
            where TState : MutableState
            where TAggregate : AggregateBase<TState>
        {
            var stream = _store.OpenStream(streamId);
            try
            {
                handleCommand(CreateAggregate<TAggregate, TState>(stream.CommittedEvents, stream.Append));
                stream.Commit();
            }
            catch (Exception e)
            {
                _logger.ErrorException("Cannot proceed command", e);
                stream.Cancel();
                
                _bus.Publish(new ServerFailure{StreamId = streamId, Message = e.Message});
                _bus.Commit();
            }
        }

        private static TAggregate CreateAggregate<TAggregate, TState>(IEnumerable<StoreEvent> events, Action<StoreEvent> publishAction)
            where TState : MutableState
            where TAggregate : AggregateBase<TState>
        {
            var state = Activator.CreateInstance<TState>();
            state.LoadFromHistory(events);
            return (TAggregate)Activator.CreateInstance(typeof (TAggregate), new object[] {state, publishAction});
        }
    }
}