using icrm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace icrm
{
    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Data.Entity.Database.SetInitializer(new MyDBInitializer());
        }

        protected static void Session_End(object Sender, EventArgs e)
        {
            //ApplicationUser user = db
            /*LoggedInUsers member = AccountController.OnlineUsers.Find(m => m.MemberName == HttpContext.Current.User.Identity.Name);
            if (member != null)
                AccountController.OnlineUsers.Remove(member);*/
        }
    }
}
