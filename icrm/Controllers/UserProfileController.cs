using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
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

        ApplicationDbContext context = new ApplicationDbContext();
        // GET: UserProfile
        public ActionResult Index()
        {
            ViewBag.Locationlist = context.Locations.ToList();
            ViewBag.Positionlist = context.Positions.ToList();
            ViewBag.Nationalitylist = context.Nationalities.ToList();
            ViewBag.Jobtitlelist = context.JobTitles.ToList();
            ViewBag.Departmentlist = context.Departments.ToList();
            ViewBag.PayScaleTypeList = context.PayScaleTypes.ToList();
            ViewBag.Religionlist = context.Religions.ToList();
            return View();
        }

        public ActionResult updateProfile(UserProfileViewModel userProfile) {
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.LocationId = userProfile.LocationId;
            user.PositionId = userProfile.PositionId;
            user.NationalityId = userProfile.NationalityId;
            user.JobTitleId = userProfile.JobTitleId;
            user.DepartmentId = userProfile.DepartmentId;
            user.PayScaleTypeId = userProfile.PayScaleTypeId;
            user.ReligionId = userProfile.ReligionId;
            UserManager.Update(user);

            return RedirectToAction("UserProfile",userProfile);


        }
    }
}