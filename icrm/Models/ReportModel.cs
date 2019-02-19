using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class ReportModel
    {
        [Display(Name ="Ticket Id")]
        public String ticketId { get; set; }

        [Display(Name = "Title")]
        public String title { get; set; }

        [Display(Name = "Incident Type")]
        public String incidentType { get; set; }

        [Display(Name = "Status")]
        public String  status { get; set; }

        [Display(Name = "Description")]
        public String description { get; set; }

        [Display(Name = "Department")]
        public String departmentName { get; set; }

        [Display(Name = "Category")]
        public String category { get; set; }

        [Display(Name = "Name")]
        public String name { get; set; }

        [Display(Name = "Batch Number")]
        public int batchNumber { get; set; }

        [Display(Name = "Position")]
        public String position { get; set; }

        [Display(Name = "Nationality")]
        public String nationality { get; set; }

        [Display(Name = "Email ID")]
        public String emailId { get; set; }

        [Display(Name = "Phone Number")]
        public String phoneNumber { get; set; }


        [Display(Name = "Created Date")]
        public DateTime createdDate { get; set; }

        public String responseTime { get; set; }


        [Display(Name = "Created BY")]
        public String createdBy { get; set; }


        [Display(Name = "Source/Location")]
        public String source { get; set; }

        [Display(Name = "Priority")]
        public String priority { get; set; }

        [Display(Name = "Owner")]
        public String owner { get; set; }


        [Display(Name = "Escalated To")]
        public String escalatedto { get; set; }

        [Display(Name = "Is Escalated")]
        public String isescalated { get; set; }


    }
}