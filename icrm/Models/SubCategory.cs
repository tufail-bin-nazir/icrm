using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class SubCategory
    {
        public int Id { get; set; }
        public String name { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public int priorityId { get; set; }
        public int FeedBackTypeId { get; set; }

        public virtual FeedBackType FeedBackType { get; set; }

    }
}