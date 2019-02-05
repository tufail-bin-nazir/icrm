using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
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
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Status = "Add";
            return View("CreateList", new EscalationUserViewModel {  EscalationUsers = db.EscalationUsers.ToList()});
        }

        // POST: EscalationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EscalationUser escalationUser)
        {
            Debug.WriteLine(ModelState.IsValid + "0-----------------------");
            if (ModelState.IsValid)
            {
                db.EscalationUsers.Add(escalationUser);
                db.SaveChanges();
                foreach (int cid in escalationUser.CategoriesIds) {
                    Category c = db.Categories.Find(cid);
                    c.EscalationUserId = escalationUser.Id;
                    db.Entry(c).State = EntityState.Modified;
                    db.SaveChanges();

                }
              
                return RedirectToAction("Create");
            }
            
            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Status = "Add";
            return View("CreateList", new EscalationUserViewModel { EscalationUsers = db.EscalationUsers.ToList() });
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

            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Status = "Update";
            return View("CreateList", new EscalationUserViewModel {EscalationUser = escalationUser, EscalationUsers = db.EscalationUsers.ToList() });
        }

        // POST: EscalationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( EscalationUser escalationUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(escalationUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Status = "Update";
            return View("CreateList", new EscalationUserViewModel { EscalationUsers = db.EscalationUsers.ToList() });
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

            ViewBag.DepartmentList = db.Departments.ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Status = "Delete";
            return View("CreateList", new EscalationUserViewModel {EscalationUser = escalationUser, EscalationUsers = db.EscalationUsers.ToList() });
        }

        // POST: EscalationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            db.EscalationUsers.Remove(escalationUser);
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
