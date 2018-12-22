using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    public class AdminController : Controller
    {
       
        private ApplicationUserManager _userManager;

        public ActionResult Dashboard()
        {
            return View();
        }


        public AdminController()
        {}
        public AdminController(ApplicationUserManager userManager)
        {
            this._userManager = userManager;    
        }

       
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


        [HttpGet]
        public ActionResult AddUser(string id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            ViewBag.Locationlist = db.Locations.ToList();
            ViewBag.Positionlist = db.Positions.ToList();
            ViewBag.Nationalitylist = db.Nationalities.ToList();
            return View();
        }


        public ActionResult postUser(RegisterViewModel model) {

            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (ModelState.IsValid)
            {

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    LocationId = model.LocationId,
                    SubLocationId = model.SubLocationId,
                    PositionId = model.PositionId,
                    NationalityId = model.NationalityId
                };

                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, roleManager.FindByName("User").Name);

                   

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("DashBoard", "User");
                }
                AddErrors(result);
            }

           

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }

   
}