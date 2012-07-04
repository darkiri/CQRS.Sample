namespace CQRS.Sample.Bus
{
    public interface IServiceBus : IUnitOfWork
    {
        void Publish(IMessage message);
        void Send(string destination, IMessage message);
    }
}