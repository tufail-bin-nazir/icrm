using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using icrm.Models;

namespace icrm.Events
{
    public class BroadcastMessageEventArgs : EventArgs
    {
        public BroadcastMessage BroadcastMessage { get; set; }
       // public List<String> Recievers { get; set; }
    }

    public class BroadcastMessageEvent
    {
        public event EventHandler<BroadcastMessageEventArgs> MessageBroadcasted;

        public void broadcast(BroadcastMessage broadcastMessage)
        {
             OnMessageBroadcasted(broadcastMessage);
        }

        protected virtual void OnMessageBroadcasted(BroadcastMessage broadcastMessage)
        {
            if (MessageBroadcasted != null)
                MessageBroadcasted(this, new BroadcastMessageEventArgs() { BroadcastMessage = broadcastMessage});
        }
    }
}