using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class VendorViewModel
    {
        public Vendor Vendor { get; set; }
        public IEnumerable<Vendor> Vendors { get; set; }
    }
}