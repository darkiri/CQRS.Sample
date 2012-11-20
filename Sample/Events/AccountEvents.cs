using CQRS.Sample.Store;

namespace CQRS.Sample.Events
{
    public class AccountCreated : StoreEvent
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class PasswordChanged : StoreEvent
    {
        public string PasswordHash { get; set; }
    }

    public class AccountChangeFailed : StoreEvent { }
}