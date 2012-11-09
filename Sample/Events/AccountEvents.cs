using System;

namespace CQRS.Sample.Events
{
    public class AccountCreated : DomainEvent
    {
        public AccountCreated(Guid streamId, int version, string email, string passwordHash) : base(streamId)
        {
            Email = email;
            PasswordHash = passwordHash;
        }

        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
    }
        
    public class PasswordChanged : DomainEvent
    {
        public PasswordChanged(Guid streamId, string passwordHash) : base(streamId)
        {
            PasswordHash = passwordHash;
        }

        public string PasswordHash { get; private set; }
    }
}