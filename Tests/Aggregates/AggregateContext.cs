using System;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Bus;
using CQRS.Sample.Commands;
using Machine.Specifications;
using StructureMap;

namespace CQRS.Sample.Tests.Aggregates
{
    [Subject("Aggregates")]
    public class AggregateContext {
        protected static IServiceBus Bus;

        Establish context = () =>
        {
            Bootstrapper
                .InMemory()
                .WithAggregatesIn(typeof (DomainCommandHandlers).Assembly)
                .Start();

            Bus = ObjectFactory.GetInstance<IServiceBus>();
        };

        protected static Guid CreateAccount(string email, string password)
        {
            var createAccount = new CreateAccount
            {
                Email = email, Password = password,
            };
            Bus.Publish(createAccount);
            Bus.Commit();
            return createAccount.StreamId;
        }
    }
}