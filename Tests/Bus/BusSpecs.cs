using System;
using CQRS.Sample.Bus;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace CQRS.Sample.Tests.Bus
{
    [Subject(typeof (ServiceBus), "Publishing")]
    public class when_message_published : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler1;
        static Mock<HandlerBase<Message1>> _handler2;

        Establish context =()=>
                            {
                                _handler1 = new Mock<HandlerBase<Message1>>();
                                _handler2 = new Mock<HandlerBase<Message1>>();

                                Bus.Subscribe<Message1>("A", _handler1.Object.Handle);
                                Bus.Subscribe<Message1>("B", _handler1.Object.Handle);
                                Bus.Subscribe<Message1>("C", _handler2.Object.Handle);
                            };
        Because of =()=> Bus.PublishNow(new Message1());

        It should_delivered_to_A_and_B =()=> VerifyHandling(_handler1, 2.Times());
        It should_delivered_to_C =()=> VerifyHandling(_handler2, 1.Times());
    }

    [Subject(typeof (ServiceBus), "Sending")]
    public class when_message_sent : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler1;
        static Mock<HandlerBase<Message2>> _handler2;

        Establish context =()=>
                            {
                                _handler1 = new Mock<HandlerBase<Message1>>();
                                _handler2 = new Mock<HandlerBase<Message2>>();

                                Bus.Subscribe<Message1>("A", _handler1.Object.Handle);
                                Bus.Subscribe<Message1>("B", _handler1.Object.Handle);
                                Bus.Subscribe<Message2>("C", _handler2.Object.Handle);
                            };
        Because of =()=> Bus.SendNow("A", new Message1());

        It should_delivere_message_to_subscriber1 =()=> VerifyHandling(_handler1, 1.Times());
        It should_not_delivere_message_to_subscriber2 =()=> VerifyHandling(_handler2, 0.Times());
    }

    [Subject(typeof (ServiceBus), "Subscribing")]
    public class when_all_handlers_in_the_assembly_are_subscribed : with_bus_context
    {
        Establish context =()=> Bus.Start();
        Because of =()=> Bus.SendNow("Handler1", new Message1());

        It should_delivere_message_to_subscriber1 =()=> AssertMessagesReceived<Handler1, Message1>(1);
        It should_delivered_message_to_subscriber2 =()=> AssertMessagesReceived<Handler2, Message1>(0);
        It should_not_delivere_message_to_subscriber3 =()=> AssertMessagesReceived<Handler3, Message2>(0);
    }

    [Subject(typeof (ServiceBus), "Routing")]
    public class when_message_sent_to_a_shard : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler;

        Establish context =()=>
                            {
                                _handler = new Mock<HandlerBase<Message1>>();
                                Bus.Subscribe<Message1>("shard1.A", _handler.Object.Handle);
                                Bus.Subscribe<Message1>("shard2.A", _handler.Object.Handle);
                            };
        Because of =()=> Bus.SendNow("shard1.A", new Message1());

        It should_delivere_message_only_to_that_shard =()=> VerifyHandling(_handler, 1.Times());
    }

    [Subject(typeof (ServiceBus), "Routing")]
    public class when_message_sent_without_a_shard : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler;

        Establish context =()=>
                            {
                                _handler = new Mock<HandlerBase<Message1>>();
                                Bus.Subscribe<Message1>("shard1.A", _handler.Object.Handle);
                                Bus.Subscribe<Message1>("shard2.A", _handler.Object.Handle);
                            };
        Because of =()=> Bus.SendNow("A", new Message1());

        It should_delivere_message_to_all_subscribers =()=> VerifyHandling(_handler, 2.Times());
    }

    [Subject(typeof (ServiceBus), "Error handling")]
    public class when_subscriber_throws_an_error : with_bus_context
    {
        static Mock<HandlerBase<BadMessage>> _handler;

        Establish context =()=>
                            {
                                _handler = new Mock<HandlerBase<BadMessage>>();
                                Bus.Subscribe<BadMessage>("A", msg => { throw new Exception("Don't like"); });
                                Bus.Subscribe<BadMessage>("Errors", _handler.Object.Handle);
                            };
        Because of =()=> Bus.SendNow("A", new BadMessage());

        It should_deliver_message_to_the_error_queue =()=> VerifyHandling(_handler, 1.Times());
    }

    [Subject(typeof (ServiceBus), "Unit of Work")]
    public class when_unit_of_work_is_commited : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler;

        Establish context =()=>
                            {
                                 _handler = new Mock<HandlerBase<Message1>>();
                                Bus.Subscribe<Message1>("A", _handler.Object.Handle);
                            };

        Because of =()=>
                     {
                         Bus.Send("A", new Message1());
                         Bus.Send("A", new Message1());
                         Bus.Commit();
                     };

        It should_deliver_all_messages =()=> VerifyHandling(_handler, 2.Times());
    }

    [Subject(typeof (ServiceBus), "Unit of Work")]
    public class when_unit_of_work_is_not_commited : with_bus_context
    {
        static Mock<HandlerBase<Message1>> _handler;

        Establish context =()=>
        {
            _handler = new Mock<HandlerBase<Message1>>();
            Bus.Subscribe<Message1>("A", _handler.Object.Handle);
        };
        Because of =()=> Bus.Send("A", new Message1());

        It should_not_deliver_any_messages =()=> VerifyHandling(_handler, 0.Times());
    }
}