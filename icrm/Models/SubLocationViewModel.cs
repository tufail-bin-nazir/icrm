using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class SubLocationViewModel
    {
        public SubLocation SubLocation { get; set; }
        public IEnumerable<SubLocation> SubLocations { get; set; }
    }
}