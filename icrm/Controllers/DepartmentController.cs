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
using System.Web.Routing;
using Constants = icrm.Models.Constants;

namespace icrm.Controllers
{
    [Authorize(Roles = "Department")]
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


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (Session["user"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
            }
            base.OnActionExecuting(filterContext);
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

        /**********************Open tickets***********/

        public ActionResult openall(int? page)
        {
            ViewBag.linkName = "openlinkticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenWithDepartment(user.Id, pageIndex, pageSize);
            TicketCounts();
            return View("DashBoard",feedbackList);
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

        /*****All Tickets***/
        public ActionResult alltickets(int? page)
        {
            ViewBag.linkName = "alltickets";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllByDept(user.Id).ToPagedList(pageIndex,pageSize);
            return View("Dashboard", feedbackList);

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
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
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
            f.checkStatus = Constants.RESPONDED;
            f.responseDate = DateTime.Now;

            System.Diagnostics.Debug.WriteLine((DateTime.Now - (DateTime)f.assignedDate) +"-----");
          
            List<Comments> cc = new List<Comments>();
           
            if (Request.Form["responsee"] != "")
            {
                f.timeHours= (DateTime.Now - (DateTime)f.assignedDate).Ticks;


                
                Comments c = new Comments();
                c.text = Request.Form["responsee"];
                c.commentedById = user.Id;
                c.feedbackId = feedback.id;
                c.commentFor = Constants.commentType[0];
                db.comments.Add(c);
                
                    db.SaveChanges();
               

                db.Entry(f).State = EntityState.Modified;
                 db.SaveChanges();
                   
                ViewData["decide"] = feedInterface.getCOmments(feedback.id);
                TempData["displayMsg"] = "Ticket has been Updated Successfully";
                return RedirectToAction("DashBoard");           
            }
            TempData["displayErrMsg"] = "Please fill Comment field";
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
                TempData["DateMsg"] = "Please Select StartDate And EndDate";
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


        /****** GET Open Tickets List ****/

        public ActionResult allopen(int? page)
        {
            ViewBag.linkName = "allopenticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpenByDept(user.Id.ToString()).ToPagedList(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /****** GET Open Tickets List ****/

        public ActionResult allclosed(int? page)
        {
            ViewBag.linkName = "allclosedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllClosedByDept(user.Id.ToString()).ToPagedList(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /****** GET Open Tickets List ****/

        public ActionResult allresolved(int? page)
        {
            ViewBag.linkName = "allresolvedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            TicketCounts();
            IPagedList<Feedback> feedbackList = feedInterface.getAllResolvedByDept(user.Id.ToString()).ToPagedList(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }
        /****** RespondedTickets List***********/

        public ActionResult respondedview(string id)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["commentList"] = feedInterface.getCOmments(id);

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
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

            ViewData["decide"]= feedInterface.getCOmments(id);
            ViewData["commentList"] = feedInterface.getCOmments(id);

            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
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
            ViewData["commentList"] = feedInterface.getCOmments(id);

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
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
            ViewData["commentList"] = feedInterface.getCOmments(id);

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }


        public ActionResult viewticket(string id)
        {

            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["commentList"] = feedInterface.getCOmments(id);

            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                ViewData["decide"] = feedInterface.getCOmments(id);
                return View("view",f);
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
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.TotalTickets = feedInterface.getAllByDept((user.Id)).Count();
            ViewBag.OpenTickets = feedInterface.getAllOpenByDept(user.Id).Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosedByDept(user.Id).Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolvedByDept(user.Id).Count();
        }

        /*****Get Attribute List***************/

        public void getAttributeList() {
           
          ViewBag.Departmn = feedInterface.getDepartmentsOnType(Constants.FORWARD);
           
            ViewBag.Priorities = feedInterface.getPriorties();

        }


       
    }
}