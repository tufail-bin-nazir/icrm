using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EmployerTypeViewModel
    {
        public EmployerType employerType { get; set; }
        public IEnumerable<EmployerType> employerTypes { get; set; }
    }
}