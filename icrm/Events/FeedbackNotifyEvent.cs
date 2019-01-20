using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using icrm.Models;

namespace icrm.Events
{
    public class FeedbackNotifyEventArgs : EventArgs
    {
        public Feedback Feedback { get; set; }
        // public List<String> Recievers { get; set; }
    }
    public class FeedbackNotifyEvent
    {
       
        public event EventHandler<FeedbackNotifyEventArgs> FeedbackNotified;

        public void notify(Feedback Feedback)
        {
            Debug.Print("----firing event--------");
            OnFeedbackNotify(Feedback);
        }

        protected virtual void OnFeedbackNotify(Feedback Feedback)
        {
            Debug.Print("----firing event-----again---" + Feedback);
            if (Feedback != null)
                FeedbackNotified(this, new FeedbackNotifyEventArgs() { Feedback = Feedback });
        }
    }
}