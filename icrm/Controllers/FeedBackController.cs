﻿using icrm.Models;
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
using System.Data.SqlClient;
using System.Data;

namespace icrm.Controllers
{
    [Authorize]
    public class FeedBackController : Controller
    {
        private IFeedback feedInterface;
        private IDepartment departInterface;
        private ApplicationDbContext db = new ApplicationDbContext();

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
        public ActionResult dashboard(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            
            List<Department> departments = departInterface.getAll();

            IPagedList<Feedback> feedbackList = feedInterface.Pagination(pageIndex, pageSize, user.Id);
            //  var Bcm= new Tuple<IPagedList<Feedback>, FeedbackDTO>(feedbackList, new FeedbackDTO());
            return View(feedbackList);
            
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
        public ActionResult add([Bind(Include = "id,title,description,userId,file")] Feedback feedback,HttpPostedFileBase file)
        {
            var fileSize = file.ContentLength;
            if (fileSize > 10 * 1024 * 1024)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                ViewData["user"] = user;
                TempData["Message"] = "File Size Limit Exceeds";
                return View("add", feedback);
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0) {
                    feedback.attachment=Server.MapPath("~/Images/" + file.FileName);
                    file.SaveAs(feedback.attachment);
                }
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

        [HttpPost]
        [Route("searchfeedback")]
        public ActionResult search(int? page)
        {
          //  DateTime d1 = Convert.ToDateTime(Request["date1"]);
          //  DateTime d2= Convert.ToDateTime(Request["date2"]);

            DateTime d1= Convert.ToDateTime("2018 - 12 - 22 00:46:42.087");
            DateTime d2 = Convert.ToDateTime("2018 - 12 - 22 00:46:42.087");
            Debug.WriteLine(d1 + "00000" + d2 + "saaaaaaaaath");
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            IPagedList<Feedback> feedbacks=feedInterface.search(d1,d2, pageIndex, pageSize);
            return View("list",feedbacks);

        }


        // update for satisfied or not
        [HttpPost]
        [Route("feedbackedit/")]
        public ActionResult edit(Feedback feedback)
        {
           if (ModelState.IsValid)
            {
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();
                
                TempData["Message"] = "Feedback Saved";
                return RedirectToAction("view", new { id=feedback.id});
            }
            else
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                ViewData["user"] = user;
                TempData["Message"] = "Fill feedback Properly";
                return View(feedback);
            }


        }

    }
}