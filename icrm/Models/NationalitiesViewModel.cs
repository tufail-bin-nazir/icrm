using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class NationalitiesViewModel
    {
        public Nationality Nationality { get; set; }
        public IEnumerable<Nationality> Nationalities { get; set; }
    }
}