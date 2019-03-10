using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using icrm.RepositoryInterface;
using icrm.RepositoryImpl;
using System.Security.Cryptography;
using PagedList;

namespace icrm.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private IFeedback feedInterface;
        private GenericPagination<ApplicationUser> gp = new GenericPagination<ApplicationUser>();
        private ApplicationUserManager _userManager;

        public ActionResult Dashboard()
        {
            TicketCounts();
            return View();
        }


        public AdminController()
        {
            feedInterface = new FeedbackRepository();
        }
        public AdminController(ApplicationUserManager userManager)
        {
            this._userManager = userManager;    
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


        [HttpGet]
        public ActionResult AddUser(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            ViewBag.rolename = id;
            if (Request.Cookies["sucess"] != null) {
                Response.SetCookie(new HttpCookie("sucess", "") { Expires = DateTime.Now.AddDays(-1) });
                ViewBag.message = "User Saved Sucessfully";
            }
            else if (Request.Cookies["fail"] != null)
            {
                Response.SetCookie(new HttpCookie("fail", "") { Expires = DateTime.Now.AddDays(-1) });
                ViewBag.message = "Employee Id Doesn't Exist";
            }
            else if (Request.Cookies["already"] != null){
                Response.SetCookie(new HttpCookie("already", "") { Expires = DateTime.Now.AddDays(-1) });
                ViewBag.message = "Employee Has Already Been Created";
            }
           ViewBag.DepartmentList = context.Departments.ToList();
           return View();
            
          
        }

        public ActionResult postUser(RegisterViewModel model,string rolename) {

            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (ModelState.IsValid)
            {

                ApplicationUser user = UserManager.Users.Where(m => m.EmployeeId == model.EmployeeId).SingleOrDefault();

                

                if (user != null)
                {
                    if (user.PasswordHash != null) {
                        Response.SetCookie(new HttpCookie("already", ""));
                        return RedirectToAction("AddUser", new { @id = rolename });

                    }
                   
                    user.UserName = Convert.ToString(model.EmployeeId);
                    user.Email = user.bussinessEmail;
                    user.PasswordHash = HashPassword(model.Password);
                    user.SecurityStamp = Guid.NewGuid().ToString("D");
                }
                
                else {
                    Response.SetCookie(new HttpCookie("fail", ""));
                    return RedirectToAction("AddUser", new { @id = rolename });

                }
                user.LastPasswordChangedDate = DateTime.Now;
                var result = UserManager.Update(user);
                if (result.Succeeded)
                {
                    
                    if(rolename.Equals("HR"))
                         UserManager.AddToRole(user.Id, roleManager.FindByName("HR").Name);
                    else if (rolename.Equals("Department"))
                        UserManager.AddToRole(user.Id, roleManager.FindByName("Department").Name);



                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    Response.SetCookie(new HttpCookie("sucess", ""));
                    return RedirectToAction("AddUser", new { @id= rolename });
                }
                AddErrors(result);
            }



            // If we got this far, something failed, redisplay form
            if (rolename.Equals("Department")) {
                ViewBag.DepartmentList = context.Departments.ToList();
            }
            ViewBag.rolename = rolename;
            return View("AddUser",model);
          
        }

        [HttpGet]
        public ActionResult UserList(int? page) {
            ApplicationDbContext context = new ApplicationDbContext();
            ApplicationUser user = new ApplicationUser();
            ViewBag.JobTitleList = context.JobTitles.ToList();
            ViewBag.DepartmentList = context.Departments.ToList();
            ViewBag.CostCenterList = context.CostCenters.ToList();
            ViewBag.NationalityList = context.Nationalities.ToList();
            ViewBag.LocationList = context.Locations.ToList();
            ViewBag.LocationGroupList = context.LocationGroups.ToList();
            ViewBag.EmployerTypeList = context.employerTypes.ToList();
            ViewBag.VendorList = context.vendors.ToList();
            ViewBag.JobTitleList = context.JobTitles.ToList();
            ViewBag.GenderList = context.Genders.ToList();
            ViewBag.EthincityList = context.Ethnicities.ToList();
            ViewBag.ReligionList = context.Religions.ToList();
            ViewBag.PayScaleTypeList = context.PayScaleTypes.ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.Status = "Add";
            return View("editUser", new UserListViewModel { users = gp.GetAll<ApplicationUser>(user.EmployeeId, pageIndex, pageSize) });


        }

        [HttpPost]
        public ActionResult UserList(ApplicationUser user)
        {
           
            ApplicationDbContext context = new ApplicationDbContext();
            if (context.Users.Where(m => m.EmployeeId == user.EmployeeId).SingleOrDefault() != null)
            {
                TempData["Success"] = "Employee Id Already Exists";
            }
            else
            {
                user.UserName = Guid.NewGuid().ToString("n").Substring(0, 8) + "@gmail.com";
                context.Users.Add(user);
                context.SaveChanges();
                TempData["Success"] = "User Added Successfully";
            }
            return RedirectToAction("UserList", new { page = 1 });
        }

        [HttpGet]
        public ActionResult Edit(String id, int? page)
        {
            

            ApplicationDbContext context = new ApplicationDbContext();
            ApplicationUser user = new ApplicationUser();
            ViewBag.JobTitleList = context.JobTitles.ToList();
            ViewBag.DepartmentList = context.Departments.ToList();
            ViewBag.CostCenterList = context.CostCenters.ToList();
            ViewBag.NationalityList = context.Nationalities.ToList();
            ViewBag.LocationList = context.Locations.ToList();
            ViewBag.LocationGroupList = context.LocationGroups.ToList();
            ViewBag.EmployerTypeList = context.employerTypes.ToList();
            ViewBag.VendorList = context.vendors.ToList();
            ViewBag.JobTitleList = context.JobTitles.ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.Status = "Update";
            return View("editUser", new UserListViewModel { user = context.Users.Find(id), users = gp.GetAll<ApplicationUser>(user.EmployeeId, pageIndex, pageSize) });
        }

        [HttpPost]
        public ActionResult Edit(ApplicationUser user)
        {

            String page =  Request.Params["page"];
           
            ApplicationDbContext context = new ApplicationDbContext();


            ApplicationUser appuser = context.Users.Find(user.Id);
            appuser.EmployeeId = user.EmployeeId;
            appuser.FirstName = user.FirstName;
            appuser.MiddleName = user.MiddleName;
            appuser.LastName = user.LastName;
            appuser.JobTitleId = user.JobTitleId;
            appuser.JobTitle = null;
            appuser.DepartmentId = user.DepartmentId;
            appuser.Department = null;
            appuser.CostCenterId = user.CostCenterId;
            appuser.CostCenter = null;
            appuser.NationalityId = user.NationalityId;
            appuser.Nationality = null;
            appuser.LocationId = user.LocationId;
            appuser.Location = null;
            appuser.LocationGroupId = user.LocationGroupId;
            appuser.LocationGroup = null;
            appuser.EmployerTypeId = user.EmployerTypeId;
            appuser.EmployerType = null;
            appuser.VendorId = user.VendorId;
            appuser.Vendor = null;
            appuser.bussinessPhoneNumber = user.bussinessPhoneNumber;
            appuser.bussinessEmail = user.bussinessEmail;

            context.Entry(appuser).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
            TempData["Success"] = "User Updated Successfully";
            return RedirectToAction("Edit", new { id = user.Id , page});

        }

        [HttpPost]
        public ActionResult searchByEmployeeId() {
            ApplicationDbContext context = new ApplicationDbContext();
            ApplicationUser user = new ApplicationUser();
            int empid =Convert.ToInt32( Request.Params["employeeId"]);
            int? page = 1;
            ViewBag.JobTitleList = context.JobTitles.ToList();
            ViewBag.DepartmentList = context.Departments.ToList();
            ViewBag.CostCenterList = context.CostCenters.ToList();
            ViewBag.NationalityList = context.Nationalities.ToList();
            ViewBag.LocationList = context.Locations.ToList();
            ViewBag.LocationGroupList = context.LocationGroups.ToList();
            ViewBag.EmployerTypeList = context.employerTypes.ToList();
            ViewBag.VendorList = context.vendors.ToList();
            ViewBag.JobTitleList = context.JobTitles.ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.Status = "Update";
            return View("editUser", new UserListViewModel { users = context.Users.Where(m=>m.EmployeeId == empid).OrderBy(m=>m.EmployeeId).ToPagedList(pageIndex, pageSize)});
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpGet]
        public ViewResult ListUser(int? page) {
            ApplicationUser user=new ApplicationUser();
            ApplicationDbContext db = new ApplicationDbContext();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            return View(gp.GetAll<ApplicationUser>(user.EmployeeId, pageIndex, pageSize));
        }

        /****** Get Ticket Counts********/
        public void TicketCounts()
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
        }

        public ViewResult charts() {

            return View("Charts");
        }

        public string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }


   
}