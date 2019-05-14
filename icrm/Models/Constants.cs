using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public static class Constants
    {
        public static List<string> statusList = new List<string>(new string[] { "Open", "Closed", "Resolved" });
        public static string SATISFIED = "Satisfied";
        public static string UN_SATISFIED = "UnSatisfied";
        public  static string PATH = @"E:\ICRMImages\" ;
        public static string ROLE_HR = "HR";
        public static string OPEN = "Open";
        public static string RESOLVED = "Resolved";
        public static string ASSIGNED = "Assigned";
        public static string RESPONDED = "Responded";
        public static string REJECTED = "Rejected";
        public static string CLOSED = "Closed";
        public static string Enquiry = "Enquiry";
        public static string Suggestion = "Suggestion";
        public static int criticalescelationtime = 1;
        public static string criticallevel1useremail = "kmiraste@gmail.com";
        public static string criticallevel2useremail = "khursheed@stie.com.sg";
        public static string criticallevel3useremail = "tufail.b.n@gmail.com";
        public static string FORWARD = "Forward";
        
        public static int highescelationtime = 12;
        public static int mediumescelationtime = 24;
        public static int lowescelationtime = 48;
        public static string Complaints = "Complaint";
        public static string OPERATIONS = "Operations";
        public static List<string> commentType = new List<string>(new string[] {"Hr","Department","User" });
    }
}