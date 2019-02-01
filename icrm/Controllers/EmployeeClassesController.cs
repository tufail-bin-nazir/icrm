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
    public class EmployeeClassesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EmployeeClasses
        public ActionResult Index()
        {
            return View(db.employeeClasses.ToList());
        }

        // GET: EmployeeClasses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeClass employeeClass = db.employeeClasses.Find(id);
            if (employeeClass == null)
            {
                return HttpNotFound();
            }
            return View(employeeClass);
        }

        // GET: EmployeeClasses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmployeeClasses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] EmployeeClass employeeClass)
        {
            if (ModelState.IsValid)
            {
                db.employeeClasses.Add(employeeClass);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(employeeClass);
        }

        // GET: EmployeeClasses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeClass employeeClass = db.employeeClasses.Find(id);
            if (employeeClass == null)
            {
                return HttpNotFound();
            }
            return View(employeeClass);
        }

        // POST: EmployeeClasses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] EmployeeClass employeeClass)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employeeClass).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employeeClass);
        }

        // GET: EmployeeClasses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeClass employeeClass = db.employeeClasses.Find(id);
            if (employeeClass == null)
            {
                return HttpNotFound();
            }
            return View(employeeClass);
        }

        // POST: EmployeeClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmployeeClass employeeClass = db.employeeClasses.Find(id);
            db.employeeClasses.Remove(employeeClass);
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
