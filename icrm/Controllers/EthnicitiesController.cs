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
    public class EthnicitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Ethnicities
        public ActionResult Index()
        {
            return View(db.Ethnicities.ToList());
        }

        // GET: Ethnicities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ethnicity ethnicity = db.Ethnicities.Find(id);
            if (ethnicity == null)
            {
                return HttpNotFound();
            }
            return View(ethnicity);
        }

        // GET: Ethnicities/Create
        public ActionResult Create()
        {
            ViewBag.Status = "Add";
            return View("CreateList", new EthnicitesViewModel {Ethnicities = db.Ethnicities.ToList() });
        }

        // POST: Ethnicities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Ethnicity ethnicity)
        {
            if (ModelState.IsValid)
            {
                db.Ethnicities.Add(ethnicity);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Status = "Add";
            return View("CreateList", new EthnicitesViewModel { Ethnicities = db.Ethnicities.ToList() });
        }

        // GET: Ethnicities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ethnicity ethnicity = db.Ethnicities.Find(id);
            if (ethnicity == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            return View("CreateList", new EthnicitesViewModel {Ethnicity = ethnicity, Ethnicities = db.Ethnicities.ToList() });
        }

        // POST: Ethnicities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Ethnicity ethnicity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ethnicity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.Status = "Update";
            return View("CreateList", new EthnicitesViewModel { Ethnicities = db.Ethnicities.ToList() });
        }

        // GET: Ethnicities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ethnicity ethnicity = db.Ethnicities.Find(id);
            if (ethnicity == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            return View("CreateList", new EthnicitesViewModel {Ethnicity = ethnicity, Ethnicities = db.Ethnicities.ToList() });
        }

        // POST: Ethnicities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ethnicity ethnicity = db.Ethnicities.Find(id);
            db.Ethnicities.Remove(ethnicity);
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
