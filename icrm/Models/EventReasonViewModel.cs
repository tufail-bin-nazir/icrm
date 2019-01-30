using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EventReasonViewModel
    {
        public EventReason eventReason { get; set; }
        public IEnumerable<EventReason> EventReasons { get; set; }
    }
}