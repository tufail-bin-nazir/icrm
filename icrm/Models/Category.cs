using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string name { get; set; }

        public IEnumerable<SubCategory> subCategories { get; set; }
        public int DepartmentId { get; set; }
        public int? EscalationUserId { get; set; }



    }
}