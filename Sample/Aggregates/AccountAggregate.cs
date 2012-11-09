using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public class AccountAggregate
    {
        readonly IEnumerable<IEvent> _events;
        readonly Action<IEvent> _publishAction;

        public AccountAggregate(IEnumerable<IEvent> events, Action<IEvent> publishAction)
        {
            _events = events;
            _publishAction = publishAction;
        }

        public void When(CreateAccount command)
        {
            // No validation logic here
            // Assuming that the UI has already checked email existence
            // It is unlikely to have duplicate emails
            // Still there will be an async email validation, just for fun

            if (_events.Any())
            {
                throw new Exception("Account exists");
            }

            var accountCreated = new AccountCreated
            (
                command.StreamId,
                0,
                command.Email,
                PasswordHash.CreateHash(command.Password)
            );
            Apply(accountCreated);
        }

        public void When(ChangePassword command)
        {
            var passwordChanged = new PasswordChanged(command.StreamId, PasswordHash.CreateHash(command.NewPassword));
            Apply(passwordChanged);
        }

        void Apply(IEvent accountCreated)
        {
            _publishAction(accountCreated);
        }
    }
}