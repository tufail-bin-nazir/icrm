using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class FeedbackDTO
    {
        public string name { get; set; }
        public int departmentId { get; set; }
        public DateTime createDate { get; set; }
        public string typeOfFeedback { get; set; }
    }
}