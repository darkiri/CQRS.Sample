using CQRS.Sample.Aggregates;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Bus;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using Machine.Specifications;
using NUnit.Framework;
using StructureMap;

namespace CQRS.Sample.Tests.Aggregates
{
    [Subject(typeof (AccountAggregate))]
    public class when_create_account_requested
    {
        static IServiceBus _bus;

        Establish context = () =>
        {
            Bootstrapper
                .InMemory()
                .WithAggregatesIn(typeof (DomainCommandHandlers).Assembly)
                .Start();

            _bus = ObjectFactory.GetInstance<IServiceBus>();
            _bus.Subscribe<AccountCreated>(OnAccountCreated);
        };

        static string Email;
        static string Hash;

        public static void OnAccountCreated(AccountCreated msg)
        {
            Email = msg.Email;
            Hash = msg.PasswordHash;
        }

        Because of = () =>
        {
            _bus.Publish(new CreateAccount
            {
                Email = "em@ai.il",
                Password = "Swordfish",
            });
            _bus.Commit();
        };

        It should_set_email = () => Assert.That(Email, Is.EqualTo("em@ai.il"));
        It should_set_password_hash = () => Assert.True(PasswordHash.ValidatePassword("Swordfish", Hash));
    }
}