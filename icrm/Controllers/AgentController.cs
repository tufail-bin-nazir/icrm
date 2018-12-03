using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace icrm.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Agent
        public ActionResult DashBoard()
        {
            //GET CURRENTLY LOGGED IN USER BY THIS CODE
            var user = UserManager.FindById(User.Identity.GetUserId());
            Debug.WriteLine(user.FirstName + "99009099090909090909090");
            return View();
        }
    }
}