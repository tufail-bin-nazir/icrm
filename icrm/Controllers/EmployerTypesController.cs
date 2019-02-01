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
    public class EmployerTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EmployerTypes
        public ActionResult Index()
        {
            return View(db.employerTypes.ToList());
        }

        // GET: EmployerTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployerType employerType = db.employerTypes.Find(id);
            if (employerType == null)
            {
                return HttpNotFound();
            }
            return View(employerType);
        }

        // GET: EmployerTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmployerTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] EmployerType employerType)
        {
            if (ModelState.IsValid)
            {
                db.employerTypes.Add(employerType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(employerType);
        }

        // GET: EmployerTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployerType employerType = db.employerTypes.Find(id);
            if (employerType == null)
            {
                return HttpNotFound();
            }
            return View(employerType);
        }

        // POST: EmployerTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] EmployerType employerType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employerType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employerType);
        }

        // GET: EmployerTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployerType employerType = db.employerTypes.Find(id);
            if (employerType == null)
            {
                return HttpNotFound();
            }
            return View(employerType);
        }

        // POST: EmployerTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmployerType employerType = db.employerTypes.Find(id);
            db.employerTypes.Remove(employerType);
            db.SaveChanges();
            return RedirectToAction("Index");
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
