using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

using System.Web.Mvc;

namespace icrm.Models
{
    public class CultureAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var culture = "";
            if (filterContext.HttpContext.Session["culture"] as String != null)
            {
                culture = filterContext.HttpContext.Session["culture"] as String;
            }
            else
            {
                culture = "en-US";
            }
            //Retreive culture from GET
            // string currentCulture = filterContext.HttpContext.Request.QueryString["culture"];

            // Also, you can retreive culture from Cookie like this :
           // string currentCulture = filterContext.HttpContext.Session["culture"] as String;

            // Set culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture);
        }
    }
}
