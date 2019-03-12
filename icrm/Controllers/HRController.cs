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
using System.Web.UI.DataVisualization.Charting;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;



namespace icrm.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
        private EventService eventService;
        private string tmpChartName;


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

        [Route("hr/chartsfeedbacktypeexcel_n/")]

        public ActionResult chartsfeedbacktypeexcel_n(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartP(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }


        [HttpGet]

        [Route("hr/chartsfeedbacktypeexcel/")]

        public ActionResult chartsfeedbacktypeexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(Chart(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] Chart(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksinquiries = null;
            IEnumerable<Feedback> mnt1feedbackscompliants = null;
            IEnumerable<Feedback> mnt1feedbacksappreciations = null;
            IEnumerable<Feedback> mnt1feedbackssuggestions = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksinquiries = null;
            IEnumerable<Feedback> mnt2feedbackscompliants = null;
            IEnumerable<Feedback> mnt2feedbacksappreciations = null;
            IEnumerable<Feedback> mnt2feedbackssuggestions = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksinquiries = null;
            IEnumerable<Feedback> mnt3feedbackscompliants = null;
            IEnumerable<Feedback> mnt3feedbacksappreciations = null;
            IEnumerable<Feedback> mnt3feedbackssuggestions = null;




            if (!string.IsNullOrEmpty(mnt1))
            {
                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "2");
                mnt1feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                mnt1feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "4");
                mnt1feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "3");
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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt1.Year.ToString());
                mnt2feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "2");
                mnt2feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                mnt2feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "4");
                mnt2feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "3");
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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt1.Year.ToString());
                mnt3feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "2");
                mnt3feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                mnt3feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "4");
                mnt3feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "3");
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


            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Type");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Type";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;



            Legend legend1 = new Legend();
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Docking = Docking.Right;
            legend1.Enabled = true;
            legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend1.IsTextAutoFit = true;
            legend1.Name = dmnt1.ToString("MMM");
            chart.Legends.Add(legend1);

            Legend legend2 = new Legend();
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.BackColor = System.Drawing.Color.Transparent;
            legend2.Docking = Docking.Right;
            legend2.Enabled = true;
            legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend2.IsTextAutoFit = true;
            legend2.Name = dmnt2.ToString("MMM");
            chart.Legends.Add(legend2);

            Legend legend3 = new Legend();
            legend3.Alignment = System.Drawing.StringAlignment.Center;
            legend3.BackColor = System.Drawing.Color.Transparent;
            legend3.Docking = Docking.Right;
            legend3.Enabled = true;
            legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend3.IsTextAutoFit = true;
            legend3.Name = dmnt3.ToString("MMM");
            chart.Legends.Add(legend3);








            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (mnt1feedbacksall.Count() != 0)
            {
                series1.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { double.Parse(((double.Parse(mnt1feedbacksinquiries.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackscompliants.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksappreciations.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackssuggestions.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series1);
            }
            else
            {
                series1.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { 0, 0, 0, 0 });
                series1.Label = "0.00%";
                chart.Series.Add(series1);
                // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (mnt2feedbacksall.Count() != 0)
            {
                series2.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { double.Parse(((double.Parse(mnt2feedbacksinquiries.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackscompliants.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksappreciations.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackssuggestions.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series2);
            }
            else
            {
                series2.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { 0, 0, 0, 0 });
                series2.Label = "0.00%";
                chart.Series.Add(series2);
                // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (mnt3feedbacksall.Count() != 0)
            {
                series3.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { double.Parse(((double.Parse(mnt3feedbacksinquiries.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackscompliants.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksappreciations.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackssuggestions.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series3);
            }
            else
            {
                series3.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { 0, 0, 0, 0 });
                series3.Label = "0.00%";
                chart.Series.Add(series3);
                // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

            }

            chart.Series[0].IsValueShownAsLabel = true;
            chart.Series[1].IsValueShownAsLabel = true;
            chart.Series[2].IsValueShownAsLabel = true;


            if (mnt1feedbacksall.Count() != 0)
            {
                chart.Series[0].Label = "#PERCENT"; //#VALY
            }
            if (mnt2feedbacksall.Count() != 0)
            {
                chart.Series[1].Label = "#PERCENT";
            }
            if (mnt3feedbacksall.Count() != 0)
            {
                chart.Series[2].Label = "#PERCENT";
            }



            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }

        private Byte[] ChartP(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksinquiries = null;
            IEnumerable<Feedback> mnt1feedbackscompliants = null;
            IEnumerable<Feedback> mnt1feedbacksappreciations = null;
            IEnumerable<Feedback> mnt1feedbackssuggestions = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksinquiries = null;
            IEnumerable<Feedback> mnt2feedbackscompliants = null;
            IEnumerable<Feedback> mnt2feedbacksappreciations = null;
            IEnumerable<Feedback> mnt2feedbackssuggestions = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksinquiries = null;
            IEnumerable<Feedback> mnt3feedbackscompliants = null;
            IEnumerable<Feedback> mnt3feedbacksappreciations = null;
            IEnumerable<Feedback> mnt3feedbackssuggestions = null;




            if (!string.IsNullOrEmpty(mnt1))
            {
                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "2");
                mnt1feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                mnt1feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "4");
                mnt1feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "3");
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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt1.Year.ToString());
                mnt2feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "2");
                mnt2feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                mnt2feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "4");
                mnt2feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "3");
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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt1.Year.ToString());
                mnt3feedbacksinquiries = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "2");
                mnt3feedbackscompliants = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                mnt3feedbacksappreciations = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "4");
                mnt3feedbackssuggestions = feedInterface.chartsFeedbackTypes(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "3");
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


            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Type");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Type";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;



            Legend legend1 = new Legend();
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Docking = Docking.Right;
            legend1.Enabled = true;
            legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend1.IsTextAutoFit = true;
            legend1.Name = dmnt1.ToString("MMM");
            chart.Legends.Add(legend1);

            Legend legend2 = new Legend();
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.BackColor = System.Drawing.Color.Transparent;
            legend2.Docking = Docking.Right;
            legend2.Enabled = true;
            legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend2.IsTextAutoFit = true;
            legend2.Name = dmnt2.ToString("MMM");
            chart.Legends.Add(legend2);

            Legend legend3 = new Legend();
            legend3.Alignment = System.Drawing.StringAlignment.Center;
            legend3.BackColor = System.Drawing.Color.Transparent;
            legend3.Docking = Docking.Right;
            legend3.Enabled = true;
            legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend3.IsTextAutoFit = true;
            legend3.Name = dmnt3.ToString("MMM");
            chart.Legends.Add(legend3);








            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            series1.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { mnt1feedbacksinquiries.Count(), mnt1feedbackscompliants.Count(), mnt1feedbacksappreciations.Count(), mnt1feedbackssuggestions.Count() });

            chart.Series.Add(series1);

            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            series2.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { mnt2feedbacksinquiries.Count(), mnt2feedbackscompliants.Count(), mnt2feedbacksappreciations.Count(), mnt2feedbackssuggestions.Count() });

            chart.Series.Add(series2);

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            series3.Points.DataBindXY(new[] { "Inquiries", "Complaints", "Appreciation", "Suggestions" }, new[] { mnt3feedbacksinquiries.Count(), mnt3feedbackscompliants.Count(), mnt3feedbacksappreciations.Count(), mnt3feedbackssuggestions.Count() });

            chart.Series.Add(series3);

            chart.Series[0].IsValueShownAsLabel = true;
            chart.Series[1].IsValueShownAsLabel = true;
            chart.Series[2].IsValueShownAsLabel = true;

            chart.Series[0].Label = "#VALY"; //#VALY
            chart.Series[1].Label = "#VALY";
            chart.Series[2].Label = "#VALY";



            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string tmpChartName = "ChartImage.jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;
                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }

        [HttpGet]

        [Route("hr/chartsfeedbacktypesearch/")]

        public ActionResult chartsfeedbacktypesearch(ChartMonths c)
        {


            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel_p"))
            {
                return RedirectToAction("chartsfeedbacktypeexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }
            else if (c.charts.Equals("Download Excel_n"))
            {
                return RedirectToAction("chartsfeedbacktypeexcel_n", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacktype");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbacksourceexcel/")]

        public ActionResult chartsfeedbacksourceexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackSource(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackSource(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation
            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbackswalkin = null;
            IEnumerable<Feedback> mnt1feedbackswhatsapp = null;
            IEnumerable<Feedback> mnt1feedbacksmobile = null;
            IEnumerable<Feedback> mnt1feedbackstollfree = null;
            IEnumerable<Feedback> mnt1feedbacksemail = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbackswalkin = null;
            IEnumerable<Feedback> mnt2feedbackswhatsapp = null;
            IEnumerable<Feedback> mnt2feedbacksmobile = null;
            IEnumerable<Feedback> mnt2feedbackstollfree = null;
            IEnumerable<Feedback> mnt2feedbacksemail = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbackswalkin = null;
            IEnumerable<Feedback> mnt3feedbackswhatsapp = null;
            IEnumerable<Feedback> mnt3feedbacksmobile = null;
            IEnumerable<Feedback> mnt3feedbackstollfree = null;
            IEnumerable<Feedback> mnt3feedbacksemail = null;




            if (!string.IsNullOrEmpty(mnt1))
            {
                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Walkin");
                mnt1feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "whatsapp");
                mnt1feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Mobile");
                mnt1feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Toll Free");
                mnt1feedbacksemail = feedInterface.chartsFeedbackSource(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "E-Mail");
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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Walkin");
                mnt2feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "whatsapp");
                mnt2feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Mobile");
                mnt2feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Toll Free");
                mnt2feedbacksemail = feedInterface.chartsFeedbackSource(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "E-Mail");
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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackswalkin = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Walkin");
                mnt3feedbackswhatsapp = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "whatsapp");
                mnt3feedbacksmobile = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Mobile");
                mnt3feedbackstollfree = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Toll Free");
                mnt3feedbacksemail = feedInterface.chartsFeedbackSource(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "E-Mail");
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


            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Source");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Source";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { double.Parse(((double.Parse(mnt1feedbackswalkin.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackswhatsapp.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksmobile.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackstollfree.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksemail.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";
                    chart.Series[0].Points[2].Label = series1.Points[2].YValues[0] + "%";
                    chart.Series[0].Points[3].Label = series1.Points[3].YValues[0] + "%";
                    chart.Series[0].Points[4].Label = series1.Points[4].YValues[0] + "%";
                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { 0, 0, 0, 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { double.Parse(((double.Parse(mnt2feedbackswalkin.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackswhatsapp.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksmobile.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackstollfree.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksemail.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";
                    chart.Series[1].Points[2].Label = series2.Points[2].YValues[0] + "%";
                    chart.Series[1].Points[3].Label = series2.Points[3].YValues[0] + "%";
                    chart.Series[1].Points[4].Label = series2.Points[4].YValues[0] + "%";
                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { 0, 0, 0, 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { double.Parse(((double.Parse(mnt3feedbackswalkin.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackswhatsapp.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksmobile.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackstollfree.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksemail.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";
                    chart.Series[2].Points[2].Label = series3.Points[2].YValues[0] + "%";
                    chart.Series[2].Points[3].Label = series3.Points[3].YValues[0] + "%";
                    chart.Series[2].Points[4].Label = series3.Points[4].YValues[0] + "%";
                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Walkin", "whatsapp", "Mobile", "Toll Free", "E-Mail" }, new[] { 0, 0, 0, 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }




        [HttpGet]

        [Route("hr/chartsfeedbacksourcesearch/")]

        public ActionResult chartsfeedbacksourcesearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbacksourceexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }



            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksource");
            }
            else
            {
                //02 / 2019
                /* DateTime dmnt1 = new DateTime();
                 DateTime dmnt2 = new DateTime();
                 DateTime dmnt3 = new DateTime();
                 */
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

        [Route("hr/chartsfeedbackdepartmentexcel/")]

        public ActionResult chartsfeedbackdepartmentexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackDepartment(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackDepartment(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksfinance = null;
            IEnumerable<Feedback> mnt1feedbackstalentmanagement = null;
            IEnumerable<Feedback> mnt1feedbacksadministrations = null;
            IEnumerable<Feedback> mnt1feedbacksoperations = null;
            IEnumerable<Feedback> mnt1feedbackssahlfeedback = null;
            IEnumerable<Feedback> mnt1feedbackssahlmds = null;
            IEnumerable<Feedback> mnt1feedbackssahltraining = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksfinance = null;
            IEnumerable<Feedback> mnt2feedbackstalentmanagement = null;
            IEnumerable<Feedback> mnt2feedbacksadministrations = null;
            IEnumerable<Feedback> mnt2feedbacksoperations = null;
            IEnumerable<Feedback> mnt2feedbackssahlfeedback = null;
            IEnumerable<Feedback> mnt2feedbackssahlmds = null;
            IEnumerable<Feedback> mnt2feedbackssahltraining = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksfinance = null;
            IEnumerable<Feedback> mnt3feedbackstalentmanagement = null;
            IEnumerable<Feedback> mnt3feedbacksadministrations = null;
            IEnumerable<Feedback> mnt3feedbacksoperations = null;
            IEnumerable<Feedback> mnt3feedbackssahlfeedback = null;
            IEnumerable<Feedback> mnt3feedbackssahlmds = null;
            IEnumerable<Feedback> mnt3feedbackssahltraining = null;



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




                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Finance");
                mnt1feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Talent Management & Corporate Compliance");
                mnt1feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Administration");
                mnt1feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Operations");
                mnt1feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Sahl");
                mnt1feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
                mnt1feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Training, Learning & Performance");
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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Finance");
                mnt2feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Talent Management & Corporate Compliance");
                mnt2feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Administration");
                mnt2feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Operations");
                mnt2feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Sahl");
                mnt2feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
                mnt2feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Training, Learning & Performance");
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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacksfinance = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Finance");
                mnt3feedbackstalentmanagement = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Talent Management & Corporate Compliance");
                mnt3feedbacksadministrations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Administration");
                mnt3feedbacksoperations = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Operations");
                mnt3feedbackssahlfeedback = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Sahl");
                mnt3feedbackssahlmds = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
                mnt3feedbackssahltraining = feedInterface.chartsFeedbackDepartment(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Training, Learning & Performance");
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


            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Department");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Department";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { double.Parse(((double.Parse(mnt1feedbacksfinance.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackstalentmanagement.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksadministrations.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksoperations.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackssahlfeedback.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackssahlmds.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackssahltraining.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";
                    chart.Series[0].Points[2].Label = series1.Points[2].YValues[0] + "%";
                    chart.Series[0].Points[3].Label = series1.Points[3].YValues[0] + "%";
                    chart.Series[0].Points[4].Label = series1.Points[4].YValues[0] + "%";
                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { 0, 0, 0, 0, 0, 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { double.Parse(((double.Parse(mnt2feedbacksfinance.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackstalentmanagement.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksadministrations.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksoperations.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackssahlfeedback.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackssahlmds.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackssahltraining.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";
                    chart.Series[1].Points[2].Label = series2.Points[2].YValues[0] + "%";
                    chart.Series[1].Points[3].Label = series2.Points[3].YValues[0] + "%";
                    chart.Series[1].Points[4].Label = series2.Points[4].YValues[0] + "%";
                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { 0, 0, 0, 0, 0, 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { double.Parse(((double.Parse(mnt3feedbacksfinance.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackstalentmanagement.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksadministrations.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksoperations.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackssahlfeedback.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackssahlmds.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackssahltraining.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";
                    chart.Series[2].Points[2].Label = series3.Points[2].YValues[0] + "%";
                    chart.Series[2].Points[3].Label = series3.Points[3].YValues[0] + "%";
                    chart.Series[2].Points[4].Label = series3.Points[4].YValues[0] + "%";
                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training" }, new[] { 0, 0, 0, 0, 0, 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }


        [HttpGet]

        [Route("hr/chartsfeedbackdepartmentsearch/")]

        public ActionResult chartsfeedbackdepartmentsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackdepartmentexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackdepartment");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbackpositionexcel/")]

        public ActionResult chartsfeedbackpositionexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackPosition(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackPosition(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbackscrews = null;
            IEnumerable<Feedback> mnt1feedbacksdrivers = null;
            IEnumerable<Feedback> mnt1feedbacksmanagers = null;
            IEnumerable<Feedback> mnt1feedbacksmds = null;
            IEnumerable<Feedback> mnt1feedbacksstars = null;
            IEnumerable<Feedback> mnt1feedbacksmaintenance = null;
            IEnumerable<Feedback> mnt1feedbacksgel = null;
            IEnumerable<Feedback> mnt1feedbacksspecialist = null;
            IEnumerable<Feedback> mnt1feedbacksconsultants = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbackscrews = null;
            IEnumerable<Feedback> mnt2feedbacksdrivers = null;
            IEnumerable<Feedback> mnt2feedbacksmanagers = null;
            IEnumerable<Feedback> mnt2feedbacksmds = null;
            IEnumerable<Feedback> mnt2feedbacksstars = null;
            IEnumerable<Feedback> mnt2feedbacksmaintenance = null;
            IEnumerable<Feedback> mnt2feedbacksgel = null;
            IEnumerable<Feedback> mnt2feedbacksspecialist = null;
            IEnumerable<Feedback> mnt2feedbacksconsultants = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbackscrews = null;
            IEnumerable<Feedback> mnt3feedbacksdrivers = null;
            IEnumerable<Feedback> mnt3feedbacksmanagers = null;
            IEnumerable<Feedback> mnt3feedbacksmds = null;
            IEnumerable<Feedback> mnt3feedbacksstars = null;
            IEnumerable<Feedback> mnt3feedbacksmaintenance = null;
            IEnumerable<Feedback> mnt3feedbacksgel = null;
            IEnumerable<Feedback> mnt3feedbacksspecialist = null;
            IEnumerable<Feedback> mnt3feedbacksconsultants = null;


            if (!string.IsNullOrEmpty(mnt1))
            {





                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Crews");
                mnt1feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Drivers");
                mnt1feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Managers");
                mnt1feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "MDS");
                mnt1feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Stars");
                mnt1feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Maintenance");
                mnt1feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "GEL");
                mnt1feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Specialist");
                mnt1feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Consultants");
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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Crews");
                mnt2feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Drivers");
                mnt2feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Managers");
                mnt2feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "MDS");
                mnt2feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Stars");
                mnt2feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Maintenance");
                mnt2feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "GEL");
                mnt2feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Specialist");
                mnt2feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Consultants");
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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackscrews = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Crews");
                mnt3feedbacksdrivers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Drivers");
                mnt3feedbacksmanagers = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Managers");
                mnt3feedbacksmds = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "MDS");
                mnt3feedbacksstars = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Stars");
                mnt3feedbacksmaintenance = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Maintenance");
                mnt3feedbacksgel = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "GEL");
                mnt3feedbacksspecialist = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Specialist");
                mnt3feedbacksconsultants = feedInterface.chartsFeedbackPosition(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Consultants");
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




            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Position");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Position";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { double.Parse(((double.Parse(mnt1feedbackscrews.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksdrivers.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksmanagers.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksmds.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksstars.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksmaintenance.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksgel.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksspecialist.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksconsultants.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";
                    chart.Series[0].Points[2].Label = series1.Points[2].YValues[0] + "%";
                    chart.Series[0].Points[3].Label = series1.Points[3].YValues[0] + "%";
                    chart.Series[0].Points[4].Label = series1.Points[4].YValues[0] + "%";
                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { double.Parse(((double.Parse(mnt2feedbackscrews.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksdrivers.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksmanagers.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksmds.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksstars.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksmaintenance.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksgel.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksspecialist.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksconsultants.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";
                    chart.Series[1].Points[2].Label = series2.Points[2].YValues[0] + "%";
                    chart.Series[1].Points[3].Label = series2.Points[3].YValues[0] + "%";
                    chart.Series[1].Points[4].Label = series2.Points[4].YValues[0] + "%";
                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { double.Parse(((double.Parse(mnt3feedbackscrews.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksdrivers.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksmanagers.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksmds.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksstars.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksmaintenance.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksgel.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksspecialist.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksconsultants.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";
                    chart.Series[2].Points[2].Label = series3.Points[2].YValues[0] + "%";
                    chart.Series[2].Points[3].Label = series3.Points[3].YValues[0] + "%";
                    chart.Series[2].Points[4].Label = series3.Points[4].YValues[0] + "%";
                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants" }, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }

        [HttpGet]

        [Route("hr/chartsfeedbackpositionsearch/")]

        public ActionResult chartsfeedbackpositionsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackpositionexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackposition");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbacknationalityexcel/")]

        public ActionResult chartsfeedbacknationalityexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackNationality(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackNationality(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksaudi = null;
            IEnumerable<Feedback> mnt1feedbacksexpatriates = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksaudi = null;
            IEnumerable<Feedback> mnt2feedbacksexpatriates = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksaudi = null;
            IEnumerable<Feedback> mnt3feedbacksexpatriates = null;


            if (!string.IsNullOrEmpty(mnt1))
            {




                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt1.Month.ToString(), dmnt1.Year.ToString());

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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt2.Month.ToString(), dmnt2.Year.ToString());

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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacksaudi = feedInterface.chartsFeedbackNationalitySaudi(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacksexpatriates = feedInterface.chartsFeedbackNationalityExpatriates(dmnt3.Month.ToString(), dmnt3.Year.ToString());

                ViewBag.mnt3feedbacksaudi = mnt3feedbacksaudi.Count();
                ViewBag.mnt3feedbacksexpatriates = mnt3feedbacksexpatriates.Count();

                ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                ViewBag.month3 = dmnt3.ToString("MMM");

            }
            else
            {
                ViewBag.month3 = "";

            }




            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Nationality");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Nationality";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { double.Parse(((double.Parse(mnt1feedbacksaudi.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksexpatriates.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { double.Parse(((double.Parse(mnt2feedbacksaudi.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksexpatriates.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";

                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { double.Parse(((double.Parse(mnt3feedbacksaudi.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksexpatriates.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";

                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Saudis", "Expatriates" }, new[] { 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }


        [HttpGet]

        [Route("hr/chartsfeedbacknationalitysearch/")]

        public ActionResult chartsfeedbacknationalitysearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbacknationalityexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacknationality");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbacksatisfactionexcel/")]

        public ActionResult chartsfeedbacksatisfactionexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackSatisfaction(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackSatisfaction(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksatisfied = null;
            IEnumerable<Feedback> mnt1feedbacksunsatisfied = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksatisfied = null;
            IEnumerable<Feedback> mnt2feedbacksunsatisfied = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksatisfied = null;
            IEnumerable<Feedback> mnt3feedbacksunsatisfied = null;


            if (!string.IsNullOrEmpty(mnt1))
            {






                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.SATISFIED);
                mnt1feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt1.Month.ToString(), dmnt1.Year.ToString(), Constants.UN_SATISFIED);

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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.SATISFIED);
                mnt2feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt2.Month.ToString(), dmnt2.Year.ToString(), Constants.UN_SATISFIED);

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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacksatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.SATISFIED);
                mnt3feedbacksunsatisfied = feedInterface.chartsFeedbackSatisfaction(dmnt3.Month.ToString(), dmnt3.Year.ToString(), Constants.UN_SATISFIED);

                ViewBag.mnt3feedbacksatisfied = mnt3feedbacksatisfied.Count();
                ViewBag.mnt3feedbacksunsatisfied = mnt3feedbacksunsatisfied.Count();

                ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                ViewBag.month3 = dmnt3.ToString("MMM");

            }
            else
            {
                ViewBag.month3 = "";

            }




            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Satisfaction");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Satisfaction";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { double.Parse(((double.Parse(mnt1feedbacksatisfied.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksunsatisfied.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { double.Parse(((double.Parse(mnt2feedbacksatisfied.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksunsatisfied.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";

                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { double.Parse(((double.Parse(mnt3feedbacksatisfied.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksunsatisfied.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";

                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Satisfied", "Not Satisfied" }, new[] { 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }




        [HttpGet]

        [Route("hr/chartsfeedbacksatisfactionsearch/")]

        public ActionResult chartsfeedbacksatisfactionsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbacksatisfactionexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksatisfaction");
            }
            else
            {
                //02 / 2019


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



            IEnumerable<Feedback> mnt2feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
            IEnumerable<Feedback> mnt2feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");


            IEnumerable<Feedback> mnt3feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
            IEnumerable<Feedback> mnt3feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");


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

        [Route("hr/chartsfeedbackescalationexcel/")]

        public ActionResult chartsfeedbackescalationexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackEscalation(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackEscalation(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbackescalated = null;
            IEnumerable<Feedback> mnt1feedbacksnotescalated = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbackescalated = null;
            IEnumerable<Feedback> mnt2feedbacksnotescalated = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbackescalated = null;
            IEnumerable<Feedback> mnt3feedbacksnotescalated = null;

            if (!string.IsNullOrEmpty(mnt1))
            {





                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                mnt1feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");

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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                mnt2feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");

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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                mnt3feedbacksnotescalated = feedInterface.chartsFeedbackEscalation(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");

                ViewBag.mnt3feedbackescalated = mnt3feedbackescalated.Count();
                ViewBag.mnt3feedbacksnotescalated = mnt3feedbacksnotescalated.Count();

                ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                ViewBag.month3 = dmnt3.ToString("MMM");

            }
            else
            {
                ViewBag.month3 = "";

            }





            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Escalation");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Escalation";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { double.Parse(((double.Parse(mnt1feedbackescalated.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksnotescalated.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { double.Parse(((double.Parse(mnt2feedbackescalated.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksnotescalated.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";

                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { double.Parse(((double.Parse(mnt3feedbackescalated.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksnotescalated.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";

                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Escalated", "Not Escalated" }, new[] { 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }



        [HttpGet]

        [Route("hr/chartsfeedbackescalationsearch/")]

        public ActionResult chartsfeedbackescalationsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackescalationexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackescalation");
            }
            else
            {
                //02 / 2019


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

        //chartsfeedbacklast12Monthssearch

        public ActionResult chartsfeedbacklast12monthsexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackLast12Months(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackLast12Months(string mnt1, string mnt2, string mnt3)
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

            string[] month = new string[12];
            month[0] = dmnt1[0].ToString("MMM") + "-" + dmnt1[0].Year;
            month[1] = dmnt1[1].ToString("MMM") + "-" + dmnt1[1].Year;
            month[2] = dmnt1[2].ToString("MMM") + "-" + dmnt1[2].Year;
            month[3] = dmnt1[3].ToString("MMM") + "-" + dmnt1[3].Year;
            month[4] = dmnt1[4].ToString("MMM") + "-" + dmnt1[4].Year;
            month[5] = dmnt1[5].ToString("MMM") + "-" + dmnt1[5].Year;
            month[6] = dmnt1[6].ToString("MMM") + "-" + dmnt1[6].Year;
            month[7] = dmnt1[7].ToString("MMM") + "-" + dmnt1[7].Year;
            month[8] = dmnt1[8].ToString("MMM") + "-" + dmnt1[8].Year;
            month[9] = dmnt1[9].ToString("MMM") + "-" + dmnt1[9].Year;
            month[10] = dmnt1[10].ToString("MMM") + "-" + dmnt1[10].Year;
            month[11] = dmnt1[11].ToString("MMM") + "-" + dmnt1[11].Year;

            /*report*/

            var chart = new Chart
            {
                Width = 1200,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Last 12 Months");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Last 12 Months";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisX.Interval = 1;

            var series1 = new Series("Last 12 Months");

            // Frist parameter is X-Axis and Second is Collection of Y- Axis



            series1.Points.DataBindXY(new[] { month[0], month[1], month[2], month[3], month[4], month[5], month[6], month[7], month[8], month[9], month[10], month[11] }, new[] { mnt1feedbacksall[0].Count(), mnt1feedbacksall[1].Count(), mnt1feedbacksall[2].Count(), mnt1feedbacksall[3].Count(),
                        mnt1feedbacksall[4].Count(), mnt1feedbacksall[5].Count(), mnt1feedbacksall[6].Count(), mnt1feedbacksall[7].Count(), mnt1feedbacksall[8].Count(), mnt1feedbacksall[9].Count(), mnt1feedbacksall[10].Count(),
                        mnt1feedbacksall[11].Count()
                });

            chart.Series.Add(series1);
            chart.Series[0]["PixelPointWidth"] = "10";

            chart.Series[0].IsValueShownAsLabel = true;
            chart.Series[0].Label = "#VALY"; //#VALY

            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }



        [HttpGet]

        [Route("hr/chartsfeedbacklast12Monthssearch/")]

        public ActionResult chartsfeedbacklast12Monthssearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbacklast12monthsexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            return View("Index");

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

        [Route("hr/chartsfeedbackmostfrequentexcel/")]

        public ActionResult chartsfeedbackmostfrequentexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackMostFrequent(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackMostFrequent(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacksalarytimeissue = null;
            IEnumerable<Feedback> mnt1feedbackhousingservices = null;
            IEnumerable<Feedback> mnt1feedbackpoortreatment = null;
            IEnumerable<Feedback> mnt1feedbackaccomodation = null;


            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacksalarytimeissue = null;
            IEnumerable<Feedback> mnt2feedbackhousingservices = null;
            IEnumerable<Feedback> mnt2feedbackpoortreatment = null;
            IEnumerable<Feedback> mnt2feedbackaccomodation = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacksalarytimeissue = null;
            IEnumerable<Feedback> mnt3feedbackhousingservices = null;
            IEnumerable<Feedback> mnt3feedbackpoortreatment = null;
            IEnumerable<Feedback> mnt3feedbackaccomodation = null;


            if (!string.IsNullOrEmpty(mnt1))
            {





                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt1.Month.ToString(), dmnt1.Year.ToString());

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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt2.Month.ToString(), dmnt2.Year.ToString());

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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacksalarytimeissue = feedInterface.chartsFeedbackSalaryTimeSheet(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackhousingservices = feedInterface.chartsFeedbackHousing(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackpoortreatment = feedInterface.chartsFeedbackPoorTreatment(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackaccomodation = feedInterface.chartsFeedbackAccomodationSupplies(dmnt3.Month.ToString(), dmnt3.Year.ToString());

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





            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Most Frequent Complaints");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Most Frequent Complaints";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;


            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { mnt1feedbacksalarytimeissue.Count(), mnt1feedbackhousingservices.Count(), mnt1feedbackpoortreatment.Count(), mnt1feedbackaccomodation.Count() });
                    chart.Series.Add(series1);
                    /* chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                     chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";
                     chart.Series[0].Points[2].Label = series1.Points[2].YValues[0] + "%";
                     chart.Series[0].Points[3].Label = series1.Points[3].YValues[0] + "%";*/

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { 0, 0, 0, 0 });
                    series1.Label = "0";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { mnt2feedbacksalarytimeissue.Count(), mnt2feedbackhousingservices.Count(), mnt2feedbackpoortreatment.Count(), mnt2feedbackaccomodation.Count() });

                    chart.Series.Add(series2);
                    /* chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                     chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";
                     chart.Series[1].Points[2].Label = series2.Points[2].YValues[0] + "%";
                     chart.Series[1].Points[3].Label = series2.Points[3].YValues[0] + "%";*/

                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { 0, 0, 0, 0 });
                    series2.Label = "0";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { mnt3feedbacksalarytimeissue.Count(), mnt3feedbackhousingservices.Count(), mnt3feedbackpoortreatment.Count(), mnt3feedbackaccomodation.Count() });

                    chart.Series.Add(series3);
                    /* chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                     chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";
                     chart.Series[2].Points[2].Label = series3.Points[2].YValues[0] + "%";
                     chart.Series[2].Points[3].Label = series3.Points[3].YValues[0] + "%";*/

                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Salary & Time sheet Issues", "Housing , other logistics, health insurance & Mobile services", "Poor Treatment", "Accommodation supplies / Equipment / Maintenance" }, new[] { 0, 0, 0, 0 });
                    series3.Label = "0";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }









            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }




        [HttpGet]

        [Route("hr/chartsfeedbackmostfrequentsearch/")]

        public ActionResult chartsfeedbackmostfrequentsearch(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackmostfrequentexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackmostfrequent");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbacksalaryissuesreasonsexcel/")]

        public ActionResult chartsfeedbacksalaryissuesreasonsexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackSalaryIssuesReasons(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackSalaryIssuesReasons(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbacklegitimate = null;
            IEnumerable<Feedback> mnt1feedbackillegitimate = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbacklegitimate = null;
            IEnumerable<Feedback> mnt2feedbackillegitimate = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbacklegitimate = null;
            IEnumerable<Feedback> mnt3feedbackillegitimate = null;

            if (!string.IsNullOrEmpty(mnt1))
            {




                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "1");
                mnt1feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "0");

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
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "1");
                mnt2feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "0");

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
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbacklegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "1");
                mnt3feedbackillegitimate = feedInterface.chartsFeedbackSalaryIssuesReasons(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "0");

                ViewBag.mnt3feedbacklegitimate = mnt3feedbacklegitimate.Count();
                ViewBag.mnt3feedbackillegitimate = mnt3feedbackillegitimate.Count();

                ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                ViewBag.month3 = dmnt3.ToString("MMM");

            }
            else
            {
                ViewBag.month3 = "";

            }





            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Salary Issues Reasons");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Salary Issues Reasons";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            //chart.ChartAreas[0].AxisY.Maximum = 100;
            //chart.ChartAreas[0].AxisY.Minimum = 0;

            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }

            if (!string.IsNullOrEmpty(mnt2))
            {
                Legend legend2 = new Legend();
                legend2.Alignment = System.Drawing.StringAlignment.Center;
                legend2.BackColor = System.Drawing.Color.Transparent;
                legend2.Docking = Docking.Right;
                legend2.Enabled = true;
                legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend2.IsTextAutoFit = true;
                legend2.Name = dmnt2.ToString("MMM");
                chart.Legends.Add(legend2);
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                Legend legend3 = new Legend();
                legend3.Alignment = System.Drawing.StringAlignment.Center;
                legend3.BackColor = System.Drawing.Color.Transparent;
                legend3.Docking = Docking.Right;
                legend3.Enabled = true;
                legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend3.IsTextAutoFit = true;
                legend3.Name = dmnt3.ToString("MMM");
                chart.Legends.Add(legend3);
            }







            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (mnt1feedbacksall.Count() != 0)
                {
                    series1.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { double.Parse(((double.Parse(mnt1feedbacklegitimate.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackillegitimate.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });
                    chart.Series.Add(series1);
                    chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                    chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { 0, 0 });
                    series1.Label = "0.00%";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }


            var series2 = new Series(dmnt2.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt2))
            {
                if (mnt2feedbacksall.Count() != 0)
                {
                    series2.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { double.Parse(((double.Parse(mnt2feedbacklegitimate.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackillegitimate.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series2);
                    chart.Series[1].Points[0].Label = series2.Points[0].YValues[0] + "%";
                    chart.Series[1].Points[1].Label = series2.Points[1].YValues[0] + "%";

                    chart.Series[1].IsValueShownAsLabel = true;
                    chart.Series[1].Label = "#VALY";//"#PERCENT";



                }
                else
                {
                    series2.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { 0, 0 });
                    series2.Label = "0.00%";
                    chart.Series.Add(series2);
                    chart.Series[1].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis
            if (!string.IsNullOrEmpty(mnt3))
            {
                if (mnt3feedbacksall.Count() != 0)
                {
                    series3.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { double.Parse(((double.Parse(mnt3feedbacklegitimate.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackillegitimate.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                    chart.Series.Add(series3);
                    chart.Series[2].Points[0].Label = series3.Points[0].YValues[0] + "%";
                    chart.Series[2].Points[1].Label = series3.Points[1].YValues[0] + "%";

                    chart.Series[2].IsValueShownAsLabel = true;
                    chart.Series[2].Label = "#VALY";
                }
                else
                {
                    series3.Points.DataBindXY(new[] { "Legitimate Claims", "Illegitimate Claims" }, new[] { 0, 0 });
                    series3.Label = "0.00%";
                    chart.Series.Add(series3);
                    chart.Series[2].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }














            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }


        [HttpGet]

        [Route("hr/chartsfeedbacksalaryissuesreasonssearch/")] // pending

        public ActionResult chartsfeedbacksalaryissuesreasons(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbacksalaryissuesreasonsexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbacksalaryissuesreasons");
            }
            else
            {
                //02 / 2019


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

        [Route("hr/chartsfeedbackmostfrequentlocationsexcel/")]

        public ActionResult chartsfeedbackmostfrequentlocationsexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartFeedbackMostFrequentLocations(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            //  string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartFeedbackMostFrequentLocations(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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

            //02 / 2019
            /* DateTime dmnt1 = new DateTime();
             DateTime dmnt2 = new DateTime();
             DateTime dmnt3 = new DateTime();
             */


            //1   Complaint
            //2   Enquiry
            //3   Suggestion
            //4   Appreciation

            if (!string.IsNullOrEmpty(mnt1))
            {





                string[] locationnamec = feedInterface.chartsFeedbackMostFrequentLocations(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                ViewBag.mnt1feedbackmostfrequentlocations = locationnamec[0];



                ViewBag.locationname = locationnamec[1];
                ViewBag.month1 = dmnt1.ToString("MMM");
            }
            else
            {
                ViewBag.month1 = "";
            }





            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Most Frequent Locations");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Most Frequent Locations";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;


            if (!string.IsNullOrEmpty(mnt1))
            {
                Legend legend1 = new Legend();
                legend1.Alignment = System.Drawing.StringAlignment.Center;
                legend1.BackColor = System.Drawing.Color.Transparent;
                legend1.Docking = Docking.Right;
                legend1.Enabled = true;
                legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
                legend1.IsTextAutoFit = true;
                legend1.Name = dmnt1.ToString("MMM");
                chart.Legends.Add(legend1);
            }









            var series1 = new Series(dmnt1.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis

            if (!string.IsNullOrEmpty(mnt1))
            {
                if (int.Parse(ViewBag.mnt1feedbackmostfrequentlocations) != 0)
                {
                    series1.Points.DataBindXY(new[] { ViewBag.locationname }, new[] { double.Parse(ViewBag.mnt1feedbackmostfrequentlocations) });
                    chart.Series.Add(series1);
                    /* chart.Series[0].Points[0].Label = series1.Points[0].YValues[0] + "%";
                     chart.Series[0].Points[1].Label = series1.Points[1].YValues[0] + "%";
                     chart.Series[0].Points[2].Label = series1.Points[2].YValues[0] + "%";
                     chart.Series[0].Points[3].Label = series1.Points[3].YValues[0] + "%";*/

                    chart.Series[0].IsValueShownAsLabel = true;
                    chart.Series[0].Label = "#VALY"; //#VALY
                }
                else
                {
                    series1.Points.DataBindXY(new[] { ViewBag.locationname }, new[] { 0 });
                    series1.Label = "0";
                    chart.Series.Add(series1);
                    chart.Series[0].IsValueShownAsLabel = true;
                    // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

                }
            }




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }



        [HttpGet]

        [Route("hr/chartsfeedbackmostfrequentlocationssearch/")]

        public ActionResult chartsfeedbackmostfrequentlocations(ChartMonths c)
        {
            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackmostfrequentlocationsexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbacktypeexcel");
            }



            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackmostfrequentlocations");
            }
            else
            {
                //02 / 2019


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





        /*  region */
        public ViewResult chartsfeedbackregion()
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
            /*
             var mnt1feedbackscentral = $("#mnt1feedbackscentral").val();

    var mnt1feedbackseastern = $("#mnt1feedbackseastern").val();

    var mnt1feedbacksnorthern = $("#mnt1feedbacksnorthern").val();
    var mnt1feedbacksheadoffice = $("#mnt1feedbacksheadoffice").val();

             
             */

            IEnumerable<Feedback> mnt1feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Central");
            IEnumerable<Feedback> mnt1feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Eastern");
            IEnumerable<Feedback> mnt1feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Northern");
            IEnumerable<Feedback> mnt1feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Head Office");

            IEnumerable<Feedback> mnt2feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Central");
            IEnumerable<Feedback> mnt2feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Eastern");
            IEnumerable<Feedback> mnt2feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Northern");
            IEnumerable<Feedback> mnt2feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Head Office");

            IEnumerable<Feedback> mnt3feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Central");
            IEnumerable<Feedback> mnt3feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Eastern");
            IEnumerable<Feedback> mnt3feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Northern");
            IEnumerable<Feedback> mnt3feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Head Office");

            ViewBag.mnt1feedbackscentral = mnt1feedbackscentral.Count();
            ViewBag.mnt1feedbackseastern = mnt1feedbackseastern.Count();
            ViewBag.mnt1feedbacksnorthern = mnt1feedbacksnorthern.Count();
            ViewBag.mnt1feedbacksheadoffice = mnt1feedbacksheadoffice.Count();

            ViewBag.mnt2feedbackscentral = mnt2feedbackscentral.Count();
            ViewBag.mnt2feedbackseastern = mnt2feedbackseastern.Count();
            ViewBag.mnt2feedbacksnorthern = mnt2feedbacksnorthern.Count();
            ViewBag.mnt2feedbacksheadoffice = mnt2feedbacksheadoffice.Count();

            ViewBag.mnt3feedbackscentral = mnt3feedbackscentral.Count();
            ViewBag.mnt3feedbackseastern = mnt3feedbackseastern.Count();
            ViewBag.mnt3feedbacksnorthern = mnt3feedbacksnorthern.Count();
            ViewBag.mnt3feedbacksheadoffice = mnt3feedbacksheadoffice.Count();

            ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
            ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
            ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();

            ViewBag.month1 = dmnt1.ToString("MMM");
            ViewBag.month2 = dmnt2.ToString("MMM");
            ViewBag.month3 = dmnt3.ToString("MMM");



            return View("DataChartsFeedbackRegion");

        }


        [HttpGet]

        [Route("hr/chartsfeedbackregionexcel/")]

        public ActionResult chartsfeedbackregionexcel(ChartMonths c)
        {

            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            var image = iTextSharp.text.Image.GetInstance(ChartRegion(mnt1, mnt2, mnt3));
            image.ScalePercent(75f);


            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=Chart.xls;");
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // string tmpChartName = "ChartImage.jpg";
            string imgPath = "hr/" + tmpChartName;


            string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/hr/" + tmpChartName);


            string headerTable = @"<table><tr><td><img src=" + imgPath2 + "></td></tr></table>";
            Response.Write(headerTable);
            Response.Write(stringWrite.ToString());
            Response.End();


            return View("Index");










            // return File(pdf, "application/pdf", "Chart.pdf");
        }

        private Byte[] ChartRegion(string mnt1, string mnt2, string mnt3)
        {

            /*report*/
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
               IEnumerable<Feedback> mnt1feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Central");
            IEnumerable<Feedback> mnt1feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Eastern");
            IEnumerable<Feedback> mnt1feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Northern");
            IEnumerable<Feedback> mnt1feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Head Office");
            
            */
            IEnumerable<Feedback> mnt1feedbacksall = null;
            IEnumerable<Feedback> mnt1feedbackscentral = null;
            IEnumerable<Feedback> mnt1feedbackseastern = null;
            IEnumerable<Feedback> mnt1feedbacksnorthern = null;
            IEnumerable<Feedback> mnt1feedbacksheadoffice = null;

            IEnumerable<Feedback> mnt2feedbacksall = null;
            IEnumerable<Feedback> mnt2feedbackscentral = null;
            IEnumerable<Feedback> mnt2feedbackseastern = null;
            IEnumerable<Feedback> mnt2feedbacksnorthern = null;
            IEnumerable<Feedback> mnt2feedbacksheadoffice = null;

            IEnumerable<Feedback> mnt3feedbacksall = null;
            IEnumerable<Feedback> mnt3feedbackscentral = null;
            IEnumerable<Feedback> mnt3feedbackseastern = null;
            IEnumerable<Feedback> mnt3feedbacksnorthern = null;
            IEnumerable<Feedback> mnt3feedbacksheadoffice = null;




            if (!string.IsNullOrEmpty(mnt1))
            {
                mnt1feedbacksall = feedInterface.chartsFeedbackAll(dmnt1.Month.ToString(), dmnt1.Year.ToString());
                mnt1feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Central");
                mnt1feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Eastern");
                mnt1feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Northern");
                mnt1feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Head Office");
                ViewBag.mnt1feedbackscentral = mnt1feedbackscentral.Count();
                ViewBag.mnt1feedbackseastern = mnt1feedbackseastern.Count();
                ViewBag.mnt1feedbacksnorthern = mnt1feedbacksnorthern.Count();
                ViewBag.mnt1feedbacksheadoffice = mnt1feedbacksheadoffice.Count();
                ViewBag.mnt1feedbacksall = mnt1feedbacksall.Count();
                ViewBag.month1 = dmnt1.ToString("MMM");
            }
            else
            {
                ViewBag.month1 = "";
            }



            if (!string.IsNullOrEmpty(mnt2))
            {
                mnt2feedbacksall = feedInterface.chartsFeedbackAll(dmnt2.Month.ToString(), dmnt2.Year.ToString());
                mnt2feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Central");
                mnt2feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Eastern");
                mnt2feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Northern");
                mnt2feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Head Office");
                ViewBag.mnt2feedbackscentral = mnt2feedbackscentral.Count();
                ViewBag.mnt2feedbackseastern = mnt2feedbackseastern.Count();
                ViewBag.mnt2feedbacksnorthern = mnt2feedbacksnorthern.Count();
                ViewBag.mnt2feedbacksheadoffice = mnt2feedbacksheadoffice.Count();
                ViewBag.mnt2feedbacksall = mnt2feedbacksall.Count();
                ViewBag.month2 = dmnt2.ToString("MMM");
            }
            else
            {
                ViewBag.month2 = "";
            }

            if (!string.IsNullOrEmpty(mnt3))
            {
                mnt3feedbacksall = feedInterface.chartsFeedbackAll(dmnt3.Month.ToString(), dmnt3.Year.ToString());
                mnt3feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Central");
                mnt3feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Eastern");
                mnt3feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Northern");
                mnt3feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Head Office");
                ViewBag.mnt3feedbackscentral = mnt3feedbackscentral.Count();
                ViewBag.mnt3feedbackseastern = mnt3feedbackseastern.Count();
                ViewBag.mnt3feedbacksnorthern = mnt3feedbacksnorthern.Count();
                ViewBag.mnt3feedbacksheadoffice = mnt3feedbacksheadoffice.Count();
                ViewBag.mnt3feedbacksall = mnt3feedbacksall.Count();
                ViewBag.month3 = dmnt3.ToString("MMM");

            }
            else
            {
                ViewBag.month3 = "";

            }


            /*report*/

            var chart = new Chart
            {
                Width = 800,
                Height = 450,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            chart.Titles.Add("Feedback Region");
            chart.Titles[0].Font = new Font("Arial", 16f);

            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = "Feedback Region";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;

            chart.ChartAreas[0].BackColor = Color.White;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;



            Legend legend1 = new Legend();
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Docking = Docking.Right;
            legend1.Enabled = true;
            legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend1.IsTextAutoFit = true;
            legend1.Name = dmnt1.ToString("MMM");
            chart.Legends.Add(legend1);

            Legend legend2 = new Legend();
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.BackColor = System.Drawing.Color.Transparent;
            legend2.Docking = Docking.Right;
            legend2.Enabled = true;
            legend2.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend2.IsTextAutoFit = true;
            legend2.Name = dmnt2.ToString("MMM");
            chart.Legends.Add(legend2);

            Legend legend3 = new Legend();
            legend3.Alignment = System.Drawing.StringAlignment.Center;
            legend3.BackColor = System.Drawing.Color.Transparent;
            legend3.Docking = Docking.Right;
            legend3.Enabled = true;
            legend3.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend3.IsTextAutoFit = true;
            legend3.Name = dmnt3.ToString("MMM");
            chart.Legends.Add(legend3);








            var series1 = new Series(dmnt1.ToString("MMM"));



            /*
             
               ViewBag.mnt3feedbackscentral = mnt3feedbackscentral.Count();
                ViewBag.mnt3feedbackseastern = mnt3feedbackseastern.Count();
                ViewBag.mnt3feedbacksnorthern = mnt3feedbacksnorthern.Count();
                ViewBag.mnt3feedbacksheadoffice = mnt3feedbacksheadoffice.Count();
             
             */

            // Frist parameter is X-Axis and Second is Collection of Y- Axis



            if (mnt1feedbacksall.Count() != 0)
            {
                series1.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { double.Parse(((double.Parse(mnt1feedbackscentral.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbackseastern.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksnorthern.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt1feedbacksheadoffice.Count().ToString()) / double.Parse(mnt1feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series1);
            }
            else
            {
                series1.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { 0, 0, 0, 0 });
                series1.Label = "0.00%";
                chart.Series.Add(series1);
                // chart.Series[dmnt1.ToString("MMM")].Label = "0%";

            }

            var series2 = new Series(dmnt2.ToString("MMM"));
            if (mnt2feedbacksall.Count() != 0)
            {


                // Frist parameter is X-Axis and Second is Collection of Y- Axis
                series2.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { double.Parse(((double.Parse(mnt2feedbackscentral.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbackseastern.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksnorthern.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt2feedbacksheadoffice.Count().ToString()) / double.Parse(mnt2feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series2);
            }
            else
            {
                series2.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { 0, 0, 0, 0 });
                series2.Label = "0.00%";
                chart.Series.Add(series2);

            }

            var series3 = new Series(dmnt3.ToString("MMM"));

            // Frist parameter is X-Axis and Second is Collection of Y- Axis


            if (mnt3feedbacksall.Count() != 0)
            {
                series3.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { double.Parse(((double.Parse(mnt3feedbackscentral.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbackseastern.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksnorthern.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")), double.Parse(((double.Parse(mnt3feedbacksheadoffice.Count().ToString()) / double.Parse(mnt3feedbacksall.Count().ToString())) * 100).ToString("0.00")) });

                chart.Series.Add(series3);
            }
            else
            {
                series3.Points.DataBindXY(new[] { "Central", "Eastern", "Northern", "Head Office" }, new[] { 0, 0, 0, 0 });

                series3.Label = "0.00%";
                chart.Series.Add(series3);

            }

            chart.Series[0].IsValueShownAsLabel = true;
            chart.Series[1].IsValueShownAsLabel = true;
            chart.Series[2].IsValueShownAsLabel = true;


            if (mnt1feedbacksall.Count() != 0)
            {
                chart.Series[0].Label = "#PERCENT";
            }

            if (mnt2feedbacksall.Count() != 0)
            {
                chart.Series[1].Label = "#PERCENT";
            }

            if (mnt3feedbacksall.Count() != 0)
            {
                chart.Series[2].Label = "#PERCENT";
            }


            /* Legend secondLegend = new Legend("Legend");

             chart.Legends.Add(mnt1);
             chart.Legends.Add(mnt2);
             chart.Legends.Add(mnt3);

             // Associate Series 2 with the second legend 
             chart.Series[0].Legend = mnt1;
             chart.Series[1].Legend = mnt2;
             chart.Series[2].Legend = mnt3;
             */



            /*  chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");
              chart.Series.Add("");

              chart.Series[0].ChartType = SeriesChartType.Column;
              chart.Series[1].ChartType = SeriesChartType.Column;
              chart.Series[2].ChartType = SeriesChartType.Column;
              chart.Series[3].ChartType = SeriesChartType.Column;


              chart.Series[0].Points.AddXY("Inquiries", mnt1feedbacksinquiries.Count());
              chart.Series[1].Points.AddXY("Complaints", mnt1feedbackscompliants.Count());
              chart.Series[2].Points.AddXY("Appreciation", mnt1feedbacksappreciations.Count());
              chart.Series[3].Points.AddXY("Suggestions", mnt1feedbackssuggestions.Count());*/




            // }
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                string timemilis = DateTime.UtcNow.Ticks.ToString();
                tmpChartName = "ChartImage" + timemilis + ".jpg";
                string imgPath = Server.MapPath(tmpChartName);// "hr/" +tmpChartName;

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("/") + "hr");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                System.IO.File.Delete(imgPath);


                chart.SaveImage(imgPath);

                string imgPath2 = Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/" + tmpChartName);



                return chartimage.GetBuffer();
            }
        }


        [HttpGet]

        [Route("hr/chartsfeedbackregionsearch/")]

        public ActionResult chartsfeedbackregionsearch(ChartMonths c)
        {


            string mnt1 = c.month1;
            string mnt2 = c.month2;
            string mnt3 = c.month3;

            DateTime dmnt1 = new DateTime();
            DateTime dmnt2 = new DateTime();
            DateTime dmnt3 = new DateTime();

            dmnt1 = DateTime.Now;
            dmnt2 = DateTime.Now.AddMonths(-1);
            dmnt3 = DateTime.Now.AddMonths(-2);

            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                string mnt1m = dmnt1.Month.ToString();
                string mnt2m = dmnt2.Month.ToString();
                string mnt3m = dmnt3.Month.ToString();

                if (!(mnt1m.Length > 1))
                {
                    mnt1m = "0" + mnt1m;
                }
                if (!(mnt2m.Length > 1))
                {
                    mnt2m = "0" + mnt2m;
                }
                if (!(mnt3m.Length > 1))
                {
                    mnt3m = "0" + mnt3m;
                }

                mnt1 = mnt1m + "/" + dmnt1.Year;
                mnt2 = mnt2m + "/" + dmnt2.Year;
                mnt3 = mnt3m + "/" + dmnt3.Year;
            }
            // 01 / 2019


            if (c.charts.Equals("Download Excel"))
            {
                return RedirectToAction("chartsfeedbackregionexcel", "HR", new
                {


                    month1 = mnt1,
                    month2 = mnt2,
                    month3 = mnt3,
                    charts = "Submit"
                });

                // return RedirectToAction("chartsfeedbackregionexcel");
            }


            if (string.IsNullOrEmpty(mnt1) && string.IsNullOrEmpty(mnt2) && string.IsNullOrEmpty(mnt3))
            {
                TempData["DateMsg"] = "Please Select  Month";
                return RedirectToAction("chartsfeedbackregion");
            }
            else
            {
                //02 / 2019


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
                    IEnumerable<Feedback> mnt1feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Central");
                    IEnumerable<Feedback> mnt1feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Eastern");
                    IEnumerable<Feedback> mnt1feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Northern");
                    IEnumerable<Feedback> mnt1feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt1.Month.ToString(), dmnt1.Year.ToString(), "Head Office");
                    ViewBag.mnt1feedbackscentral = mnt1feedbackscentral.Count();
                    ViewBag.mnt1feedbackseastern = mnt1feedbackseastern.Count();
                    ViewBag.mnt1feedbacksnorthern = mnt1feedbacksnorthern.Count();
                    ViewBag.mnt1feedbacksheadoffice = mnt1feedbacksheadoffice.Count();
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
                    IEnumerable<Feedback> mnt2feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Central");
                    IEnumerable<Feedback> mnt2feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Eastern");
                    IEnumerable<Feedback> mnt2feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Northern");
                    IEnumerable<Feedback> mnt2feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt2.Month.ToString(), dmnt2.Year.ToString(), "Head Office");
                    ViewBag.mnt2feedbackscentral = mnt2feedbackscentral.Count();
                    ViewBag.mnt2feedbackseastern = mnt2feedbackseastern.Count();
                    ViewBag.mnt2feedbacksnorthern = mnt2feedbacksnorthern.Count();
                    ViewBag.mnt2feedbacksheadoffice = mnt2feedbacksheadoffice.Count();
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
                    IEnumerable<Feedback> mnt3feedbackscentral = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Central");
                    IEnumerable<Feedback> mnt3feedbackseastern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Eastern");
                    IEnumerable<Feedback> mnt3feedbacksnorthern = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Northern");
                    IEnumerable<Feedback> mnt3feedbacksheadoffice = feedInterface.chartsFeedbackRegion(dmnt3.Month.ToString(), dmnt3.Year.ToString(), "Head Office");
                    ViewBag.mnt3feedbackscentral = mnt3feedbackscentral.Count();
                    ViewBag.mnt3feedbackseastern = mnt3feedbackseastern.Count();
                    ViewBag.mnt3feedbacksnorthern = mnt3feedbacksnorthern.Count();
                    ViewBag.mnt3feedbacksheadoffice = mnt3feedbacksheadoffice.Count();
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
                return View("DataChartsFeedbackRegion");

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