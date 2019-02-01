using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using icrm.Models;

namespace icrm.Controllers
{
    [Authorize(Roles ="Admin")]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private GenericPagination<Category> gp = new GenericPagination<Category>();
        // GET: Categories
        public ActionResult Index(int? page)
        {
            
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();            
            return View(db.Categories.ToList());

        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create(int? page)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            @ViewBag.Status = "Add";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList",new CategoryViewModel { Categories = gp.GetAll<Category>(pageIndex, pageSize) });
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            @ViewBag.Status = "Add";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList", new CategoryViewModel { Categories = db.Categories.ToList() });
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList", new CategoryViewModel {Category = category, Categories = db.Categories.ToList() });
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.Status = "Update";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList", new CategoryViewModel {  Categories = db.Categories.ToList() });
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            @ViewBag.Status = "Delete";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList", new CategoryViewModel { Category = category, Categories = db.Categories.ToList() });
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
