using System;
using CQRS.Sample.Events;
using SignalR;

namespace CQRS.Sample.GUI
{
    public class NotificationProjection
    {
        public void Handle(AccountCreated message)
        {
            SendNotification(message.StreamId, string.Format("Account {0} created", message.Email));
        }

        public void Handle(PasswordChanged message)
        {
            SendNotification(message.StreamId, "Password was changed.");
        }


        public void Handle(AccountChangeFailed message)
        {
            SendNotification(message.StreamId, "Changes could not be done.");
        }


        public void Handle(ServerFailure message)
        {
            SendNotification(message.StreamId, message.Message);
        }

        public void SendNotification(Guid streamId, string message)
        {
            var context = GlobalHost.ConnectionManager.GetConnectionContext<NotificationsEndpoint>();
            context.Groups.Send(streamId.ToString(), message);
        }
    }
}