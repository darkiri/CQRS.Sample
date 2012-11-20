using CQRS.Sample.Events;

namespace CQRS.Sample.Aggregates
{
    public class AccountAggregateMemento : MutableState
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        void Apply(AccountCreated evt)
        {
            Email = evt.Email;
            PasswordHash = evt.PasswordHash;
        }

        void Apply(PasswordChanged evt)
        {
            PasswordHash = evt.PasswordHash;
        }

        void Apply(AccountChangeFailed evt) { }
    }
}