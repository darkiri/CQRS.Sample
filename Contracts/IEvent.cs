
using CQRS.Sample.Bus;

namespace CQRS.Sample.Events
{
    public interface IEvent : IMessage
    {
        int Version { get; } 
    }
}