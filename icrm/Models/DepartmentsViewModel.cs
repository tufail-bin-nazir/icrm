using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class DepartmentsViewModel
    {
        public Department Department { get; set; }

       
        public IEnumerable<Department> Departments { get; set; }
    }
}