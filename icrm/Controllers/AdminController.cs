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
using System.Diagnostics;
using icrm.RepositoryInterface;
using icrm.RepositoryImpl;

namespace icrm.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private IFeedback feedInterface;

        private ApplicationUserManager _userManager;

        public ActionResult Dashboard()
        {
            TicketCounts();
            return View();
        }


        public AdminController()
        {
            feedInterface = new FeedbackRepository();
        }
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
            ApplicationDbContext context = new ApplicationDbContext();
            ViewBag.rolename = id;
            if (Request.Cookies["sucess"] != null) {
                Response.SetCookie(new HttpCookie("sucess", "") { Expires = DateTime.Now.AddDays(-1) });
                ViewBag.message = "User Saved Sucessfully";
            }
            ViewBag.DepartmentList = context.Departments.ToList();
                return View();
        }


        public ActionResult postUser(RegisterViewModel model,string rolename) {

            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (ModelState.IsValid)
            {

                var user = new ApplicationUser
                {
                    EmployeeId = model.EmployeeId,
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };

                if (rolename.Equals("Department")) {
                    user.DepartmentId = model.DepartmentId;
                }

                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    if(rolename.Equals("HR"))
                         UserManager.AddToRole(user.Id, roleManager.FindByName("HR").Name);
                    else if (rolename.Equals("Department"))
                        UserManager.AddToRole(user.Id, roleManager.FindByName("Department").Name);



                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    Response.SetCookie(new HttpCookie("sucess", ""));
                    return RedirectToAction("AddUser", new { @id= rolename });
                }
                AddErrors(result);
            }



            // If we got this far, something failed, redisplay form
            if (rolename.Equals("Department")) {
                ViewBag.DepartmentList = context.Departments.ToList();
            }
            ViewBag.rolename = rolename;
            return View("AddUser",model);
          
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpGet]
        public ViewResult ListUser() {
            ApplicationDbContext db = new ApplicationDbContext();
            
            return View(db.Users.ToList());
        }

        /****** Get Ticket Counts********/
        public void TicketCounts()
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
        }
    }


   
}