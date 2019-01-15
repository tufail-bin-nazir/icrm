using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class ChatViewModel
    {
        public string Text { get; set; }
        public int? ChatId { get; set; }
        public string Sender { get; set; }

        public override string ToString()
        {
            return "Text: " + Text;
            // Reciever  "+Reciever+"    Sender    "+Sender;
        }
    }
}