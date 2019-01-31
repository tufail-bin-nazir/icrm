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
        public static int criticalescelationtime = 1;
        public static string criticallevel1useremail = "";
        public static string criticallevel2useremail = "";
        public static string criticallevel3useremail = "";
        public static string FORWARD = "Forward";
    }
}