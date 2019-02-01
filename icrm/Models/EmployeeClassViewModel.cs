using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class EmployeeClassViewModel
    {
        public EmployeeClass employeeClass { get; set; }
        public IEnumerable<EmployeeClass> employeeClasses { get; set; }
    }
}