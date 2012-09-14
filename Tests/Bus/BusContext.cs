using System.Reflection;
using CQRS.Sample.Bus;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace CQRS.Sample.Tests.Bus
{
    [Subject(typeof (ServiceBus))]
    public class with_bus_context
    {

        protected static ServiceBus Bus;

        Establish context = () =>
                                {
                                    Bus = new ServiceBus(new HandlerRepository(Assembly.GetExecutingAssembly()));
                                };


        protected static T Handler<T>()
        {
            return ObjectFactory.GetInstance<T>();
        }

        protected static void AssertMessagesReceived<THandler, TMsg>(int expectedMessages)
            where THandler : HandlerBase<TMsg>
            where TMsg : IMessage
        {
            Assert.That(Handler<THandler>().MessagesReceived, Is.EqualTo(expectedMessages));
        }

        protected static void VerifyHandling<TMsg>(Mock<HandlerBase<TMsg>> handler, int times) where TMsg : IMessage
        {
            handler.Verify(h => h.Handle(Moq.It.IsAny<TMsg>()), Times.Exactly(times));
        }
    }

    public static class BusContextExtensions
    {
        public static int Times(this int times)
        {
            return times;
        }
    }
}