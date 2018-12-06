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

namespace icrm.Controllers
{
    [Authorize]
    public class FeedBackController : Controller
    {

        ApplicationDbContext db = new ApplicationDbContext();
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
            int pageSize = 3;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IPagedList<Feedback> feedbackList = null;
          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedbackList = db.Feedbacks.OrderByDescending(m=>m.user.Id).Where(m=>m.user.Id==user.Id).ToPagedList(pageIndex,pageSize);
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
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback f = db.Feedbacks.Find(id);

            

            return View("view", f);

        }


       

    }
}