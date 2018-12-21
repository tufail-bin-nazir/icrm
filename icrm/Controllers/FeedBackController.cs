using icrm.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using PagedList.Mvc;
using PagedList;
using icrm.RepositoryInterface;
using icrm.RepositoryImpl;

namespace icrm.Controllers
{
    [Authorize]
    public class FeedBackController : Controller
    {
        private IFeedback feedInterface;
        private IDepartment departInterface;

        public FeedBackController() {
            feedInterface = new FeedbackRepository();
            departInterface = new DepartmentRepository();        }
       
        private ApplicationUserManager _userManager;

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


        [Route("feedbackdashboard/")]
        public ActionResult dashboard()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            return View();
        }

        

        [Route("feedbacklist/")]
        public ActionResult list(int? page)
        {
           
            
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            List<Department> departments = departInterface.getAll();

           // ViewBag.departmemts = departments;
            IPagedList<Feedback> feedbackList = feedInterface.Pagination(pageIndex,pageSize,user.Id);
         //  var Bcm= new Tuple<IPagedList<Feedback>, FeedbackDTO>(feedbackList, new FeedbackDTO());
            return View(feedbackList);
        
        }

        [Route("feedback/")]
        public ActionResult add()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());        
            ViewData["user"] = user;
            return View();
        }



        [HttpPost]
        [Route("feedback/")]
        public ActionResult add([Bind(Include = "id,name,email,contactNo,typeOfFeedback,subject,details,userId")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedInterface.Save(feedback);
                TempData["Message"] = "Feedback Saved";
                return RedirectToAction("add");
            }
            else
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                ViewData["user"] = user;
                TempData["Message"] = "Fill feedback Properly";
                return View("add", feedback);
            }
            

        }



        [HttpGet]
        [Route("viewfeedback/{id}")]
        public ActionResult view(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            if (id == null)
            {
                ViewBag.ErrorMsg = "FeedBack not found";
                return RedirectToAction("list");
            }
            else {
                Feedback f = feedInterface.Find(id);
                return View("view", f);
            }

        }

        [HttpGet]
        [Route("searchfeedback")]
        public ActionResult search(string search,int? page)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            IPagedList<Feedback> feedbacks=feedInterface.search(search, pageIndex, pageSize);
            return View("list",feedbacks);

        }

    }
}