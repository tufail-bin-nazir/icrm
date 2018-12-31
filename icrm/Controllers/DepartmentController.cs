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
using System.Globalization;
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
            ViewBag.linkName = "openticket";
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenWithDepartment(user.Id,pageIndex, pageSize);

            return View(feedbackList);
        }



        // GET: Agent
        public ActionResult responded(int? page)
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();

            ViewBag.linkName = "respondedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllRespondedWithDepartment(user.Id, pageIndex, pageSize);
            return View("Dashboard",feedbackList);
        }
        

        [HttpGet]
        [Route("view/{id}")]
        public ActionResult view(string name, int? id)
        {
            ViewBag.viewlink = name;


            var departments = db.Departments.OrderByDescending(m => m.name).ToList();
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
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.responseDate = DateTime.Today;
            f.response = feedback.response;
            if (ModelState.IsValid)
            {
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();             
                return RedirectToAction("DashBoard");
            }
            else
            {
                TempData["displayMsg"] = "Information is not Valid";
                return RedirectToAction("view", new { name= "respondedview", id = feedback.id });

            }

        }
        [HttpPost]
        [Route("search/")]
        public ActionResult search(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
            ViewBag.Status = Request.Form["Status"];
            string status= Request.Form["Status"]; ;
            switch(status){
                case "Open":
                    ViewBag.linkName = "openticket";
                break;
                case "Resolved":
                    ViewBag.linkName = "resolvedticket";
                break;
                case "Closed":
                    ViewBag.linkName = "closedticket";
                    break;
                default:
                    ViewBag.linkName = "openticket";
                    break;
            }
            string d3 = Request.Form["date22"];

            string dd = Request.Form["date1"];

            DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
           DateTime dt2= DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);
           
            ViewBag.showDate = Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff");
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewData["user"] = user;
            IPagedList<Feedback> feedbacks = feedInterface.search(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"),status,user.Id, pageIndex, pageSize);
            return View("DashBoard",feedbacks);

        }

        public ActionResult open(int? page, string id)
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
            ViewBag.linkName = id;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenWithDepartment(user.Id.ToString(),pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        public ActionResult respondedview(int? id)
        {
            var departments = db.Departments.OrderByDescending(m => m.name).ToList(); var categories = db.Categories.OrderByDescending(m => m.name).ToList();
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


        public ActionResult openview(int? id)
        {
            var departments = db.Departments.OrderByDescending(m => m.name).ToList(); var categories = db.Categories.OrderByDescending(m => m.name).ToList();
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
                return View("view", f);
            }
        }
        public ActionResult resolvedview(int? id)
        {
            var departments = db.Departments.OrderByDescending(m => m.name).ToList(); var categories = db.Categories.OrderByDescending(m => m.name).ToList();
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

        public ActionResult closedview(int? id)
        {
            var departments = db.Departments.OrderByDescending(m => m.name).ToList(); var categories = db.Categories.OrderByDescending(m => m.name).ToList();
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
    }
}