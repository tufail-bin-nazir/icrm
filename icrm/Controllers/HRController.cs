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
using System.Globalization;
using System.IO;

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

        // GET: HR DAshboard
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
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex,pageSize);          
            return View(feedbackList);
        }


        //HR Ticket Raise
        [HttpGet]
        [Route("hr/feedback/")]
        public ActionResult Create()
        {
            

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
              var userRole = roleManager.FindByName("User").Users.First();
            
            var departRole = roleManager.FindByName("Department").Users.First();
            Debug.WriteLine(userRole + "------------------iiiiii");


            var departments = db.Departments.OrderByDescending(m=>m.name).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
           
               ViewBag.Departmn = departments;
                ViewBag.Categories = categories;
                ViewBag.Priorities = priorities;
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId==userRole.RoleId)).ToList();
            return View();
            }

        




        [HttpGet]
        [Route("feedback/{id}")]
        public ActionResult view(int? id)
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



        [HttpPost]
        [Route("hr/feedback/")]
        public ActionResult Create(int? id,string submitButton, Feedback feedback, HttpPostedFileBase file)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            var userRole = roleManager.FindByName("User").Users.First();
            var departments = db.Departments.OrderByDescending(m => m.name).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();

            ViewBag.Departmn = departments;
            ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
            ViewBag.Categories = categories;
            ViewBag.Priorities = priorities;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            if (feedback.userId == null) {
                feedback.userId=user.Id;
            }
            
            var fileSize = file.ContentLength;
            if (fileSize > 10 * 1024 * 1024)
            {

                
                TempData["Message"] = "File Size Limit Exceeds";
                return View("Create", feedback);
            }
            else
            {
                switch (submitButton)
                {
                    case "Forward":
                        if (feedback.departmentID != null && feedback.response == null)
                        {
                            if (ModelState.IsValid)
                            {
                                string filename = null;
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Today;
                                if (file != null && file.ContentLength > 0)
                                {
                                    String ext = Path.GetExtension(file.FileName);
                                    filename = $@"{Guid.NewGuid()}" + ext;
                                    feedback.attachment = filename;
                                    file.SaveAs(Path.Combine(icrm.Models.Constants.PATH, filename));
                                }
                                feedInterface.Save(feedback);
                                TempData["Message"] = "Feedback Saved";
                               
                            }
                            else
                            {
                                ViewData["user"] = user;
                                TempData["Message"] = "Fill feedback Properly";
                                return View("Create", feedback);
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Either Select Department/Comment field should be empty";
                            return View("Create", feedback);
                        }
                        return View("Create");


                    case "Resolve":
                        if (feedback.departmentID == null && feedback.response != null)
                        {
                            if (ModelState.IsValid)
                            {
                                String filename = null;
                                if (file != null && file.ContentLength > 0)
                                {
                                    String ext = Path.GetExtension(file.FileName);
                                    filename = $@"{Guid.NewGuid()}" + ext;
                                    feedback.attachment = filename;
                                    file.SaveAs(Path.Combine(icrm.Models.Constants.PATH, filename));
                                }
                                feedInterface.Save(feedback);
                                TempData["Message"] = "Feedback Saved";
                                return RedirectToAction("Create");
                            }
                            else
                            {
                                ViewData["user"] = user;
                                TempData["Message"] = "Fill feedback Properly";
                                return View("Create", feedback);
                            }

                        }
                        else
                        {
                            TempData["Message"] = "Response Field is Empty/ Clear department";
                            return View("Create", feedback);
                        }
                      
                    default:
                        return RedirectToAction("Create");
                     

                }

            }
            

        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("update/")]
        public ActionResult update(Feedback feedback)
        {
            string type=Request.Form["typeoflink"];

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            var userRole = roleManager.FindByName("User").Users.First();
            var departments = db.Departments.OrderByDescending(m => m.name).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();

            ViewBag.Departmn = departments;
            ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
            ViewBag.Categories = categories;
            ViewBag.Priorities = priorities;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.status = feedback.status;
            
            if (feedback.status == "Closed") {
                f.closedDate = DateTime.Today;
            }
           
            switch(type)
            {
                case "Resolvedtype":
                    if (ModelState.IsValid)
                    {

                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    @TempData["Message"] = "Updated";
                    return View("resolvedview",feedback);
                case "Respondedtype":
                    if (ModelState.IsValid)
                    {

                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    @TempData["Message"] = "Updated";
                    return View("respondedview", feedback);
                case "Assignedtype":
                    f.response = feedback.response;
                    f.responseDate = DateTime.Today;
                    if (ModelState.IsValid)
                    {

                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    @TempData["Message"] = "Updated";
                    return View("assignedview", feedback);
                 default:
                    return View("Assignedtype", feedback);
                  
            }
           
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("assigndepart/")]
        public ActionResult assign(string submitButton,Feedback feedback)
        {
            Debug.WriteLine(submitButton+"------------------");
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.assignedBy = user.Id;
            feedback.assignedDate = DateTime.Today;
           
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && feedback.response == null)
                    {
                        feedback.department = db.Departments.Find(feedback.departmentID);
                        if (ModelState.IsValid)
                        {
                            feedback.assignedBy = user.Id;
                            feedback.assignedDate = DateTime.Today;

                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Message"] = "Feedback Forwarded";

                        }
                        else
                        {
                            ViewData["user"] = user;
                            TempData["Message"] = "Fill feedback Properly";
                            return RedirectToAction("view", new { id = feedback.id });
                        }
                    }
                    else
                    {
                        TempData["Message"] = "Either Select Department or Comment field should be empty";
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                    return RedirectToAction("view", new { id = feedback.id });


                case "Resolve":
                    if (feedback.departmentID == null && feedback.response != null)
                    {
                        if (ModelState.IsValid)
                        {


                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Message"] = "Feedback Resolved";
                            return RedirectToAction("view", new { id = feedback.id });
                        }
                        else
                        {
                          
                            TempData["Message"] = "Fill feedback Properly";
                            return RedirectToAction("view", new { id = feedback.id });
                        }

                    }
                    else
                    {
                        TempData["Message"] = "Either Empty Response Field or Deselect department";
                        return RedirectToAction("view", new { id = feedback.id });
                    }

                default:
                    return RedirectToAction("view", new { id = feedback.id });


            }


        }

        [HttpPost]
        [Route("hr/search/")]
        public ActionResult search(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
            ViewBag.Status = Request.Form["Status"];
            string status = Request.Form["Status"]; ;
            switch (status)
            {
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
            DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            ViewBag.showDate = Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff");
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewData["user"] = user;
            IPagedList<Feedback> feedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status, pageIndex, pageSize);
            return View("DashBoard", feedbacks);

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
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult assigned(int? page, string id)
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
            IPagedList<Feedback> feedbackList = feedInterface.getAllAssigned(pageIndex, pageSize);
            return View("Dashboard",feedbackList);
        }

        public ActionResult resolved(int? page,string id)
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
            IPagedList<Feedback> feedbackList = feedInterface.getAllResolved(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult responded(int? page, string id)
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
            IPagedList<Feedback> feedbackList = feedInterface.getAllResponded(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult Closed(int? page, string id)
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
            IPagedList<Feedback> feedbackList = feedInterface.getAllClosed(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
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
                return View("view",f);
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


        public ActionResult assignedview(int? id)
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


        public void DownloadFile(string filename)
        {
            string myfile = Models.Constants.PATH + filename;
            
            var fi = new FileInfo(myfile);
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fi.Name);
            Response.WriteFile(myfile);
            Response.End();
        }

        
    }
}