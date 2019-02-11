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
            Category c = new Category();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            @ViewBag.Status = "Add";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            gp.GetAll<Category>(c.Id,pageIndex,pageSize);
            return View("CreateList",new CategoryViewModel { Categories = gp.GetAll<Category>(c.Id, pageIndex, pageSize) });
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Category category)
        {
             int pageSize = 10;
            int pageIndex = 1;
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category has been Added Successfully";
                return RedirectToAction("Create");
            }

            @ViewBag.Status = "Add";
            TempData["Fail"] = "Category is not created,Enter valid Information";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return View("CreateList", new CategoryViewModel { Categories = gp.GetAll<Category>(category.Id,pageIndex,pageSize)});
        }
        // GET: Categories/Edit/5
        public ActionResult Edit(int? id,int? page)
        {
            int pageSize = 10;
            int pageIndex = 1;
            TempData["page"] = page;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
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
            return View("CreateList", new CategoryViewModel {Category = category, Categories = gp.GetAll<Category>(category.Id, pageIndex, pageSize) });
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            int? page =Convert.ToInt32 (Request.Form["pagee"]);
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Category has been Updated Successfully";
                return RedirectToAction("Edit",new { id=category.Id,page=page});
            }
            ViewBag.Status = "Update";
           TempData["Fail"] = "Category is not Updated,Enter valid Information";
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.FeedBackTypeList = db.FeedbackTypes.ToList();
            return RedirectToAction("Edit", new { id = category.Id, page = page });
        
    }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            int pageSize = 10;
            int pageIndex = 1;
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
            return View("CreateList", new CategoryViewModel { Category = category, Categories = gp.GetAll<Category>(category.Id, pageIndex, pageSize) });
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Category is deleted Successfully";
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
