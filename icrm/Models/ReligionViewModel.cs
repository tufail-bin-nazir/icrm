using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class ReligionViewModel
    {
        public Religion Religion { get; set; }
        public IEnumerable<Religion> Religions { get; set; }
    }
}