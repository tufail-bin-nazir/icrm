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
    }
}