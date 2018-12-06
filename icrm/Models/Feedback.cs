using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace icrm.Models
{
    public class Feedback
    {

        public int id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }
        [Required(ErrorMessage = "Contact is required")]
        public string contactNo { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; }
        [Required(ErrorMessage = "Type of feedback is required")]
        public string typeOfFeedback { set; get; }
        [Required(ErrorMessage = "Subject is required")]
        public string subject { set; get; }
        [Required(ErrorMessage = "Detail is required")]
        public string details { get; set; }
        public string userId { get; set; }
        public virtual ApplicationUser user { get; set; }
    }
}