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
using Microsoft.AspNet.Identity.EntityFramework;

namespace icrm.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
       

        public HRController() {
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
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpen(pageIndex,pageSize);
            
            return View(feedbackList);
        }


        [HttpGet]
        [Route("feedback/{id}")]
        public ActionResult view(int? id)
        {
            var departments = db.Users.Where(m => m.Roles.Any(s=>s.RoleId == "fdc6f3b2-e87b-4719-909d-569ce5340854")).ToList();
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
        [Route("assigndepart/")]
        public ActionResult assign(Feedback feedback)
        {
            if (feedback.departmentID == null || feedback.categoryId==null || feedback.priorityId==null) {
                TempData["displayMsg"] = "Enter Fields Properly";
                return RedirectToAction("view",new { id=feedback.id});
            }

            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.assignedBy = user.Id;
            feedback.assignedDate = DateTime.Now;
            feedback.userDepartment = db.Users.Find(feedback.departmentID);
            feedback.user = db.Users.Find(feedback.userId);

            if (ModelState.IsValid)
            {
                Debug.WriteLine(feedback.departmentID + "----------fbnds--------------00");
                
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();
                TempData["displayMsg"] ="FeedBack Updated";
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