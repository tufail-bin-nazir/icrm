using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class FeedBackType
    {
        public int Id { get; set; }
        public string name { get; set; }

        public virtual IEnumerable<Feedback> Feedbacks { get; set; }
    }
}