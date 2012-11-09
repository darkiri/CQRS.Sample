using System;

namespace CQRS.Sample.Reporting
{
    public class AccountDTO
    {
        public Guid StreamId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}