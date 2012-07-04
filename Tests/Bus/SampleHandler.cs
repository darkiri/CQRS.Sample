using System;
using CQRS.Sample.Bus;
using StructureMap;

namespace CQRS.Sample.Tests.Bus
{
    public class Message1 : IMessage {}

    public class Message2 : IMessage {}

    public class BadMessage : IMessage {}

    public abstract class HandlerBase<T> where T : IMessage
    {
        public virtual void Handle(T message)
        {
            if (message.GetType() == typeof (BadMessage))
            {
                throw new Exception("Dont like");
            }
            MessagesReceived++;
        }

        public int MessagesReceived { get; private set; }
    }

    [PluginFamily(IsSingleton = true)]
    public class Handler1 : HandlerBase<Message1> {}

    [PluginFamily(IsSingleton = true)]
    public class Handler2 : HandlerBase<Message1> {}

    [PluginFamily(IsSingleton = true)]
    public class Handler3 : HandlerBase<Message2> {}
}