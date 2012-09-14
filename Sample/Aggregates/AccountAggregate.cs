using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;

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
            if (_events.Any())
            {
                throw new Exception("Account exists");
            }

            var accountCreated = new AccountCreated
            {
                Version = 0,
                Email = command.Email,
                PasswordHash = PasswordHash.CreateHash(command.Password)
            };
            Apply(accountCreated);
        }

        void Apply(AccountCreated accountCreated)
        {
            _publishAction(accountCreated);
        }
    }
}