using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class Comments
    {
        public Comments() {

            date = DateTime.Today;

           
        }
         
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime date { get; set; }

        public string text { get; set; }
       
        public string commentedById { get; set; }
        [JsonIgnore]
        public ApplicationUser commentedBy { get; set; }
        public string feedbackId { get; set; }
        [JsonIgnore]
        public Feedback feedback { get; set; }

    }
}