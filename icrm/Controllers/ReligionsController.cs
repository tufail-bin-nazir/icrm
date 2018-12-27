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
    [Authorize(Roles = "Admin")]
    public class ReligionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Religions
        public ActionResult Index()
        {
            return View(db.Religions.ToList());
        }

        // GET: Religions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Religion religion = db.Religions.Find(id);
            if (religion == null)
            {
                return HttpNotFound();
            }
            return View(religion);
        }

        // GET: Religions/Create
        public ActionResult Create()
        {
            ViewBag.Status = "Add";
            return View("CreateList", new ReligionViewModel { Religions = db.Religions.ToList()});
        }

        // POST: Religions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Religion religion)
        {
            if (ModelState.IsValid)
            {
                db.Religions.Add(religion);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Status = "Add";
            return View("CreateList", new ReligionViewModel { Religions = db.Religions.ToList() });
        }

        // GET: Religions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Religion religion = db.Religions.Find(id);
            if (religion == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            return View("CreateList", new ReligionViewModel {Religion = religion, Religions = db.Religions.ToList() });
        }

        // POST: Religions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Religion religion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(religion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.Status = "Update";
            return View("CreateList", new ReligionViewModel { Religions = db.Religions.ToList() });
        }

        // GET: Religions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Religion religion = db.Religions.Find(id);
            if (religion == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            return View("CreateList", new ReligionViewModel {Religion = religion, Religions = db.Religions.ToList() });
        }

        // POST: Religions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Religion religion = db.Religions.Find(id);
            db.Religions.Remove(religion);
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
