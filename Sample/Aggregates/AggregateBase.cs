using System;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public class AggregateBase<TState> where TState : MutableState {
        private readonly Action<StoreEvent> _publishAction;

        public AggregateBase(TState state, Action<StoreEvent> publishAction)
        {
            State = state;
            _publishAction = publishAction;
        }

        public TState State { get; private set; }

        protected void ApplyAndPublish(StoreEvent evt)
        {
            State.ApplyEvent(evt);
            _publishAction(evt);
        }
    }
}