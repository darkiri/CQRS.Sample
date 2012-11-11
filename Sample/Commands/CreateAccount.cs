using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Commands
{
    public class CreateAccount : DomainCommand
    {
        public CreateAccount()
        {
            // new account is a new stream
            StreamId = Guid.NewGuid();
        }

        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class ChangePassword : DomainCommand
    {
        public ChangePassword(Guid accountStreamId)
        {
            StreamId = accountStreamId;
        }

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}