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
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Net.Http;
using System.Web.UI.WebControls;
using System.Web.UI;
using Constants = icrm.Models.Constants;
using Comments = icrm.Models.Comments;
using System.Net.Mail;
using System.Net.Mime;
using icrm.Events;
using System.Drawing.Imaging;
using System.Web.Routing;



namespace icrm.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
        private EventService eventService;
       

        public HRController() {
            ViewBag.Status = Models.Constants.statusList;
            feedInterface = new FeedbackRepository();
            eventService = new EventService();
           
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

        // GET: HR DAshboard
        public ActionResult DashBoard(int? page)
        {
            ViewBag.linkName = "openticket";
            TicketCounts();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex,pageSize);          
            return View(feedbackList);
        }


        /*************** Get HR Ticket Raise by HR****************/
        [HttpGet]
        [Route("hr/feedback/")]
        public ActionResult Create()
        {
             var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
              var userRole = roleManager.FindByName("User").Users.FirstOrDefault();
            if (userRole!=null) {
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
              }         
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            getAttributeList();
            return View();
            }

        /*****************FeedBack View**********************/
        [HttpGet]
        [Route("feedback/{id}")]
        public ActionResult view(string id)
        {           
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


        /************HR FeedBack Post*******************/
        [HttpPost]
        [Route("hr/feedback/")]
        public ActionResult Create(int? id,string submitButton, Feedback feedback, HttpPostedFileBase file)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.FindByName("User").Users.FirstOrDefault() != null)
            {
                var userRole = roleManager.FindByName("User").Users.First();
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
            }
            getAttributeList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

           

            if (feedback.userId == null) {
                feedback.userId=user.Id;
            }
            if (file != null)
            {
                var fileSize = file.ContentLength;
                if (fileSize > 2100000)
                {
                    TempData["Message"] = "File Size Limit Exceeds";
                    return View("Create", feedback);
                }
                else
                {
                    string filename = null;
                    String ext = Path.GetExtension(file.FileName);
                    filename = feedback.id + ext;
                    feedback.attachment = filename;
                    file.SaveAs(Path.Combine(icrm.Models.Constants.PATH, filename));
                }
            }
            else
            {
                feedback.attachment = null;
            }
                switch (submitButton)
                {
                    case "Forward/Create":
                        if (feedback.departmentID != null && Request.Form["responsee"] == "")
                        {
                        ApplicationUser deptUser;
                       Department dep = db.Departments.Find(feedback.departmentID);
                        if (dep.name == Constants.OPERATIONS)
                        {
                             
                            deptUser = feedInterface.getOperationsEscalationUser(Convert.ToInt32(Request.Form["costcentrId"]));
                            
                             
                        }
                        else
                        {
                            deptUser = feedInterface.getEscalationUser(feedback.departmentID, feedback.categoryId);
                            
                        }
                        
                        feedback.departUserId = deptUser.Id;
                        feedback.departUser = deptUser;
                        if (ModelState.IsValid)
                            {                                
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;
                                feedback.checkStatus = Constants.ASSIGNED;                               
                                feedInterface.Save(feedback);
                                TempData["MessageSuccess"] = "Ticket has been Forwarded Successfully";
                                eventService.sendEmails(Request.Form["emailsss"]+","+feedback.departUser.bussinessEmail, PopulateBody(feedback));
                        }
                            else
                            {
                                ViewData["user"] = user;
                                TempData["Message"] = "Ticket details are not valid,Fill details properly";
                                return View("Create", feedback);
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Comment field should be empty";
                            return View("Create", feedback);
                        }
                        return RedirectToAction("DashBoard");


                    case "Submit":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                        if (feedback.status == Constants.CLOSED)
                        {
                            feedback.closedDate = DateTime.Now;
                            feedback.checkStatus = Constants.CLOSED;
                        }
                        else
                        {
                            feedback.resolvedDate = DateTime.Now;
                            feedback.checkStatus = Constants.RESOLVED;
                        }

                        if (ModelState.IsValid)
                        {
                            feedback.submittedById = user.Id;
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedInterface.Save(feedback);
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Created Successfully";
                            return RedirectToAction("DashBoard");
                        }
                        else
                        {
                            ViewData["user"] = user;
                            TempData["Message"] = "Ticket details are not valid,Fill details properly";
                            return View("Create", feedback);
                        }

                    }
                    else
                    {
                        if (feedback.departmentID != null)
                        {
                            TempData["Message"] = "Department should be empty";
                        }
                        else
                        {

                            TempData["Message"] = "Comment Field should not be empty";
                        }
                        return View("Create", feedback);

                    }
                default:
                        return RedirectToAction("Create");                    
                }
        }


        /*****************Change Status By HR in FeedBack*********************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("update/")]
        public ActionResult update(Feedback feedback)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            getAttributeList();
            string type=Request.Form["typeoflink"];
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.FindByName("User").Users.FirstOrDefault() != null)
            {
                var userRole = roleManager.FindByName("User").Users.First();
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();

            }                     
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.satisfaction = feedback.satisfaction;
            f.status = feedback.status;
            if (feedback.status == Constants.CLOSED)
            {
                f.closedDate = DateTime.Now;
                f.checkStatus = Constants.CLOSED;
            }
            else if (feedback.status == Constants.RESOLVED)
            {
                f.resolvedDate = DateTime.Now;
                f.checkStatus = Constants.RESOLVED;
            }
            else {
                f.checkStatus = Constants.RESPONDED;
            }
            switch (type)
            {
                case "Resolvedtype":
                    if (ModelState.IsValid)
                    {
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                        @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                        return RedirectToAction("dashBoard");
                    }
                    else {
                        @TempData["Message"] = "Please enter fields properly";
                        return View("resolvedview", feedback);
                    }
                         
                case "Respondedtype":
                    if (Request.Form["responsee"] != "")
                    {
                        Comments c = new Comments();
                        c.text = Request.Form["responsee"];
                        c.commentedById = user.Id;
                        c.feedbackId = feedback.id;
                        db.comments.Add(c);
                        db.SaveChanges();
                    }

                    if (ModelState.IsValid)
                    {
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();

                        @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                        ViewData["commentList"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();
                        return RedirectToAction("DashBoard");
                    }
                    else {
                        @TempData["Message"] = "Please enter fields properly";
                        ViewData["commentList"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();
                        return View("respondedview", feedback);
                    }
                case "Assignedtype":                  
                    if (ModelState.IsValid)
                    {                       
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                        @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";

                        return RedirectToAction("DashBoard");
                    }
                    else
                    {
                        @TempData["Message"] = "Please enter fields properly";
                        return View("assignedview", feedback);

                    }

                case "Rejectedtype":
                    if (ModelState.IsValid)
                    {
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                        @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                        return RedirectToAction("DashBoard");
                        
                    }
                    else {
                        @TempData["Message"] = "Please enter fields properly";
                        return View("rejectedview", feedback);
                    }
                      
                default:
                    return View("Assignedtype", feedback);
                  
            }
           
        }

        /**************ASSIGN DEPARTMENT************************/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("assigndepartment/")]
        public ActionResult assign(string submitButton,Feedback feedback)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && Request.Form["responsee"] == "")
                    {
                        db.Feedbacks.Attach(feedback);
                        ApplicationUser deptUser;
                        Department dep = db.Departments.Find(feedback.departmentID);
                        if (dep.name == Constants.OPERATIONS)
                        {
                            deptUser = feedInterface.getOperationsEscalationUser(Convert.ToInt32(Request.Form["costcentrId"]));
                        }
                        else
                        {
                            deptUser = feedInterface.getEscalationUser(feedback.departmentID, feedback.categoryId);
                        }
                        deptUser=  db.Users.Find(deptUser.Id);
                        feedback.departUserId = deptUser.Id;
                        feedback.departUser = deptUser;

                        feedback.departmentID = feedback.departmentID;
                        feedback.checkStatus = Constants.ASSIGNED;
                       
                        if (feedback.assignedDate == null)
                            {
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;                               
                            }
                       
                        db.Entry(feedback).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["MessageSuccess"] = "Ticket has been Forwarded Successfully";
                             eventService.sendEmails(Request.Form["emailsss"], PopulateBody(feedback));                      
                    }
                    else
                    {
                             TempData["Message"] = "Comment field should be empty";

                             return RedirectToAction("view", new { id = feedback.id });
                    }
                    return RedirectToAction("DashBoard");
                case "Submit":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                        if (feedback.status == Constants.CLOSED)
                        {
                            feedback.closedDate = DateTime.Now;
                            feedback.checkStatus = Constants.CLOSED;
                            TempData["MessageSuccess"] = "Ticket has been Closed Successfully";
                        }
                        else {
                            feedback.resolvedDate = DateTime.Now;
                            feedback.checkStatus = Constants.RESOLVED;
                            TempData["MessageSuccess"] = "Ticket has been Resolved Successfully";
                        }

                        
                        Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.submittedById = user.Id;
                            
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            
                            return RedirectToAction("DashBoard");                     
                    }
                    else
                    {
                        if (feedback.departmentID != null)
                        {
                            TempData["Message"] = "Department should be empty";
                        }
                        else
                        {

                            TempData["Message"] = "Comment Field should not be empty";
                        }
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                  case "Reject":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "") {
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.submittedById = user.Id;          
                            feedback.checkStatus = Constants.REJECTED;
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Rejected";
                            return RedirectToAction("DashBoard");
                    }
                    else
                    {
                        if (feedback.departmentID != null)
                        {
                            TempData["Message"] = "Department should be empty";
                        }
                        else
                        {

                            TempData["Message"] = "Comment Field should not be empty";
                        }
                       
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                default:
                    return RedirectToAction("view", new { id = feedback.id });
            }
        }

        /****************************SEARCH BY HR**************************/
        [HttpGet]
        [Route("hr/search/")]
        public ActionResult search(int? page, string status, string date22, string date1, string export)
        {

           
            string d3 = date22;           
            string dd = date1;
            if (d3.Equals("") || dd.Equals(""))
            {
                TempData["DateMsg"] = "Please Select StartDate And EndDate";
                return RedirectToAction("Dashboard");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                IEnumerable<Feedback> excelfeedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status);
                List<ReportModel> reports = new List<ReportModel>();
                foreach (Feedback f in excelfeedbacks) {
                    ReportModel report = new ReportModel();
                    report.ticketId = f.id;
                    report.title = f.title;
                    report.incidentType = f.type.name;
                    report.status = f.status;
                    report.description = f.description;
                    report.departmentName = f.department== null ? "": f.department.name ;
                    report.category = f.category == null ? "" : f.category.name;
                    report.name = f.user.FirstName;
                    report.batchNumber = f.user.EmployeeId;
                    report.position = f.user.JobTitle.name;
                    report.nationality = f.user.Nationality.name;
                    report.emailId = f.user.bussinessEmail;
                    report.phoneNumber = f.user.bussinessPhoneNumber;
                    report.createdDate = f.createDate;
                    report.createdBy = f.submittedBy == null ? f.user.FirstName : "ICRM AGENT";
                    

                    var totalsecs = TimeSpan.FromTicks(f.timeHours).TotalSeconds;
                    var sec = totalsecs % 60;

                    var totalminutes = totalsecs / 60;

                    var totalhours = totalminutes / 60;
                    var minutes = totalminutes % 60;

                    var days = totalhours / 24;
                    var hours = totalhours % 24;
                    report.responseTime = String.Format("{0} days,{1} hours, {2} minutes, {3} seconds",
                                          Math.Round(days),Math.Round(hours), Math.Round(minutes), Math.Round(sec)); 
                    report.source = f.user.CostCenter.name;
                    report.priority = f.priority == null ? "": f.priority.name;
                    report.owner = f.departUser == null? "": f.departUser.FirstName;
                    report.isescalated = f.escalationlevel == null ? "No" : "Yes";
                    reports.Add(report);
                }
                if (export == "excel")
                {
                    
                    var grid = new GridView();
                    grid.DataSource = reports;
                    grid.DataBind();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachement; filename=data.xls");
                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.ContentType = "application/excel";
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    grid.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                    return View();
                }
                else
                {
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
                    IPagedList<Feedback> feedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status, pageIndex, pageSize);
                    ViewBag.Status = Models.Constants.statusList;

                    return View("DashBoard", feedbacks);


                }

            }
            

        }

        /**********GET ALL TICKETS*******************/

        public ActionResult alltickets(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "Alltickets";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAll(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }




        /*****************OPEN TICKETS LIST********************/

        public ActionResult open(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }



        public ActionResult openAll(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openticket";

            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult openAlltickets(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openAllticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpen(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult assigned(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "assignedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllAssigned(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************RESOLVED TICKETS LIST********************/

        public ActionResult resolved(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "resolvedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllResolved(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /*****************RESPONDED TICKETS LIST********************/

        public ActionResult responded(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "respondedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllResponded(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************CLOSED TICKETS LIST********************/

        public ActionResult Closed(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "closedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllClosed(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************REJECTED TICKETS LIST********************/

        public ActionResult Rejected(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "rejectedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllRejected(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /*****************ENQUIRIES TICKETS LIST********************/
        public ActionResult tickets(int? page, string typeId)
        {
            TicketCounts();
            ViewBag.linkName = "tickettype";
            ViewBag.type = typeId;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getListBasedOnType(pageIndex, pageSize, typeId);
            return View("Dashboard", feedbackList);
        }

       
        /*****************VIEW OPEN  TICKET********************/

        public ActionResult openview(string  id)
        {
            var param2 = new SqlParameter();
            param2.ParameterName = "@TotalCount";
            param2.SqlDbType = SqlDbType.Int;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            ViewBag.fff = param2.Value;
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
                return View("view",f);
            }
        }

        /*****************VIEW RESOLVED  TICKET********************/
        public ActionResult resolvedview(string id)
        {           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
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

        /*****************VIEW ASSIGNED  TICKET********************/
        public ActionResult assignedview(string id)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);

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



        /*****************VIEW   TICKET********************/
        public ActionResult viewticket(string id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View("assignedview",f);
            }
        }



        /*****************VIEW RESPONDED  TICKET********************/
        public ActionResult respondedview(string id)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
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

        /*****************VIEW CLOSED  TICKET********************/
        public ActionResult closedview(string id)
        {           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
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


        /*****************VIEW REJECTED  TICKET********************/
        public ActionResult rejectedview(string id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
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

        /*************DOWNLOAD ATTACHMENT*********/
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
        public void TicketCounts()
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
        }

        /*****Get Attribute List***************/
        public void getAttributeList()
        {
            ViewBag.Departmn = feedInterface.getDepartmentsOnType(Constants.FORWARD);
            ViewBag.Priorities = feedInterface.getPriorties();
            ViewBag.Emails = feedInterface.getEmails();
            ViewBag.typeList = feedInterface.getFeedbackTypes();
            
                 ViewBag.sourceList = feedInterface.getSourceList();

        }
        [HttpPost]
        public JsonResult getEmpDetails(string id)
        {
            ApplicationUser u = feedInterface.getEmpDetails(id);
            System.Diagnostics.Debug.WriteLine(u.Nationality.name+"lllllllllllllllllllllllll"+u.NationalityId +"djdsj"+u.saudiNationalId);
            return Json(feedInterface.getEmpDetails(id));
        }

        

        public ViewResult charts()
        {
            string chartsAll = (db.Feedbacks.Count()).ToString();
            string chartsOpen = (db.Feedbacks.Where(f => f.status == "Open").Count()).ToString();
            string chartsClosed = (db.Feedbacks.Where(f => f.status == "Closed").Count()).ToString();
            string chartsResolved = (db.Feedbacks.Where(f => f.status == "Resolved").Count()).ToString();
            ViewBag.All = chartsAll;
            ViewBag.Open = chartsOpen;
            ViewBag.Closed = chartsClosed;
            ViewBag.Resolved = chartsResolved;
            return View("DataCharts");
        }

        [HttpGet]
        [Route("hr/chartssearch/")]
        public ActionResult chartssearch(string date22, string date1)
        {
            string d3 = date22;
            string dd = date1;
            if (d3.Equals("") || dd.Equals(""))
            {
                TempData["DateMsg"] = "Please Select StartDate And EndDate";
                return RedirectToAction("DataCharts");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                IEnumerable<Feedback> feedbacksopen = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Open");
                IEnumerable<Feedback> feedbacksclosed = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Closed");
                IEnumerable<Feedback> feedbacksresolved = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Resolved");        
                ViewBag.Open = feedbacksopen.Count();
                ViewBag.Closed = feedbacksclosed.Count();
                ViewBag.Resolved = feedbacksresolved.Count();
                ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataCharts");

            }
        }


        /*ffedbacktyp*/
        public ViewResult chartsfeedback_type()
        {





            //02 / 2019


            return View("DataChartsFeedback_Type");

        }

        [HttpGet]

        [Route("hr/chartsfeedback_typesearch/")]

        public ActionResult chartsfeedback_typesearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedback_type");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation







                if (!string.IsNullOrEmpty(mnt1))
                {
                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt1feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt1feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt1feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "3");
                    ViewBag.mnt1feedbacksinquiries = mnt1feedbacksinquiries.Count();
                    ViewBag.mnt1feedbackscompliants = mnt1feedbackscompliants.Count();
                    ViewBag.mnt1feedbacksappreciations = mnt1feedbacksappreciations.Count();
                    ViewBag.mnt1feedbackssuggestions = mnt1feedbackssuggestions.Count();
                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt2feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt2feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt2feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "3");
                    ViewBag.mnt2feedbacksinquiries = mnt2feedbacksinquiries.Count();
                    ViewBag.mnt2feedbackscompliants = mnt2feedbackscompliants.Count();
                    ViewBag.mnt2feedbacksappreciations = mnt2feedbacksappreciations.Count();
                    ViewBag.mnt2feedbackssuggestions = mnt2feedbackssuggestions.Count();
                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt3feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt3feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt3feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "3");
                    ViewBag.mnt3feedbacksinquiries = mnt3feedbacksinquiries.Count();
                    ViewBag.mnt3feedbackscompliants = mnt3feedbackscompliants.Count();
                    ViewBag.mnt3feedbacksappreciations = mnt3feedbacksappreciations.Count();
                    ViewBag.mnt3feedbackssuggestions = mnt3feedbackssuggestions.Count();
                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedback_Type");

            }
        }
        /*fedbkktp*/

        public ViewResult chartsfeedbacktype()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt1.Year.ToString());


            IEnumerable<Feedback> mnt1feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "2");
            IEnumerable<Feedback> mnt1feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
            IEnumerable<Feedback> mnt1feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "4");
            IEnumerable<Feedback> mnt1feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "3");

            IEnumerable<Feedback> mnt2feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "2");
            IEnumerable<Feedback> mnt2feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
            IEnumerable<Feedback> mnt2feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "4");
            IEnumerable<Feedback> mnt2feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "3");

            IEnumerable<Feedback> mnt3feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "2");
            IEnumerable<Feedback> mnt3feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
            IEnumerable<Feedback> mnt3feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "4");
            IEnumerable<Feedback> mnt3feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "3");

            ViewBag.mnt1feedbacksinquiries = mnt1feedbacksinquiries.Count();
            ViewBag.mnt1feedbackscompliants = mnt1feedbackscompliants.Count();
            ViewBag.mnt1feedbacksappreciations = mnt1feedbacksappreciations.Count();
            ViewBag.mnt1feedbackssuggestions = mnt1feedbackssuggestions.Count();

            ViewBag.mnt2feedbacksinquiries = mnt2feedbacksinquiries.Count();
            ViewBag.mnt2feedbackscompliants = mnt2feedbackscompliants.Count();
            ViewBag.mnt2feedbacksappreciations = mnt2feedbacksappreciations.Count();
            ViewBag.mnt2feedbackssuggestions = mnt2feedbackssuggestions.Count();

            ViewBag.mnt3feedbacksinquiries = mnt3feedbacksinquiries.Count();
            ViewBag.mnt3feedbackscompliants = mnt3feedbackscompliants.Count();
            ViewBag.mnt3feedbacksappreciations = mnt3feedbacksappreciations.Count();
            ViewBag.mnt3feedbackssuggestions = mnt3feedbackssuggestions.Count();

            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackType");

        }

        

      

        [HttpGet]

        [Route("hr/chartsfeedbacktypesearch/")]

        public ActionResult chartsfeedbacktypesearch(ChartMonths c)
        {

            
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;
             

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacktype");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation







                if (!string.IsNullOrEmpty(mnt1))
                {
                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt1feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt1feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt1feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "3");
                    ViewBag.mnt1feedbacksinquiries = mnt1feedbacksinquiries.Count();
                    ViewBag.mnt1feedbackscompliants = mnt1feedbackscompliants.Count();
                    ViewBag.mnt1feedbacksappreciations = mnt1feedbacksappreciations.Count();
                    ViewBag.mnt1feedbackssuggestions = mnt1feedbackssuggestions.Count();
                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt2feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt2feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt2feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "3");
                    ViewBag.mnt2feedbacksinquiries = mnt2feedbacksinquiries.Count();
                    ViewBag.mnt2feedbackscompliants = mnt2feedbackscompliants.Count();
                    ViewBag.mnt2feedbacksappreciations = mnt2feedbacksappreciations.Count();
                    ViewBag.mnt2feedbackssuggestions = mnt2feedbackssuggestions.Count();
                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "2");
                    IEnumerable<Feedback> mnt3feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt3feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "4");
                    IEnumerable<Feedback> mnt3feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "3");
                    ViewBag.mnt3feedbacksinquiries = mnt3feedbacksinquiries.Count();
                    ViewBag.mnt3feedbackscompliants = mnt3feedbackscompliants.Count();
                    ViewBag.mnt3feedbacksappreciations = mnt3feedbacksappreciations.Count();
                    ViewBag.mnt3feedbackssuggestions = mnt3feedbackssuggestions.Count();
                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackType");

            }
        }



        public ViewResult chartsfeedbacksource()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            IEnumerable<Feedback> mnt1feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Walkin");
            IEnumerable<Feedback> mnt1feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "whatsapp");
            IEnumerable<Feedback> mnt1feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Mobile");
            IEnumerable<Feedback> mnt1feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Toll Free");
            IEnumerable<Feedback> mnt1feedbacksemail = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "E-Mail");


            IEnumerable<Feedback> mnt2feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Walkin");
            IEnumerable<Feedback> mnt2feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "whatsapp");
            IEnumerable<Feedback> mnt2feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Mobile");
            IEnumerable<Feedback> mnt2feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Toll Free");
            IEnumerable<Feedback> mnt2feedbacksemail = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "E-Mail");

            IEnumerable<Feedback> mnt3feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Walkin");
            IEnumerable<Feedback> mnt3feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "whatsapp");
            IEnumerable<Feedback> mnt3feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Mobile");
            IEnumerable<Feedback> mnt3feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Toll Free");
            IEnumerable<Feedback> mnt3feedbacksemail = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "E-Mail");

            ViewBag.mnt1feedbackswalkin = mnt1feedbackswalkin.Count();
            ViewBag.mnt1feedbackswhatsapp = mnt1feedbackswhatsapp.Count();
            ViewBag.mnt1feedbacksmobile = mnt1feedbacksmobile.Count();
            ViewBag.mnt1feedbackstollfree = mnt1feedbackstollfree.Count();
            ViewBag.mnt1feedbacksemail = mnt1feedbacksemail.Count();

            ViewBag.mnt2feedbackswalkin = mnt2feedbackswalkin.Count();
            ViewBag.mnt2feedbackswhatsapp = mnt2feedbackswhatsapp.Count();
            ViewBag.mnt2feedbacksmobile = mnt2feedbacksmobile.Count();
            ViewBag.mnt2feedbackstollfree = mnt2feedbackstollfree.Count();
            ViewBag.mnt2feedbacksemail = mnt2feedbacksemail.Count();

            ViewBag.mnt3feedbackswalkin = mnt3feedbackswalkin.Count();
            ViewBag.mnt3feedbackswhatsapp = mnt3feedbackswhatsapp.Count();
            ViewBag.mnt3feedbacksmobile = mnt3feedbacksmobile.Count();
            ViewBag.mnt3feedbackstollfree = mnt2feedbackstollfree.Count();
            ViewBag.mnt3feedbacksemail = mnt3feedbacksemail.Count();

            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackSource");

        }

        [HttpGet]

        [Route("hr/chartsfeedbacksourcesearch/")]

        public ActionResult chartsfeedbacksourcesearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksource");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation




                if (!string.IsNullOrEmpty(mnt1))
                {
                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Walkin");
                    IEnumerable<Feedback> mnt1feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "whatsapp");
                    IEnumerable<Feedback> mnt1feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Mobile");
                    IEnumerable<Feedback> mnt1feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Toll Free");
                    IEnumerable<Feedback> mnt1feedbacksemail = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "E-Mail");
                    ViewBag.mnt1feedbackswalkin = mnt1feedbackswalkin.Count();
                    ViewBag.mnt1feedbackswhatsapp = mnt1feedbackswhatsapp.Count();
                    ViewBag.mnt1feedbacksmobile = mnt1feedbacksmobile.Count();
                    ViewBag.mnt1feedbackstollfree = mnt1feedbackstollfree.Count();
                    ViewBag.mnt1feedbacksemail = mnt1feedbacksemail.Count();
                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Walkin");
                    IEnumerable<Feedback> mnt2feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "whatsapp");
                    IEnumerable<Feedback> mnt2feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Mobile");
                    IEnumerable<Feedback> mnt2feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Toll Free");
                    IEnumerable<Feedback> mnt2feedbacksemail = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "E-Mail");
                    ViewBag.mnt2feedbackswalkin = mnt2feedbackswalkin.Count();
                    ViewBag.mnt2feedbackswhatsapp = mnt2feedbackswhatsapp.Count();
                    ViewBag.mnt2feedbacksmobile = mnt2feedbacksmobile.Count();
                    ViewBag.mnt2feedbackstollfree = mnt2feedbackstollfree.Count();
                    ViewBag.mnt2feedbacksemail = mnt2feedbacksemail.Count();
                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Walkin");
                    IEnumerable<Feedback> mnt3feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "whatsapp");
                    IEnumerable<Feedback> mnt3feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Mobile");
                    IEnumerable<Feedback> mnt3feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Toll Free");
                    IEnumerable<Feedback> mnt3feedbacksemail = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "E-Mail");
                    ViewBag.mnt3feedbackswalkin = mnt3feedbackswalkin.Count();
                    ViewBag.mnt3feedbackswhatsapp = mnt3feedbackswhatsapp.Count();
                    ViewBag.mnt3feedbacksmobile = mnt3feedbacksmobile.Count();
                    ViewBag.mnt3feedbackstollfree = mnt3feedbackstollfree.Count();
                    ViewBag.mnt3feedbacksemail = mnt3feedbacksemail.Count();
                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackSource");

            }
        }


        public ViewResult chartsfeedbackdepartment()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
              mnt1feedbacksfinance 
    mnt1feedbackstalentmanagement 
    mnt1feedbacksadministrations 
    mnt1feedbacksoperations 
    mnt1feedbackssahlfeedback 
     mnt1feedbackssahlmds 
     mnt1feedbackssahltraining 
             
             
             */



            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
            IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
            IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
            IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
            IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
            IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");


            IEnumerable<Feedback> mnt2feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Finance");
            IEnumerable<Feedback> mnt2feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Talent Management & Corporate Compliance");
            IEnumerable<Feedback> mnt2feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Administration");
            IEnumerable<Feedback> mnt2feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Operations");
            IEnumerable<Feedback> mnt2feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Sahl");
            IEnumerable<Feedback> mnt2feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt2feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Training, Learning & Performance");

            IEnumerable<Feedback> mnt3feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Finance");
            IEnumerable<Feedback> mnt3feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Talent Management & Corporate Compliance");
            IEnumerable<Feedback> mnt3feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Administration");
            IEnumerable<Feedback> mnt3feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Operations");
            IEnumerable<Feedback> mnt3feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Sahl");
            IEnumerable<Feedback> mnt3feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt3feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Training, Learning & Performance");

            ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();

            ViewBag.mnt2feedbacksfinance = mnt2feedbacksfinance.Count();
            ViewBag.mnt2feedbackstalentmanagement = mnt2feedbackstalentmanagement.Count();
            ViewBag.mnt2feedbacksadministrations = mnt2feedbacksadministrations.Count();
            ViewBag.mnt2feedbacksoperations = mnt2feedbacksoperations.Count();
            ViewBag.mnt2feedbackssahlfeedback = mnt2feedbackssahlfeedback.Count();
            ViewBag.mnt2feedbackssahlmds = mnt2feedbackssahlmds.Count();
            ViewBag.mnt2feedbackssahltraining = mnt2feedbackssahltraining.Count();

            ViewBag.mnt3feedbacksfinance = mnt3feedbacksfinance.Count();
            ViewBag.mnt3feedbackstalentmanagement = mnt3feedbackstalentmanagement.Count();
            ViewBag.mnt3feedbacksadministrations = mnt3feedbacksadministrations.Count();
            ViewBag.mnt3feedbacksoperations = mnt3feedbacksoperations.Count();
            ViewBag.mnt3feedbackssahlfeedback = mnt3feedbackssahlfeedback.Count();
            ViewBag.mnt3feedbackssahlmds = mnt3feedbackssahlmds.Count();
            ViewBag.mnt3feedbackssahltraining = mnt3feedbackssahltraining.Count();

            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackDepartment");

        }

        [HttpGet]

        [Route("hr/chartsfeedbackdepartmentsearch/")]

        public ActionResult chartsfeedbackdepartmentsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackdepartment");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");

                     
                     */




                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
                    IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
                    IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
                    IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
                    IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
                    IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
                    ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
                    ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
                    ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
                    ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
                    ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
                    ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
                    ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Finance");
                    IEnumerable<Feedback> mnt2feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Talent Management & Corporate Compliance");
                    IEnumerable<Feedback> mnt2feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Administration");
                    IEnumerable<Feedback> mnt2feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Operations");
                    IEnumerable<Feedback> mnt2feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Sahl");
                    IEnumerable<Feedback> mnt2feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt2feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Training, Learning & Performance");
                    ViewBag.mnt2feedbacksfinance = mnt2feedbacksfinance.Count();
                    ViewBag.mnt2feedbackstalentmanagement = mnt2feedbackstalentmanagement.Count();
                    ViewBag.mnt2feedbacksadministrations = mnt2feedbacksadministrations.Count();
                    ViewBag.mnt2feedbacksoperations = mnt2feedbacksoperations.Count();
                    ViewBag.mnt2feedbackssahlfeedback = mnt2feedbackssahlfeedback.Count();
                    ViewBag.mnt2feedbackssahlmds = mnt2feedbackssahlmds.Count();
                    ViewBag.mnt2feedbackssahltraining = mnt2feedbackssahltraining.Count();
                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Finance");
                    IEnumerable<Feedback> mnt3feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Talent Management & Corporate Compliance");
                    IEnumerable<Feedback> mnt3feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Administration");
                    IEnumerable<Feedback> mnt3feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Operations");
                    IEnumerable<Feedback> mnt3feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Sahl");
                    IEnumerable<Feedback> mnt3feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt3feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Training, Learning & Performance");
                    ViewBag.mnt3feedbacksfinance = mnt3feedbacksfinance.Count();
                    ViewBag.mnt3feedbackstalentmanagement = mnt3feedbackstalentmanagement.Count();
                    ViewBag.mnt3feedbacksadministrations = mnt3feedbacksadministrations.Count();
                    ViewBag.mnt3feedbacksoperations = mnt3feedbacksoperations.Count();
                    ViewBag.mnt3feedbackssahlfeedback = mnt3feedbackssahlfeedback.Count();
                    ViewBag.mnt3feedbackssahlmds = mnt3feedbackssahlmds.Count();
                    ViewBag.mnt3feedbackssahltraining = mnt3feedbackssahltraining.Count();
                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackDepartment");

            }
        }



        public ViewResult chartsfeedbackposition()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"

            IEnumerable<Feedback> mnt1feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Crews");
            IEnumerable<Feedback> mnt1feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Drivers");
            IEnumerable<Feedback> mnt1feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Managers");
            IEnumerable<Feedback> mnt1feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt1feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Stars");
            IEnumerable<Feedback> mnt1feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Maintenance");
            IEnumerable<Feedback> mnt1feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "GEL");
            IEnumerable<Feedback> mnt1feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Specialist");
            IEnumerable<Feedback> mnt1feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Consultants");


            IEnumerable<Feedback> mnt2feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Crews");
            IEnumerable<Feedback> mnt2feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Drivers");
            IEnumerable<Feedback> mnt2feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Managers");
            IEnumerable<Feedback> mnt2feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt2feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Stars");
            IEnumerable<Feedback> mnt2feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Maintenance");
            IEnumerable<Feedback> mnt2feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "GEL");
            IEnumerable<Feedback> mnt2feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Specialist");
            IEnumerable<Feedback> mnt2feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Consultants");

            IEnumerable<Feedback> mnt3feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Crews");
            IEnumerable<Feedback> mnt3feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Drivers");
            IEnumerable<Feedback> mnt3feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Managers");
            IEnumerable<Feedback> mnt3feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
            IEnumerable<Feedback> mnt3feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Stars");
            IEnumerable<Feedback> mnt3feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Maintenance");
            IEnumerable<Feedback> mnt3feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "GEL");
            IEnumerable<Feedback> mnt3feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Specialist");
            IEnumerable<Feedback> mnt3feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Consultants");

            /*
               var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            ViewBag.mnt1feedbackscrews = mnt1feedbackscrews.Count();
            ViewBag.mnt1feedbacksdrivers = mnt1feedbacksdrivers.Count();
            ViewBag.mnt1feedbacksmanagers = mnt1feedbacksmanagers.Count();
            ViewBag.mnt1feedbacksmds = mnt1feedbacksmds.Count();
            ViewBag.mnt1feedbacksstars = mnt1feedbacksstars.Count();
            ViewBag.mnt1feedbacksmaintenance = mnt1feedbacksmaintenance.Count();
            ViewBag.mnt1feedbacksgel = mnt1feedbacksgel.Count();
            ViewBag.mnt1feedbacksspecialist = mnt1feedbacksspecialist.Count();
            ViewBag.mnt1feedbacksconsultants = mnt1feedbacksconsultants.Count();

            ViewBag.mnt2feedbackscrews = mnt2feedbackscrews.Count();
            ViewBag.mnt2feedbacksdrivers = mnt2feedbacksdrivers.Count();
            ViewBag.mnt2feedbacksmanagers = mnt2feedbacksmanagers.Count();
            ViewBag.mnt2feedbacksmds = mnt2feedbacksmds.Count();
            ViewBag.mnt2feedbacksstars = mnt2feedbacksstars.Count();
            ViewBag.mnt2feedbacksmaintenance = mnt2feedbacksmaintenance.Count();
            ViewBag.mnt2feedbacksgel = mnt2feedbacksgel.Count();
            ViewBag.mnt2feedbacksspecialist = mnt2feedbacksspecialist.Count();
            ViewBag.mnt2feedbacksconsultants = mnt2feedbacksconsultants.Count();

            ViewBag.mnt3feedbackscrews = mnt3feedbackscrews.Count();
            ViewBag.mnt3feedbacksdrivers = mnt3feedbacksdrivers.Count();
            ViewBag.mnt3feedbacksmanagers = mnt3feedbacksmanagers.Count();
            ViewBag.mnt3feedbacksmds = mnt3feedbacksmds.Count();
            ViewBag.mnt3feedbacksstars = mnt3feedbacksstars.Count();
            ViewBag.mnt3feedbacksmaintenance = mnt3feedbacksmaintenance.Count();
            ViewBag.mnt3feedbacksgel = mnt3feedbacksgel.Count();
            ViewBag.mnt3feedbacksspecialist = mnt3feedbacksspecialist.Count();
            ViewBag.mnt3feedbacksconsultants = mnt3feedbacksconsultants.Count();

            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackPosition");

        }

        [HttpGet]

        [Route("hr/chartsfeedbackpositionsearch/")]

        public ActionResult chartsfeedbackpositionsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackposition");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");

                     
                     */




                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Crews");
                    IEnumerable<Feedback> mnt1feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Drivers");
                    IEnumerable<Feedback> mnt1feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Managers");
                    IEnumerable<Feedback> mnt1feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt1feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Stars");
                    IEnumerable<Feedback> mnt1feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Maintenance");
                    IEnumerable<Feedback> mnt1feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "GEL");
                    IEnumerable<Feedback> mnt1feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Specialist");
                    IEnumerable<Feedback> mnt1feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Consultants");
                    ViewBag.mnt1feedbackscrews = mnt1feedbackscrews.Count();
                    ViewBag.mnt1feedbacksdrivers = mnt1feedbacksdrivers.Count();
                    ViewBag.mnt1feedbacksmanagers = mnt1feedbacksmanagers.Count();
                    ViewBag.mnt1feedbacksmds = mnt1feedbacksmds.Count();
                    ViewBag.mnt1feedbacksstars = mnt1feedbacksstars.Count();
                    ViewBag.mnt1feedbacksmaintenance = mnt1feedbacksmaintenance.Count();
                    ViewBag.mnt1feedbacksgel = mnt1feedbacksgel.Count();
                    ViewBag.mnt1feedbacksspecialist = mnt1feedbacksspecialist.Count();
                    ViewBag.mnt1feedbacksconsultants = mnt1feedbacksconsultants.Count();
                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Crews");
                    IEnumerable<Feedback> mnt2feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Drivers");
                    IEnumerable<Feedback> mnt2feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Managers");
                    IEnumerable<Feedback> mnt2feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt2feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Stars");
                    IEnumerable<Feedback> mnt2feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Maintenance");
                    IEnumerable<Feedback> mnt2feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "GEL");
                    IEnumerable<Feedback> mnt2feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Specialist");
                    IEnumerable<Feedback> mnt2feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Consultants");
                    ViewBag.mnt2feedbackscrews = mnt2feedbackscrews.Count();
                    ViewBag.mnt2feedbacksdrivers = mnt2feedbacksdrivers.Count();
                    ViewBag.mnt2feedbacksmanagers = mnt2feedbacksmanagers.Count();
                    ViewBag.mnt2feedbacksmds = mnt2feedbacksmds.Count();
                    ViewBag.mnt2feedbacksstars = mnt2feedbacksstars.Count();
                    ViewBag.mnt2feedbacksmaintenance = mnt2feedbacksmaintenance.Count();
                    ViewBag.mnt2feedbacksgel = mnt2feedbacksgel.Count();
                    ViewBag.mnt2feedbacksspecialist = mnt2feedbacksspecialist.Count();
                    ViewBag.mnt2feedbacksconsultants = mnt2feedbacksconsultants.Count();
                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Crews");
                    IEnumerable<Feedback> mnt3feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Drivers");
                    IEnumerable<Feedback> mnt3feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Managers");
                    IEnumerable<Feedback> mnt3feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
                    IEnumerable<Feedback> mnt3feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Stars");
                    IEnumerable<Feedback> mnt3feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Maintenance");
                    IEnumerable<Feedback> mnt3feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "GEL");
                    IEnumerable<Feedback> mnt3feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Specialist");
                    IEnumerable<Feedback> mnt3feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Consultants");
                    ViewBag.mnt3feedbackscrews = mnt3feedbackscrews.Count();
                    ViewBag.mnt3feedbacksdrivers = mnt3feedbacksdrivers.Count();
                    ViewBag.mnt3feedbacksmanagers = mnt3feedbacksmanagers.Count();
                    ViewBag.mnt3feedbacksmds = mnt3feedbacksmds.Count();
                    ViewBag.mnt3feedbacksstars = mnt3feedbacksstars.Count();
                    ViewBag.mnt3feedbacksmaintenance = mnt3feedbacksmaintenance.Count();
                    ViewBag.mnt3feedbacksgel = mnt3feedbacksgel.Count();
                    ViewBag.mnt3feedbacksspecialist = mnt3feedbacksspecialist.Count();
                    ViewBag.mnt3feedbacksconsultants = mnt3feedbacksconsultants.Count();
                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackPosition");

            }
        }


        public ViewResult chartsfeedbacknationality()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
     var mnt1feedbacksaudi = $("#mnt1feedbacksaudi").val();

   var mnt1feedbacksexpatriates = $("#mnt1feedbacksexpatriates").val();
   */

            IEnumerable<Feedback> mnt1feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt1feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt1.Month.ToString(), dmnt1.Year.ToString());



            IEnumerable<Feedback> mnt2feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt2.Month.ToString(), dmnt2.Year.ToString());


            IEnumerable<Feedback> mnt3feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt3.Month.ToString(), dmnt3.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
               var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            ViewBag.mnt1feedbacksaudi = mnt1feedbacksaudi.Count();
            ViewBag.mnt1feedbacksexpatriates = mnt1feedbacksexpatriates.Count();


            ViewBag.mnt2feedbacksaudi = mnt2feedbacksaudi.Count();
            ViewBag.mnt2feedbacksexpatriates = mnt2feedbacksexpatriates.Count();


            ViewBag.mnt3feedbacksaudi = mnt3feedbacksaudi.Count();
            ViewBag.mnt3feedbacksexpatriates = mnt3feedbacksexpatriates.Count();


            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackNationality");

        }

        [HttpGet]

        [Route("hr/chartsfeedbacknationalitysearch/")]

        public ActionResult chartsfeedbacknationalitysearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacknationality");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");

                     
                     */

                    /*
     var mnt1feedbacksaudi = $("#mnt1feedbacksaudi").val();

   var mnt1feedbacksexpatriates = $("#mnt1feedbacksexpatriates").val();
   */



                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt1.Month.ToString(), dmnt1.Year.ToString());

                    ViewBag.mnt1feedbacksaudi = mnt1feedbacksaudi.Count();
                    ViewBag.mnt1feedbacksexpatriates = mnt1feedbacksexpatriates.Count();

                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt2.Month.ToString(), dmnt2.Year.ToString());

                    ViewBag.mnt2feedbacksaudi = mnt2feedbacksaudi.Count();
                    ViewBag.mnt2feedbacksexpatriates = mnt2feedbacksexpatriates.Count();

                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt3.Month.ToString(), dmnt3.Year.ToString());

                    ViewBag.mnt3feedbacksaudi = mnt3feedbacksaudi.Count();
                    ViewBag.mnt3feedbacksexpatriates = mnt3feedbacksexpatriates.Count();

                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackNationality");

            }
        }

        /*  public static string SATISFIED = "Satisfied";
        public static string UN_SATISFIED = "UnSatisfied";*/


        public ViewResult chartsfeedbacksatisfaction()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
     var mnt1feedbacksaudi = $("#mnt1feedbacksaudi").val();

   var mnt1feedbacksexpatriates = $("#mnt1feedbacksexpatriates").val();
   */

            IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);


            /*
               var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();


            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackSatisfaction");

        }

        [HttpGet]

        [Route("hr/chartsfeedbacksatisfactionsearch/")]

        public ActionResult chartsfeedbacksatisfactionsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksatisfaction");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
  IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(),Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);
                 ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();     
                     */

                    /*
     var mnt1feedbacksaudi = $("#mnt1feedbacksaudi").val();

   var mnt1feedbacksexpatriates = $("#mnt1feedbacksexpatriates").val();
   */



                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
                    IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.UN_SATISFIED);

                    ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
                    ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();

                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
                    IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);

                    ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
                    ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();

                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
                    IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);

                    ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
                    ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();

                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackSatisfaction");

            }
        }


        public ViewResult chartsfeedbackescalation()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
     var mnt1feedbacksaudi = $("#mnt1feedbacksaudi").val();

   var mnt1feedbacksexpatriates = $("#mnt1feedbacksexpatriates").val();
   */

            IEnumerable<Feedback> mnt1feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
            IEnumerable<Feedback> mnt1feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");



            IEnumerable<Feedback> mnt2feedbackescalated = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
            IEnumerable<Feedback> mnt2feedbacksnotescalated = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");


            IEnumerable<Feedback> mnt3feedbackescalated = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
            IEnumerable<Feedback> mnt3feedbacksnotescalated = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");


            /*
               var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
            ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();


            ViewBag.mnt2feedbackescalated = mnt2feedbackescalated.Count();
            ViewBag.mnt2feedbacksnotescalated = mnt2feedbacksnotescalated.Count();


            ViewBag.mnt3feedbackescalated = mnt3feedbackescalated.Count();
            ViewBag.mnt3feedbacksnotescalated = mnt3feedbacksnotescalated.Count();


            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackEscalation");

        }

        [HttpGet]

        [Route("hr/chartsfeedbackescalationsearch/")]

        public ActionResult chartsfeedbackescalationsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackescalation");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
  IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(),Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);
                 ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();     
                     */

                    /*
     ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
            ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();
   */



                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt1feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");

                    ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
                    ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();

                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt2feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");

                    ViewBag.mnt2feedbackescalated = mnt2feedbackescalated.Count();
                    ViewBag.mnt2feedbacksnotescalated = mnt2feedbacksnotescalated.Count();

                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt3feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");

                    ViewBag.mnt3feedbackescalated = mnt3feedbackescalated.Count();
                    ViewBag.mnt3feedbacksnotescalated = mnt3feedbacksnotescalated.Count();

                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackEscalation");

            }
        }


        public ViewResult chartsfeedbacklast12months()
        {





            //02 / 2019
            DateTime[] dmnt1 = new DateTime[12];
            dmnt1[0] = new DateTime();
            dmnt1[0] = DateTime.Now;
            for (int i = 1; i < 12; i++)
            {
                dmnt1[i] = dmnt1[i - 1].AddMonths(-1);

            }




            IEnumerable<Feedback>[] mnt1feedbacksall = new IEnumerable<Feedback>[12];
            for (int i = 0; i < 12; i++)
            {
                mnt1feedbacksall[i] = feedInterface.chartsFeedbackAll(dmnt1[i].Month.ToString(), dmnt1[i].Year.ToString());
            }









            ViewBag.mnt1feedbacksall = mnt1feedbacksall[0].Count();
            ViewBag.mnt2feedbacksall = mnt1feedbacksall[1].Count();
            ViewBag.mnt3feedbacksall = mnt1feedbacksall[2].Count();
            ViewBag.mnt4feedbacksall = mnt1feedbacksall[3].Count();
            ViewBag.mnt5feedbacksall = mnt1feedbacksall[4].Count();
            ViewBag.mnt6feedbacksall = mnt1feedbacksall[5].Count();
            ViewBag.mnt7feedbacksall = mnt1feedbacksall[6].Count();
            ViewBag.mnt8feedbacksall = mnt1feedbacksall[7].Count();
            ViewBag.mnt9feedbacksall = mnt1feedbacksall[8].Count();
            ViewBag.mnt10feedbacksall = mnt1feedbacksall[9].Count();
            ViewBag.mnt11feedbacksall = mnt1feedbacksall[10].Count();
            ViewBag.mnt12feedbacksall = mnt1feedbacksall[11].Count();



            ViewBag.month1 = dmnt1[0].ToString("MMM") + "-" + dmnt1[0].Year;
            ViewBag.month2 = dmnt1[1].ToString("MMM") + "-" + dmnt1[1].Year;
            ViewBag.month3 = dmnt1[2].ToString("MMM") + "-" + dmnt1[2].Year;
            ViewBag.month4 = dmnt1[3].ToString("MMM") + "-" + dmnt1[3].Year;
            ViewBag.month5 = dmnt1[4].ToString("MMM") + "-" + dmnt1[4].Year;
            ViewBag.month6 = dmnt1[5].ToString("MMM") + "-" + dmnt1[5].Year;
            ViewBag.month7 = dmnt1[6].ToString("MMM") + "-" + dmnt1[6].Year;
            ViewBag.month8 = dmnt1[7].ToString("MMM") + "-" + dmnt1[7].Year;
            ViewBag.month9 = dmnt1[8].ToString("MMM") + "-" + dmnt1[8].Year;
            ViewBag.month10 = dmnt1[9].ToString("MMM") + "-" + dmnt1[9].Year;
            ViewBag.month11 = dmnt1[10].ToString("MMM") + "-" + dmnt1[10].Year;
            ViewBag.month12 = dmnt1[11].ToString("MMM") + "-" + dmnt1[11].Year;






            return View("DataChartsFeedbackLast12Months");

        }


        public ViewResult chartsfeedbackmostfrequent()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
      var mnt1feedbacksalarytimeissue = $("#mnt1feedbacksalarytimeissue").val();

    var mnt1feedbackhousingservices = $("#mnt1feedbackhousingservices").val();
    var mnt1feedbackpoortreatment = $("#mnt1feedbackpoortreatment").val();

    var mnt1feedbackaccomodation = $("#mnt1feedbackaccomodation").val();
   */

            IEnumerable<Feedback> mnt1feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt1feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt1feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt1feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt1.Month.ToString(), dmnt1.Year.ToString());



            IEnumerable<Feedback> mnt2feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt2feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt2feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt2feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt2.Month.ToString(), dmnt2.Year.ToString());


            IEnumerable<Feedback> mnt3feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt3.Month.ToString(), dmnt3.Year.ToString());
            IEnumerable<Feedback> mnt3feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt3.Month.ToString(), dmnt3.Year.ToString());
            IEnumerable<Feedback> mnt3feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt3.Month.ToString(), dmnt3.Year.ToString());
            IEnumerable<Feedback> mnt3feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt3.Month.ToString(), dmnt3.Year.ToString());




            ViewBag.mnt1feedbacksalarytimeissue = mnt1feedbacksalarytimeissue.Count();
            ViewBag.mnt1feedbackhousingservices = mnt1feedbackhousingservices.Count();
            ViewBag.mnt1feedbackpoortreatment = mnt1feedbackpoortreatment.Count();
            ViewBag.mnt1feedbackaccomodation = mnt1feedbackaccomodation.Count();


            ViewBag.mnt2feedbacksalarytimeissue = mnt2feedbacksalarytimeissue.Count();
            ViewBag.mnt2feedbackhousingservices = mnt2feedbackhousingservices.Count();
            ViewBag.mnt2feedbackpoortreatment = mnt2feedbackpoortreatment.Count();
            ViewBag.mnt2feedbackaccomodation = mnt2feedbackaccomodation.Count();


            ViewBag.mnt3feedbacksalarytimeissue = mnt3feedbacksalarytimeissue.Count();
            ViewBag.mnt3feedbackhousingservices = mnt3feedbackhousingservices.Count();
            ViewBag.mnt3feedbackpoortreatment = mnt3feedbackpoortreatment.Count();
            ViewBag.mnt3feedbackaccomodation = mnt3feedbackaccomodation.Count();


            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackMostFrequent");

        }

        [HttpGet]

        [Route("hr/chartsfeedbackmostfrequentsearch/")]

        public ActionResult chartsfeedbackmostfrequentsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackmostfrequent");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
  IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(),Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);
                 ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();     
                     */

                    /*
     ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
            ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();
   */



                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt1.Month.ToString(), dmnt1.Year.ToString());

                    ViewBag.mnt1feedbacksalarytimeissue = mnt1feedbacksalarytimeissue.Count();
                    ViewBag.mnt1feedbackhousingservices = mnt1feedbackhousingservices.Count();
                    ViewBag.mnt1feedbackpoortreatment = mnt1feedbackpoortreatment.Count();
                    ViewBag.mnt1feedbackaccomodation = mnt1feedbackaccomodation.Count();

                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt2.Month.ToString(), dmnt2.Year.ToString());

                    ViewBag.mnt2feedbacksalarytimeissue = mnt2feedbacksalarytimeissue.Count();
                    ViewBag.mnt2feedbackhousingservices = mnt2feedbackhousingservices.Count();
                    ViewBag.mnt2feedbackpoortreatment = mnt2feedbackpoortreatment.Count();
                    ViewBag.mnt2feedbackaccomodation = mnt2feedbackaccomodation.Count();


                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt3.Month.ToString(), dmnt3.Year.ToString());

                    ViewBag.mnt3feedbacksalarytimeissue = mnt3feedbacksalarytimeissue.Count();
                    ViewBag.mnt3feedbackhousingservices = mnt3feedbackhousingservices.Count();
                    ViewBag.mnt3feedbackpoortreatment = mnt3feedbackpoortreatment.Count();
                    ViewBag.mnt3feedbackaccomodation = mnt3feedbackaccomodation.Count();

                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackMostFrequent");

            }
        }


        public ViewResult chartsfeedbacksalaryissuesreasons() // pending
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
            IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
            IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());


            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
       mnt1feedbacklegitimate 
   mnt1feedbackillegitimate 
   */

            IEnumerable<Feedback> mnt1feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
            IEnumerable<Feedback> mnt1feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");




            IEnumerable<Feedback> mnt2feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
            IEnumerable<Feedback> mnt2feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");


            IEnumerable<Feedback> mnt3feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
            IEnumerable<Feedback> mnt3feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");




            ViewBag.mnt1feedbacklegitimate = mnt1feedbacklegitimate.Count();
            ViewBag.mnt1feedbackillegitimate = mnt1feedbackillegitimate.Count();



            ViewBag.mnt2feedbacklegitimate = mnt2feedbacklegitimate.Count();
            ViewBag.mnt2feedbackillegitimate = mnt2feedbackillegitimate.Count();


            ViewBag.mnt3feedbacklegitimate = mnt3feedbacklegitimate.Count();
            ViewBag.mnt3feedbackillegitimate = mnt3feedbackillegitimate.Count();


            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackSalaryIssuesReasons");

        }

        [HttpGet]

        [Route("hr/chartsfeedbacksalaryissuesreasonssearch/")] // pending

        public ActionResult chartsfeedbacksalaryissuesreasons(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksalaryissuesreasons");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();
                DateTime dmnt2 = new DateTime();
                DateTime dmnt3 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(mnt2))
                {
                    dmnt2 = DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }


                if (!string.IsNullOrEmpty(mnt3))
                {
                    dmnt3 = DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
  IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(),Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);
                 ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();     
                     */

                    /*
     ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
            ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();
   */



                    IEnumerable<Feedback> mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    IEnumerable<Feedback> mnt1feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt1feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");

                    ViewBag.mnt1feedbacklegitimate = mnt1feedbacklegitimate.Count();
                    ViewBag.mnt1feedbackillegitimate = mnt1feedbackillegitimate.Count();

                    ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



                if (!string.IsNullOrEmpty(mnt2))
                {
                    IEnumerable<Feedback> mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                    IEnumerable<Feedback> mnt2feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt2feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");

                    ViewBag.mnt2feedbacklegitimate = mnt2feedbacklegitimate.Count();
                    ViewBag.mnt2feedbackillegitimate = mnt2feedbackillegitimate.Count();

                    ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                    ViewBag.month2 = dmnt2.ToString("MMM");
                }
                else
                {
                    ViewBag.month2 = "";
                }

                if (!string.IsNullOrEmpty(mnt3))
                {
                    IEnumerable<Feedback> mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                    IEnumerable<Feedback> mnt3feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                    IEnumerable<Feedback> mnt3feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");

                    ViewBag.mnt3feedbacklegitimate = mnt3feedbacklegitimate.Count();
                    ViewBag.mnt3feedbackillegitimate = mnt3feedbackillegitimate.Count();

                    ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                    ViewBag.month3 = dmnt3.ToString("MMM");

                }
                else
                {
                    ViewBag.month3 = "";

                }

















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackSalaryIssuesReasons");

            }
        }



        public ViewResult chartsfeedbackmostfrequentlocations()
        {





            //02 / 2019
            DateTime dmnt1 = DateTime.Now;//DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt2 = DateTime.Now.AddMonths(-1); //DateTime.ParseExact("01-" + mnt2.Substring(0, 2) + "-" + mnt2.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime dmnt3 = DateTime.Now.AddMonths(-2);//DateTime.ParseExact("01-" + mnt3.Substring(0, 2) + "-" + mnt3.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation


            string[] locationnamec = feedInterface.chartsFeedbackMostFrequentLocations(dmnt1.Month.ToString(), dmnt1.Year.ToString());



            /*
             mnt1feedbackswalkin
    mnt1feedbackswhatsapp
    mnt1feedbacksmobile
    mnt1feedbackstollfree
    mnt1feedbacksemail
             */

            /*
             
             var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
             
             
             */

            //"Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"


            /*
       mnt1feedbacklegitimate 
   mnt1feedbackillegitimate 
   */







            ViewBag.mnt1feedbackmostfrequentlocations = locationnamec[0];



            ViewBag.locationname = locationnamec[1];


            ViewBag.month1 = dmnt1.ToString("MMM");




            return View("DataChartsFeedbackMostFrequentLocations");

        }

        [HttpGet]

        [Route("hr/chartsfeedbackmostfrequentlocationssearch/")]

        public ActionResult chartsfeedbackmostfrequentlocations(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackmostfrequentlocations");
            }
            else
            {
                //02 / 2019
                DateTime dmnt1 = new DateTime();

                if (!string.IsNullOrEmpty(mnt1))
                {
                    dmnt1 = DateTime.ParseExact("01-" + mnt1.Substring(0, 2) + "-" + mnt1.Substring(3, 4), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }



                //1   Complaint
                //2   Enquiry
                //3   Suggestion
                //4   Appreciation

                /*
                  ViewBag.mnt1feedbacksfinance = mnt1feedbacksfinance.Count();
            ViewBag.mnt1feedbackstalentmanagement = mnt1feedbackstalentmanagement.Count();
            ViewBag.mnt1feedbacksadministrations = mnt1feedbacksadministrations.Count();
            ViewBag.mnt1feedbacksoperations = mnt1feedbacksoperations.Count();
            ViewBag.mnt1feedbackssahlfeedback = mnt1feedbackssahlfeedback.Count();
            ViewBag.mnt1feedbackssahlmds = mnt1feedbackssahlmds.Count();
            ViewBag.mnt1feedbackssahltraining = mnt1feedbackssahltraining.Count();
                 
                 */


                if (!string.IsNullOrEmpty(mnt1))
                {
                    /*
            IEnumerable<Feedback> mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
IEnumerable<Feedback> mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
IEnumerable<Feedback> mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
IEnumerable<Feedback> mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
IEnumerable<Feedback> mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
IEnumerable<Feedback> mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
IEnumerable<Feedback> mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
  IEnumerable<Feedback> mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(),Constants.UN_SATISFIED);



            IEnumerable<Feedback> mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);


            IEnumerable<Feedback> mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);
                 ViewBag.mnt1feedbacksatisfied = mnt1feedbacksatisfied.Count();
            ViewBag.mnt1feedbacksunsatisfied = mnt1feedbacksunsatisfied.Count();


            ViewBag.mnt2feedbacksatisfied = mnt2feedbacksatisfied.Count();
            ViewBag.mnt2feedbacksunsatisfied = mnt2feedbacksunsatisfied.Count();


            ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
            ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();     
                     */

                    /*
     ViewBag.mnt1feedbackescalated = mnt1feedbackescalated.Count();
            ViewBag.mnt1feedbacksnotescalated = mnt1feedbacksnotescalated.Count();
   */



                    string[] locationnamec = feedInterface.chartsFeedbackMostFrequentLocations(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                    ViewBag.mnt1feedbackmostfrequentlocations = locationnamec[0];



                    ViewBag.locationname = locationnamec[1];
                    ViewBag.month1 = dmnt1.ToString("MMM");
                }
                else
                {
                    ViewBag.month1 = "";
                }



















                //ViewBag.Open = feedbacksopen.Count();
                //ViewBag.Closed = feedbacksclosed.Count();
                //ViewBag.Resolved = feedbacksresolved.Count();
                //ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataChartsFeedbackMostFrequentLocations");

            }
        }


        /*

                public ViewResult charts()
                {

                    return View("DataCharts");
                }
        */

        /**************REJECT ACTION************************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("rejected/action/")]
        public ActionResult feedbackupdate(string submitButton, Feedback feedback)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && Request.Form["responsee"] == "")
                    {
                        db.Feedbacks.Attach(feedback);

                        ApplicationUser deptUser;
                        Department dep = db.Departments.Find(feedback.departmentID);
                        if (dep.name == Constants.OPERATIONS)
                        {
                               deptUser = feedInterface.getOperationsEscalationUser(Convert.ToInt32(Request.Form["costcentrId"]));
                           
                           
                        }
                        else
                        {
                            deptUser = feedInterface.getEscalationUser(feedback.departmentID, feedback.categoryId);
                        }

                        deptUser = db.Users.Find(deptUser.Id);
                        feedback.departUserId = deptUser.Id;
                        feedback.departUser = deptUser;
                      //  feedback.department = db.Departments.Find(feedback.departmentID);
                        feedback.checkStatus = Constants.ASSIGNED;                     
                            if (feedback.assignedDate == null)
                            {
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;                            
                            }
                                db.Entry(feedback).State = EntityState.Modified;
                                db.SaveChanges();
                        eventService.sendEmails(Request.Form["emailsss"], PopulateBody(feedback));
                        TempData["MessageSuccess"] = "Ticket has been Forwarded Successfully";

                    }
                    else
                    {
                        TempData["Message"] = "Comment field should be empty";
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                    return RedirectToAction("DashBoard");
                case "Submit":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {

                        if (feedback.status == Constants.CLOSED)
                        {
                            feedback.closedDate = DateTime.Now;
                            feedback.checkStatus = Constants.CLOSED;
                            TempData["MessageSuccess"] = "Ticket has been Closed Successfully";
                        }
                        else
                        {
                            feedback.resolvedDate = DateTime.Now;
                            feedback.checkStatus = Constants.RESOLVED;
                            TempData["MessageSuccess"] = "Ticket has been Resolved Successfully";
                        }
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.submittedById = user.Id;
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            
                            return RedirectToAction("DashBoard");                     
                    }
                    else
                    {
                        if (feedback.departmentID != null)
                        {
                            TempData["Message"] = "Department should be empty";
                        }
                        else
                        {

                            TempData["Message"] = "Comment Field should not be empty";
                        }
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                case "Reject":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                        feedback.checkStatus = Constants.REJECTED;
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.submittedById = user.Id;           //----------------later will use this to check rejectedby                           
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Rejected";
                            return RedirectToAction("DashBoard");                    
                    }
                    else
                    {
                        if (feedback.departmentID != null)
                        {
                            TempData["Message"] = "Department should be empty";
                        }
                        else
                        {

                            TempData["Message"] = "Comment Field should not be empty";
                        }
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                default:
                    return RedirectToAction("rejectedview", new { id = feedback.id });
            }
        }

        /******** HR update status Resolved to Open/Closed on User's satisfaction status******/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult updatestatus(Feedback feedback, string submitBtn)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.satisfaction = feedback.satisfaction;
            f.status = feedback.status;
            
         
            f.submittedById = user.Id;

            switch (submitBtn) {
                case "Submit":
                    if (feedback.status == Constants.CLOSED)
                    {
                        f.closedDate = DateTime.Now;
                        f.resolvedDate = null;
                        f.checkStatus = Constants.CLOSED;


                    }
                    else if(feedback.status==Constants.RESOLVED)
                    {
                        f.resolvedDate = f.resolvedDate;
                        f.checkStatus = Constants.RESOLVED;
                    }
                    break;

                case "Update":
                    if (feedback.status == Constants.RESOLVED)
                    {
                        f.resolvedDate = DateTime.Now;
                        f.closedDate = null;
                        f.checkStatus = Constants.RESOLVED;
                    }
                    else if(feedback.status==Constants.CLOSED)
                    {
                        f.closedDate = f.closedDate;
                        f.checkStatus = Constants.CLOSED;
                    }
                    break;
            }

            
            if (feedback.status == Constants.OPEN)
            {
                f.departmentID = null;
                f.resolvedDate = null;
                f.closedDate = null;
                f.departUserId = null;
                f.satisfaction = null;
                f.priorityId = null;
                f.categoryId = null;
                f.subcategoryId = null;
                f.responseDate = null;
                f.timeHours = 0;
                f.assignedBy = null;
                f.assignedDate = null;
                f.checkStatus = Constants.OPEN;
            }
            
       
            if (ModelState.IsValid)
            {

                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                TempData["displayMsg"] = "Ticket has been Updated Successfully";
                ViewData["decide"] = feedInterface.getCOmments(feedback.id);
                return RedirectToAction("DashBoard");
            }
            else
            {
                ViewData["commentList"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();

                TempData["displayMsgErr"] = "Please enter fields properly";
                if (submitBtn == "Submit")
                {
                    return View("resolvedview", feedback);
                }
                else {
                    return View("closedview", feedback);
                }
                
            }
        }
      [HttpPost]
        public JsonResult getCategories(int depId, int type)
        {
            List<Category> categories = feedInterface.getCategories(depId,type);               
            return Json(categories);
        }
      [HttpPost]
        public JsonResult getSubCategories(int categoryId,int type)
        {
            List<SubCategory> subCategories = feedInterface.getSubCategories(categoryId,type);
            return Json(subCategories);
        }

     public  string PopulateBody(Feedback feedback)
        {
            ApplicationUser user = db.Users.Find(feedback.userId);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/HR/HRemail.html")))
            {
                body = reader.ReadToEnd();
            }      
            body = body.Replace("{Title}", feedback.title);
            body = body.Replace("{TicketId}", feedback.id);
            
         body = body.Replace("{Location}", user.Location.name);

            Debug.WriteLine(feedback.description + "hhhhh");
            
            body = body.Replace("{EmployeeId}", user.EmployeeId.ToString());
           body = body.Replace("{Description}",feedback.description);
           body = body.Replace("{email}", user.bussinessEmail);
            body = body.Replace("{issueClass}", "YES");
            
           if (feedback.attachment == null) {
               body = body.Replace("{Attachment}", "No");
           }
           else {
               body = body.Replace("{Attachment}", "Yes");
            }           
            body = body.Replace("{IssueEscalate}", "Yes"); 
            return body;
        }

        [HttpGet]
        public ActionResult HRemail() {

            return View();
        }
    }


    
}