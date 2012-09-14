using System;
using StructureMap;

namespace CQRS.Sample.Bus
{
    [PluginFamily(IsSingleton = true)]
    public interface IServiceBus : IUnitOfWork
    {
        void Publish(IMessage message);
        void Send(string destination, IMessage message);
        void Start();
        void Subscribe<T>(Action<T> handler) where T : IMessage;
    }
}