using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class GenderViewModel
    {
        public Gender Gender { get; set; }
        public IEnumerable<Gender> Genders { get; set; }
    }
}