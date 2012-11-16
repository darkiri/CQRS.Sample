/*using System.Threading;
using SignalR;
using SignalR.Hubs;

namespace CQRS.Sample.GUI
{
    [HubName("notifications")]
    public class NotificationsHub : Hub
    {
        private Timer _timer = new Timer(UpdateStatus, null, 1000, 1000);

        public void Init(string message)
        {
            if (null != _timer)
            {
                _timer.Dispose();
            }
            _timer = new Timer(UpdateStatus, null, 1000, 1000);
        }

        private static void UpdateStatus(object state)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
            context.Clients.updateStatus("hello world!");
        }
    }
}*/