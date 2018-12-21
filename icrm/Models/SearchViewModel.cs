using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class SearchViewModel
    {
        public string email { get; set; }
        public string feedbackType { get; set; }
        public DateTime feedbackDate { get; set; }
        public int departmentId { get; set; }

    }
}