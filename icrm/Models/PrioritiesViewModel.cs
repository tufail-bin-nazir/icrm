using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class PrioritiesViewModel
    {
        public Priority Priority { get; set; }
        public IEnumerable<Priority> Priorities { get; set; }
    }
}