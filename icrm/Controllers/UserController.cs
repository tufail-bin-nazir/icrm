using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
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

        // GET: User
        public ActionResult DashBoard()
        {
            //GET CURRENTLY LOGGED IN USER BY THIS CODE
            var user = UserManager.FindById(User.Identity.GetUserId());
            Debug.WriteLine(user.FirstName + "99009099090909090909090");
            return View();
        }
    }
}