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
using Microsoft.Office.Interop.Excel;
using System.Net.Http;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace icrm.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
       

        public HRController() {
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
                    filename = $@"{Guid.NewGuid()}" + ext;
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
                        if (feedback.departmentID != null && feedback.response == null)
                        {
                            if (ModelState.IsValid)
                            {
                                
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Today;
                                
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

                            feedback.submittedById = user.Id;
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
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


        /*****************Change Status By HR in FeedBack*********************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("update/")]
        public ActionResult update(Feedback feedback)
        {
            string type=Request.Form["typeoflink"];
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.FindByName("User").Users.FirstOrDefault() != null)
            {
                var userRole = roleManager.FindByName("User").Users.First();
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();

            }
            getAttributeList();
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

        /**************ASSIGN DEPARTMENT************************/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("assigndepart/")]
        public ActionResult assign(string submitButton,Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
                   
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && feedback.response == null)
                    {
                        feedback.department = db.Departments.Find(feedback.departmentID);
                        if (ModelState.IsValid)
                        {
                            if (feedback.assignedDate == null)
                            {
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Today;
                            }
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
                        TempData["Message"] = "Select Department/ Comment Field Should be Empty";
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                    return RedirectToAction("view", new { id = feedback.id });


                case "Resolve":
                    if (feedback.departmentID == null && feedback.response != null)
                    {
                        if (ModelState.IsValid)
                        {
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.submittedById = user.Id;
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
                        TempData["Message"] = "Response Field should not be empty / Deselect department";
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                  case "Rejected":
                    if (feedback.departmentID == null && feedback.response == null) {
                        if (ModelState.IsValid)
                        {
                            feedback.submittedById = user.Id;           //----------------later will use this to check rejectedby
                            feedback.status = "Rejected";
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Message"] = "Feedback Rejected";
                            return RedirectToAction("DashBoard");
                        }
                        else {
                            TempData["Message"] = "FeedBack Information is not Valid";
                            return RedirectToAction("view", new { id = feedback.id });
                        }
                    }
                    else
                    {
                        TempData["Message"] = "Response & Deaprtment Field should be empty";
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
                TempData["DateMsg"] = "Select StartDate And EndDate";
                return RedirectToAction("Dashboard");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);
              
                if (export == "excel")
                {
                    IEnumerable<Feedback> feedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status);
                    //Application application = new Application();
                    //Workbook workbook = application.Workbooks.Add(System.Reflection.Missing.Value);
                    //Worksheet worksheet = workbook.ActiveSheet;

                    //worksheet.Cells[1, 1] = "Ticket ID";
                    //worksheet.Cells[1, 2] = "Title";
                    //worksheet.Cells[1, 3] = "Incident Type";
                    //worksheet.Cells[1, 4] = "Status";
                    //worksheet.Cells[1, 5] = "Description";
                    //worksheet.Cells[1, 6] = "Name";
                    //worksheet.Cells[1, 7] = "Email ID";
                    //worksheet.Cells[1, 8] = "Phone Number";


                    //int row = 2;
                    //foreach (var item in feedbacks)
                    //{
                    //    worksheet.Cells[row, 1] = item.id;
                    //    worksheet.Cells[row, 2] = item.title;
                    //    worksheet.Cells[row, 3] = "";
                    //    worksheet.Cells[row, 4] = item.status;
                    //    worksheet.Cells[row, 5] = item.description;
                    //    worksheet.Cells[row, 6] = item.user.FirstName;
                    //    worksheet.Cells[row, 7] = item.user.Email;
                    //    worksheet.Cells[row, 8] = item.user.PhoneNumber;



                    //    row++;
                    //}
                    //workbook.SaveAs("D:\\tempex/myreport.xlsx");

                    ////workbook.Close();
                    //string tempPath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + "_temp";
                    //workbook.SaveAs(tempPath, workbook.FileFormat);
                    //tempPath = workbook.FullName;
                    //workbook.Close();
                    //byte[] result = System.IO.File.ReadAllBytes(tempPath);
                    //System.IO.File.Delete(tempPath);

                    //this.Response.AddHeader("Content-Disposition", "Employees.xls");
                    //this.Response.ContentType = "application/vnd.ms-excel";

                    //return File(result, "application/vnd.ms-excel");

                    var grid = new GridView();
                    grid.DataSource = feedbacks;
                    grid.DataBind();

                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachement; filename=data.xls");
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

        /*****************VIEW OPEN  TICKET********************/

        public ActionResult openview(string  id)
        {
            var param2 = new SqlParameter();
            param2.ParameterName = "@TotalCount";
            param2.SqlDbType = SqlDbType.Int;
            var resil = db.Feedbacks.SqlQuery("totalRecords @TotalCount OUTPUT", param2);
            

            Debug.WriteLine( param2.Value);
            ViewBag.fff = param2.Value;
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
                return View("view",f);
            }
        }

        /*****************VIEW RESOLVED  TICKET********************/

        public ActionResult resolvedview(string id)
        {           
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

        /*****************VIEW ASSIGNED  TICKET********************/

        public ActionResult assignedview(string id)
        {          
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


        /*****************VIEW RESPONDED  TICKET********************/

        public ActionResult respondedview(string id)
        {          
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

        /*****************VIEW CLOSED  TICKET********************/

        public ActionResult closedview(string id)
        {           
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
            var departments = db.Departments.OrderByDescending(m => m.name).ToList();
            var categories = db.Categories.OrderByDescending(m => m.name).ToList();
            var priorities = db.Priorities.OrderByDescending(m => m.priorityId).ToList();
            ViewBag.Departmn = departments;
            ViewBag.Categories = categories;
            ViewBag.Priorities = priorities;
            ViewBag.typeList = db.FeedbackTypes.OrderBy(m => m.Id).ToList();

        }



        [HttpPost]
        public JsonResult getEmpDetails(string id)
        {
            return Json(db.Users.Where(u => u.Id == id).FirstOrDefault());
        }
    }
}