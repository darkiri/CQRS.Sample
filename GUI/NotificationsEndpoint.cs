using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using SignalR;

namespace CQRS.Sample.GUI
{
    public class NotificationsEndpoint : PersistentConnection
    {
        protected override Task OnConnectedAsync(IRequest request, string connectionId)
        {
            Groups.Add(connectionId, HttpContext.Current.GetStreamId().ToString());
            return base.OnConnectedAsync(request, connectionId);
        }

        protected override Task OnReconnectedAsync(IRequest request, IEnumerable<string> groups, string connectionId)
        {
            Groups.Add(connectionId, HttpContext.Current.GetStreamId().ToString());
            return base.OnReconnectedAsync(request, groups, connectionId);
        }

        protected override Task OnDisconnectAsync(string connectionId)
        {
            //Groups.Remove(connectionId, HttpContext.Current.GetStreamId().ToString());
            return base.OnDisconnectAsync(connectionId);
        }
    }
}