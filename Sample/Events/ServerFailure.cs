using System;

namespace CQRS.Sample.Events
{
    public class ServerFailure : DomainEvent
    {
        public ServerFailure(Guid streamId, string message) : base(streamId)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}