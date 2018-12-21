using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using icrm.RepositoryInterface;
using PagedList;
using icrm.Models;
using icrm.RepositoryImpl;
using System.Data.Entity;
using System.Net;

namespace icrm.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
        private IDepartment departInterface;

        public AgentController() {
            feedInterface = new FeedbackRepository();
            departInterface = new DepartmentRepository();
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
            IPagedList<Feedback> feedbackList = feedInterface.getAll(pageIndex,pageSize);
            //GET CURRENTLY LOGGED IN USER BY THIS CODE
            
            return View(feedbackList);
        }


        [HttpGet]
        [Route("feedback/{id}")]
        public ActionResult view(int? id)
        {
            var departments = departInterface.getAll();
            ViewBag.Departmn = departments;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                Feedback f = feedInterface.Find(id);
                return View("view", f);
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("assigndepart/")]
        public ActionResult assign(Feedback feedback)
        {
            if (feedback.departmentID == null) {
                TempData["displayMsg"] = "Choose Department";
                return RedirectToAction("view",new { id=feedback.id});
            }

            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.department = db.Departments.Find(feedback.departmentID);
            feedback.user = db.Users.Find(feedback.userId);

            if (ModelState.IsValid)
            {
                Debug.WriteLine(feedback.departmentID + "----------fbnds--------------00");
                
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();
                TempData["displayMsg"] ="Department Assigned";
                return RedirectToAction("view",new { id = feedback.id });
            }
            else
            {


                TempData["displayMsg"] = "Enter Fields Properly";
                return RedirectToAction("view", new { id = feedback.id });
                
            }

        }

    }
}