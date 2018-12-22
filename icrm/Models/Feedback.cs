using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace icrm.Models
{
    public class Feedback
    {
        public Feedback() {
            createDate = DateTime.Now;
            status = "Open";
        }
        public int id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string title { get; set; }

        public DateTime createDate { get; set; }

        [Required(ErrorMessage = "Detail is required")]
        public string description { get; set; }

        public string userId { get; set; }
        public virtual ApplicationUser user { get; set; }

        public int? departmentID { get; set; }
        public virtual ApplicationUser userDepartment { get; set; }

        public string status { get; set; }

        public Boolean satisfaction { get; set; }

        public string attachment { get; set; }

        public int? categoryId { get; set; }

        public virtual Category category {get;set;}

        public int? priorityId { get; set; }

        public virtual Priority priority { get; set; }

    }
}