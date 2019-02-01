using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EthnicitesViewModel
    {
        public Ethnicity Ethnicity { get; set; }
        public IEnumerable<Ethnicity> Ethnicities { get; set; }
    }
}