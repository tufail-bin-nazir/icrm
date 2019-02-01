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
    public class EscalationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EscalationUsers
        public ActionResult Index()
        {
            return View(db.EscalationUsers.ToList());
        }

        // GET: EscalationUsers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            if (escalationUser == null)
            {
                return HttpNotFound();
            }
            return View(escalationUser);
        }

        // GET: EscalationUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EscalationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DepartmentId")] EscalationUser escalationUser)
        {
            if (ModelState.IsValid)
            {
                db.EscalationUsers.Add(escalationUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(escalationUser);
        }

        // GET: EscalationUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            if (escalationUser == null)
            {
                return HttpNotFound();
            }
            return View(escalationUser);
        }

        // POST: EscalationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DepartmentId")] EscalationUser escalationUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(escalationUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(escalationUser);
        }

        // GET: EscalationUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            if (escalationUser == null)
            {
                return HttpNotFound();
            }
            return View(escalationUser);
        }

        // POST: EscalationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            db.EscalationUsers.Remove(escalationUser);
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
