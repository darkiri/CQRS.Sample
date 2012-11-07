using System;
using CQRS.Sample.Store;

namespace CQRS.Sample.Events
{
    public class AccountCreated : IEvent
    {
        public AccountCreated(Guid streamId, int version, string email, string passwordHash)
        {
            Version = version;
            StreamId = streamId;
            Email = email;
            PasswordHash = passwordHash;
        }

        public Guid StreamId { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public int Version { get; private set; }
    }
}