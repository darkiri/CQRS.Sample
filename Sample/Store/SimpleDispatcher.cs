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
            foreach (var commit in _persister.GetUndispatchedCommits())
            {
                foreach (var evt in commit.Events) {
                    _bus.Publish(evt);
                }
                _bus.Commit();
                _persister.MarkAsDispatched(commit);
            }
        }
    }
}