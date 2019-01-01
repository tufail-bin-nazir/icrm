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

            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuwxyz0123456789";
            id= new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray()) + "-" + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year;

            createDate = DateTime.Today;
            status = "Open";
        }
        public string id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string title { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createDate { get; set; }

        [Required(ErrorMessage = "Detail is required")]
        public string description { get; set; }

        public string userId { get; set; }
        public virtual ApplicationUser user { get; set; }


        public int? departmentID { get; set; }
        public virtual Department department { get; set; }

        public string status { get; set; }

        public Boolean satisfaction { get; set; }

        public string attachment { get; set; }

        public int? categoryId { get; set; }

        public virtual Category category {get;set;}

        public int? priorityId { get; set; }

        public virtual Priority priority { get; set; }

        public string assignedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? assignedDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? responseDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? closedDate { get; set; }

        public string response { get; set; }

        public string submittedById { get; set; }
        public virtual ApplicationUser submittedBy { get; set; }

    }
}