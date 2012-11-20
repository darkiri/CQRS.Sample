using System;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public class AggregateBase<TState> where TState : MutableState
    {
        private readonly Action<StoreEvent> _publishAction;
        private readonly TState _state;

        public AggregateBase(TState state, Action<StoreEvent> publishAction)
        {
            _state = state;
            _publishAction = publishAction;
        }

        public TState State
        {
            get { return _state; }
        }

        protected void ApplyAndPublish(StoreEvent evt)
        {
            State.ApplyEvent(evt);
            _publishAction(evt);
        }
    }
}