using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class BandViewModel
    {
        public Band Band { get; set; }

        public IEnumerable<Band> Bands { get; set; }

    }
}