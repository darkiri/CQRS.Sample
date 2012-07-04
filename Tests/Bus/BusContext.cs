using CQRS.Sample.Bus;
using Machine.Specifications;
using Moq;
using NUnit.Framework;

namespace CQRS.Sample.Tests.Bus
{
    [Subject(typeof (ServiceBus))]
    public class with_bus_context
    {

        static SimpleContainer _container;
        protected static ServiceBus Bus;

        Establish context = () =>
                                {
                                    _container = new SimpleContainer();
                                    Bus = new ServiceBus(_container);
                                };


        protected static T Handler<T>()
        {
            return _container.GetInstance<T>();
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