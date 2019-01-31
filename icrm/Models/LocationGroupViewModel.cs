using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class LocationGroupViewModel
    {
        public LocationGroup locationGroup { get; set; }
        public IEnumerable<LocationGroup> locationGroups { get; set; }
    }
}