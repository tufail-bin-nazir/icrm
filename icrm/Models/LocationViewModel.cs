using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class LocationViewModel
    {
        public Location Location { get; set; }
        public IEnumerable<Location> Locations { get; set; }
    }
}