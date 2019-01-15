using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public string name { get; set; }

        public int departmntNumber { get; set; }

        public IEnumerable<ApplicationUser> Users { get; set; }

    }
}