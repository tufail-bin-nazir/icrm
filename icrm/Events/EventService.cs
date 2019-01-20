using System.Threading.Tasks;
using System.Web.Hosting;
using icrm.Models;

namespace icrm.Events
{
    public class EventService
    {
        private BroadcastMessageEvent broadcastEvent;
        private Notification notification;
        private FeedbackNotifyEvent feedbackNotifyEvent;
        private FeedbackNotification feedbackNotification;
        public EventService()
        {
            broadcastEvent = new BroadcastMessageEvent();
            notification = new Notification();
            feedbackNotification = new FeedbackNotification();
            feedbackNotifyEvent = new FeedbackNotifyEvent();
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

        public Task notifyFeedback(Feedback feedback)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                feedbackNotifyEvent.FeedbackNotified += feedbackNotification.OnFeedbackNotified;
                feedbackNotifyEvent.notify(feedback);

            });
            return null;
        }
    }
}