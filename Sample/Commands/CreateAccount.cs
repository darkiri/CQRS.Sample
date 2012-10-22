using System;
using CQRS.Sample.Bus;

namespace CQRS.Sample.Commands
{
    public class CreateAccount : IMessage
    {
        private readonly Guid _accountStreamId;
        public CreateAccount()
        {
            // new account is a new stream
            _accountStreamId = Guid.NewGuid();
        }

        public Guid StreamId { get { return _accountStreamId; } }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}