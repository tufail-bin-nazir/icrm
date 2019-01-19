using System.Threading.Tasks;
using System.Web.Hosting;
using icrm.Models;

namespace icrm.Events
{
    public class EventService
    {
        private BroadcastMessageEvent broadcastEvent;
        private Notification notification;

        public EventService()
        {
            broadcastEvent = new BroadcastMessageEvent();
            notification = new Notification();
        }

        public Task sendmessage(BroadcastMessage broadcastMessage)
        {

             HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                broadcastEvent.MessageBroadcasted += notification.OnMessageBroadcasted;
                broadcastEvent.broadcast(broadcastMessage);
                
            });
            return null;
        }
    }
}