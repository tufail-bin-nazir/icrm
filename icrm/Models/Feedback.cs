using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace icrm.Models
{
    public class Feedback
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public Feedback() {
            createDate = DateTime.Today;
            status = "Open";
            Random random = new Random();

            int sequenceNumber = db.Feedbacks.Count()+1;
            id = string.Format("IR{0}", sequenceNumber.ToString().PadLeft(5, '0'));




        } 
        public string id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string title { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? departmentAssignedDate { get; set; }

        [Required(ErrorMessage = "Detail is required")]
        public string description { get; set; }

        public string userId { get; set; }
        public virtual ApplicationUser user { get; set; }


        public int? departmentID { get; set; }
        public virtual Department department { get; set; }

        public string status { get; set; }

        public string satisfaction { get; set; }

        public string attachment { get; set; }

        public int? categoryId { get; set; }

        public virtual Category category {get;set;}

        public int? subcategoryId { get; set; }
        public virtual  SubCategory SubCategory { get; set; }

        public string escalationlevel { get; set; }

        public string  internalstatus { get; set; }

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
        public string responseById { get; set; }
        public virtual ApplicationUser responseBy { get; set; }
        public string submittedById { get; set; }
        public virtual ApplicationUser submittedBy { get; set; }
        public int? typeId { get; set; }

        public virtual FeedBackType type { get; set; }
        public string checkStatus { get; set; }

    }
}