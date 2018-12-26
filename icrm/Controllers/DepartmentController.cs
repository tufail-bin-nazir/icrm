using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    public class DepartmentController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;


        public DepartmentController()
        {
            feedInterface = new FeedbackRepository();

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

        // GET: Agent
        public ActionResult DashBoard(int? page)
        {

            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllWithDepartment(user.Id,pageIndex, pageSize);
            return View(feedbackList);
        }



        [HttpGet]
        [Route("view/{id}")]
        public ActionResult view(int? id)
        {
            var departments = db.Users.Where(m => m.Roles.Any(s => s.RoleId == "fdc6f3b2-e87b-4719-909d-569ce5340854")).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                ViewBag.Departmn = departments;
                ViewBag.Categories = categories;
                ViewBag.Priorities = priorities;
                Feedback f = feedInterface.Find(id);


                return View(f);
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("response/")]
        public ActionResult response(Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.responseDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();             
                return RedirectToAction("DashBoard");
            }
            else
            {
                TempData["displayMsg"] = "Information is not Valid";
                return RedirectToAction("view", new { id = feedback.id });

            }

        }

    }
}