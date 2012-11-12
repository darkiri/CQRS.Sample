using System;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public class AccountAggregate : AggregateBase<AccountAggregateMemento>
    {
        public AccountAggregate(AccountAggregateMemento state, Action<IEvent> publishAction)
            : base(state, publishAction) {}

        public void CreateNew(CreateAccount command)
        {
            // Assuming that the UI has already checked email existence
            // It is unlikely to have duplicate emails
            // Still there will be an async email validation, just for fun

            if (AccountExists())
            {
                ApplyAndPublish(new AccountChangeFailed(command.StreamId));
            }
            else
            {
                ApplyAndPublish(new AccountCreated(
                                    command.StreamId,
                                    command.Email,
                                    PasswordHash.CreateHash(command.Password)));
            }
        }

        bool AccountExists()
        {
            return !String.IsNullOrEmpty(State.Email);
        }

        public void ChangePassword(ChangePassword command)
        {
            if (!PasswordHash.ValidatePassword(command.OldPassword, State.PasswordHash))
            {
                ApplyAndPublish(new AccountChangeFailed(command.StreamId));
            }
            else
            {
                ApplyAndPublish(new PasswordChanged(command.StreamId, PasswordHash.CreateHash(command.NewPassword)));
            }
        }
    }
}