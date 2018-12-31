using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public static class Constants
    {
        static List<string> statusList = new List<string>(new string[] { "Open", "Closed", "Resolved" });
       public  static string PATH = "~/Images/" ;
    }
}