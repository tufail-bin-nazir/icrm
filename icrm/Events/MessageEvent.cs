using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    using System.Threading.Tasks;

    public class MessageEventArgs : EventArgs
    {
        public Message message { get; set; }
    }

    public class MessageEvent
    {
        public event EventHandler<MessageEventArgs> MessageNotified;

        public void notify(Message message)
        {
            Debug.Print("----firing event--------");
            OnMesssageNotify(message);
        }

        protected virtual void OnMesssageNotify(Message message)
        {
            Debug.Print("----firing event-----again---" + message);
            if (message != null)
                MessageNotified(this,new MessageEventArgs(){message = message});
        }
    }
}