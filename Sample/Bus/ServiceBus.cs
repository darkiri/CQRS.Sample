using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CQRS.Sample.Bus
{
    public class ServiceBus : IServiceBus
    {
        readonly IHandlerRepository _repository;
        // should there be many queues with few listeners pro queue?
        // or just couple of queues with loads of listeners?
        // i am not going to imply anything or do any optimizations
        private readonly IList<Subscriber> _queues = new List<Subscriber>();
        private readonly Queue<MessageFuture> _pendingMessages = new Queue<MessageFuture>();
        private readonly string _errorsQueueName;


        public ServiceBus(IHandlerRepository repository)
        {
            _repository = repository;
            _errorsQueueName = "Errors";
        }

        public void Start()
        {
            _repository.GetHandlers().ToList().ForEach(AddHandler);
        }

        public void Subscribe<T>()
        {
            HandlerRepository.MessageHandlersIn(typeof (T))
                             .ToList()
                             .ForEach(AddHandler);
        }

        public void Subscribe<TMsg>(string queue, Action<TMsg> handler) where TMsg : IMessage
        {
            AddSubscriber2Queue(queue, typeof (TMsg), msg => handler((TMsg) msg));
        }

        public void Subscribe<TMsg>(Action<TMsg> handler) where TMsg : IMessage
        {
            var reflectedType = typeof (TMsg).ReflectedType;
            AddSubscriber2Queue(reflectedType != null ? reflectedType.Name : null, typeof (TMsg), msg => handler((TMsg) msg));
        }

        private void AddSubscriber2Queue(string queue, Type messageType, Action<IMessage> handler)
        {
            _queues.Add(Subscriber.Create(queue, messageType, handler));
        }

        private void AddHandler(MethodInfo handlerMethod)
        {
            var handlerInstance = _repository.GetInstance(handlerMethod);
            var messageType = handlerMethod.GetParameters().First().ParameterType;

            var action = BuildLambda(handlerInstance, handlerMethod, messageType);

            AddSubscriber2Queue(handlerInstance.GetType().Name, messageType, action);
        }


        private Action<IMessage> BuildLambda(object handlerInstance, MethodInfo handlerMethod, Type messageType)
        {
            var lambdaParameter = Expression.Parameter(typeof (IMessage), "msg");
            var handlerParameter = Expression.Convert(lambdaParameter, messageType);

            var handlerCall = Expression.Call(
                Expression.Constant(handlerInstance),
                handlerMethod,
                new Expression[] {handlerParameter});

            return Expression.Lambda<Action<IMessage>>(handlerCall, lambdaParameter).Compile();
        }


        public void Publish(IMessage message)
        {
            _pendingMessages.Enqueue(MessageFuture.Create("", message));
        }

        public void Send(string destination, IMessage message)
        {
            _pendingMessages.Enqueue(MessageFuture.Create(destination, message));
        }

        public void PublishNow(IMessage message)
        {
            Publish(message);
            Commit();
        }

        public void SendNow(string destination, IMessage message)
        {
            Send(destination, message);
            Commit();
        }

        public void Commit()
        {
            while (_pendingMessages.Any())
            {
                var msg = _pendingMessages.Dequeue();
                _queues
                    .Where(msg.IsForSubscriber)
                    .Where(msg.ShouldBeRouted)
                    .ToList()
                    .ForEach(h => DoSend(h, msg.Message));
            }
        }

        public void Cancel()
        {
            _pendingMessages.Clear();
        }

        private void DoSend(Subscriber subscriber, IMessage message)
        {
            try
            {
                subscriber.HandlerAction(message);
            }
            catch (Exception)
            {
                if (subscriber.Queue != _errorsQueueName)
                {
                    SendNow(_errorsQueueName, message);
                }
            }
        }

        private class MessageFuture
        {
            private string Destination { get; set; }
            public IMessage Message { get; private set; }

            public static MessageFuture Create(string destination, IMessage message)
            {
                return new MessageFuture
                       {
                           Destination = destination,
                           Message = message,
                       };
            }

            public bool IsForSubscriber(Subscriber subscriber)
            {
                return subscriber.MessageType == Message.GetType();
            }

            public bool ShouldBeRouted(Subscriber subscriber)
            {
                return IsBroadcast || QueueOnTheRoute(subscriber.Queue);
            }

            private bool QueueOnTheRoute(string queue)
            {
                return queue.EndsWith(Destination);
            }

            private bool IsBroadcast
            {
                get { return String.IsNullOrEmpty(Destination); }
            }
        }

        private class Subscriber
        {
            public Type MessageType { get; private set; }
            public Action<IMessage> HandlerAction { get; private set; }
            public string Queue { get; private set; }

            public static Subscriber Create(string queue, Type messageType, Action<IMessage> handler)
            {
                return new Subscriber
                       {
                           MessageType = messageType,
                           Queue = queue,
                           HandlerAction = handler,
                       };
            }
        }
    }
}