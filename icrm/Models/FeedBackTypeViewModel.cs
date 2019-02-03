using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class FeedBackTypeViewModel
    {
        public FeedBackType FeedBackType { get; set; }

        public IEnumerable<FeedBackType> FeedBackTypes { get; set; }
    }
}