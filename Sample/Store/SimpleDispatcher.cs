using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    public class SimpleDispatcher : ICommitDispatcher
    {
        private readonly IPersister _persister;
        private readonly IServiceBus _bus;

        public SimpleDispatcher(IPersister persister, IServiceBus bus)
        {
            _persister = persister;
            _bus = bus;
        }

        public void Dispatch()
        {
            foreach (var evt in _persister.GetUndispatchedEvents())
            {
                _bus.Publish(evt.Body);
                _bus.Commit();
                _persister.MarkAsDispatched(evt);
            }
        }
    }
}