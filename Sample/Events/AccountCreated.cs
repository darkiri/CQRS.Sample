using CQRS.Sample.Bus;

namespace CQRS.Sample.Events
{
    public class AccountCreated : IEvent
    {
        public int Version { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}