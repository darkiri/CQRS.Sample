using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public class AccountAggregate
    {
        readonly Action<IEvent> _publishAction;

        private string _email;
        private string _passwordHash;

        public AccountAggregate(IEnumerable<IEvent> events, Action<IEvent> publishAction)
        {
            LoadFromHistory(events);
            _publishAction = publishAction;
        }

        private void LoadFromHistory(IEnumerable<IEvent> events)
        {
            foreach (var evt in events)
            {
                ApplyFromHistory(evt);
            }
        }

        void ApplyFromHistory(IEvent evt)
        {
            ApplyEvent(evt, _ => { });
        }

        void ApplyAndPublish(IEvent evt)
        {
            ApplyEvent(evt, _publishAction);
        }

        void ApplyEvent(IEvent evt, Action<IEvent> continuation)
        {
            GetType()
                .GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance,
                           null, new Type[] {evt.GetType()}, null)
                .Invoke(this, new object[] {evt});
            continuation(evt);
        }

        public void When(CreateAccount command)
        {
            // No validation logic here
            // Assuming that the UI has already checked email existence
            // It is unlikely to have duplicate emails
            // Still there will be an async email validation, just for fun

            if (AccountnExists())
            {
                ApplyAndPublish(new AccountChangeFailed(command.StreamId));
            }
            else
            {
                ApplyAndPublish(new AccountCreated (
                                    command.StreamId,
                                    command.Email,
                                    PasswordHash.CreateHash(command.Password)));
            }
        }

        bool AccountnExists()
        {
            return !String.IsNullOrEmpty(_email);
        }

        public void When(ChangePassword command)
        {
            if (!PasswordHash.ValidatePassword(command.OldPassword, _passwordHash))
            {
                ApplyAndPublish(new AccountChangeFailed(command.StreamId));
            }
            else
            {
                _passwordHash = command.NewPassword;

                ApplyAndPublish(new PasswordChanged(command.StreamId, PasswordHash.CreateHash(command.NewPassword)));
            }
        }

        private void Apply(AccountChangeFailed evt)
        {
        }

        private void Apply(AccountCreated evt)
        {
            _email = evt.Email;
            _passwordHash = evt.PasswordHash;
        }
        private void Apply(PasswordChanged evt)
        {
            _passwordHash = evt.PasswordHash;
        }
    }
}