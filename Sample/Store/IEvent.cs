
using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    public interface IEvent : IMessage
    {
        int Version { get; } 
    }
}