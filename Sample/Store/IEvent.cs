using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Store
{
    public interface IEvent : IMessage
    {
        int StreamRevision { get; }
        Guid StreamId { get; }
        Guid Id { get; }
        Object Body { get; }
    }
}