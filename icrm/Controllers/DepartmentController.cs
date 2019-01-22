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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Constants = icrm.Models.Constants;

namespace icrm.Controllers
{
    public class DepartmentController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;


        public DepartmentController()
        {
            ViewBag.Status = Models.Constants.statusList;
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

           /**********************DashBoard***********/

        public ActionResult DashBoard(int? page)
        {
            ViewBag.linkName = "openticket";         
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenWithDepartment(user.Id,pageIndex, pageSize);
            TicketCounts();
            return View(feedbackList);
        }


        /**Responded Tickets*********************/
     
        public ActionResult responded(int? page)
        {        
            ViewBag.linkName = "respondedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllRespondedWithDepartment(user.Id, pageIndex, pageSize);
            return View("Dashboard",feedbackList);

        }
        
        /*******Department TicketView**************/ 

        [HttpGet]
        [Route("view/{id}")]
        public ActionResult view(string name, string id)
        {
            ViewBag.viewlink = name;
           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }

        }


        /******** Response By Department User Post******/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("response/")]
        public ActionResult response(Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            Feedback f = db.Feedbacks.Find(feedback.id);

            List<Comments> cc = new List<Comments>();
           
            if (Request.Form["responsee"] != "")
            {


                Comments c = new Comments();
                c.text = Request.Form["responsee"];
                c.commentedById = user.Id;
                c.feedbackId = feedback.id;
                db.comments.Add(c);
                db.SaveChanges();

                f.checkStatus = Constants.RESPONDED;
                // f.response = cc;
                // f.response = feedback.response;
                //f.responseById = user.Id;
                //f.responseBy = db.Users.Find(user.Id);
                //f.responseDate = DateTime.Today;

                if (ModelState.IsValid)
                {

                    db.Entry(f).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["displayMsg"] = "FeedBacK Updated";
                    ViewData["decide"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();
                    return RedirectToAction("DashBoard");
                }
                else
                {
                    TempData["displayMsg"] = "Information is not Valid";
                    ViewData["decide"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();
                    return RedirectToAction("view", new { name = "respondedview", id = feedback.id });
                }
            }
            TempData["displayMsg"] = "Response field is empty";
            return RedirectToAction("view", new { name = "respondedview", id = feedback.id });

        }

        /******* Search in Department****/
        [HttpGet]
        [Route("department/search/")]
        public ActionResult search(int? page,string status,string date22,string date1,string export)
        {
            string d3 = date22;
            string dd = date1;
            if (d3.Equals("")|| dd.Equals(""))
            {
                TempData["DateMsg"] = "Select StartDate And EndDate";
                return RedirectToAction("Dashboard");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);


                ViewBag.Search = export;
                ViewBag.statuss = status;
                ViewBag.startDate = date1;
                ViewBag.endDate = date22;
                var user = UserManager.FindById(User.Identity.GetUserId());
                TicketCounts();
                switch (status)
                {
                    case "Open":
                        ViewBag.linkName = "openticket";
                        break;
                    case "Closed":
                        ViewBag.linkName = "closedticket";
                        break;
                    case "Resolved":
                        ViewBag.linkName = "resolvedticket";
                        break;
                    case "Rejected":
                        ViewBag.linkName = "rejectedticket";
                        break;
                    default:
                        break;
                }
                ViewBag.showDate = Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff");
                int pageSize = 10;
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewData["user"] = user;
                IPagedList<Feedback> feedbacks = feedInterface.search(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status, user.Id, pageIndex, pageSize);
                ViewBag.Status = Models.Constants.statusList;
                return View("DashBoard", feedbacks);
            }
        }

        /****** GET Open Tickets List ****/

        public ActionResult open(int? page)
        {
            ViewBag.linkName = "openticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenWithDepartment(user.Id.ToString(),pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /****** RespondedTickets List***********/

        public ActionResult respondedview(string id)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["commentList"] = db.comments.Where(m => m.feedbackId ==id).ToList();

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }

        /*******Get Open View*******/

        public ActionResult openview(string id)
        {

            ViewData["decide"]= db.comments.Where(m => m.feedbackId ==id).ToList();
            ViewData["commentList"] = db.comments.Where(m => m.feedbackId == id).ToList();

            ViewBag.co = db.comments.Where(m => m.feedbackId == id).ToList().Count();
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View("view", f);
            }
        }

        /****** Get Resolved Ticket View*****/
        public ActionResult resolvedview(string id)
        {
           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["commentList"] = db.comments.Where(m => m.feedbackId == id).ToList();

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }

        /***** Get Closed View*****/

        public ActionResult closedview(string id)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["commentList"] = db.comments.Where(m => m.feedbackId == id).ToList();

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }
        
        /*******************DOWNLOAD FILE********************/

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


        /****** Get Ticket Counts********/

        public void TicketCounts() {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
        }

        /*****Get Attribute List***************/

        public void getAttributeList() {
            var departments = db.Departments.OrderByDescending(m => m.name).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();

            ViewBag.Departmn = departments;
            ViewBag.Categories = categories;
            ViewBag.Priorities = priorities;

        }
    }
}