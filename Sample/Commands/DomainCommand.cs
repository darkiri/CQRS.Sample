using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Commands
{
    public abstract class DomainCommand : IMessage {
        public Guid StreamId { get; protected set; }
    }
}