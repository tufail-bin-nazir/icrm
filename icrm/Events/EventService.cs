using System.Threading.Tasks;
using System.Web.Hosting;
using icrm.Models;

namespace icrm.Events
{
    using System.Diagnostics;

    public class EventService
    {
        private BroadcastMessageEvent broadcastEvent;
        private Notification notification;
        private FeedbackNotifyEvent feedbackNotifyEvent;
        private FeedbackNotification feedbackNotification;
        public MessageEvent messasgeEvent;
        public MessageHub messageHub;
        public HrAvailableNotify hrAvailableNotify;

        private EmailSend emailSend;
        public EventService()
        {
            this.broadcastEvent = new BroadcastMessageEvent();
            this.notification = new Notification();
            this.feedbackNotification = new FeedbackNotification();
            this.feedbackNotifyEvent = new FeedbackNotifyEvent();
            this.messageHub=new MessageHub();
            this.messasgeEvent=new MessageEvent();
            this.hrAvailableNotify = new HrAvailableNotify();
            emailSend = new EmailSend();
        }

        public Task sendbroadcastMessage(BroadcastMessage broadcastMessage)
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

        public Task pushMessage(Message message)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                Debug.Print("--------in push messages call-----");
                messasgeEvent.MessageNotified += messageHub.OnMessageNotified;
                messasgeEvent.notify(message);

            });
            return null;
        }

        public Task sendEmails(string emails,string body)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                System.Diagnostics.Debug.WriteLine("jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj");
                emailSend.sendEmailAsync(emails,body);

            });
            return null;
        }

        public Task NotifyHrAboutChat(Message message)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                {
                    Debug.Print("--------notify hr about chat----");
                    this.messageHub.NotifyHRAboutChat(message);

                });
            return null;
        }

        public Task hrAvailableNotification(string deviceId)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                {
                    Debug.Print("--------inhr available notify----");
                    this.hrAvailableNotify.OnHrAvailableNotify(deviceId);

                });
            return null;
        }

        public Task chatClosedByUser(string username)
        {

            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                {
                    Debug.Print("--------chat closed by user notify----");
                    this.messageHub.userClosedChat(username);

                });
            return null;
        }
    }

}
