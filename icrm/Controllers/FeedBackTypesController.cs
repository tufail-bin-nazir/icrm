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
    public class FeedBackTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FeedBackTypes
        public ActionResult Index()
        {
            return View(db.FeedbackTypes.ToList());
        }

        // GET: FeedBackTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBackType feedBackType = db.FeedbackTypes.Find(id);
            if (feedBackType == null)
            {
                return HttpNotFound();
            }
            return View(feedBackType);
        }

        // GET: FeedBackTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FeedBackTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] FeedBackType feedBackType)
        {
            if (ModelState.IsValid)
            {
                db.FeedbackTypes.Add(feedBackType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(feedBackType);
        }

        // GET: FeedBackTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBackType feedBackType = db.FeedbackTypes.Find(id);
            if (feedBackType == null)
            {
                return HttpNotFound();
            }
            return View(feedBackType);
        }

        // POST: FeedBackTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] FeedBackType feedBackType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(feedBackType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(feedBackType);
        }

        // GET: FeedBackTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBackType feedBackType = db.FeedbackTypes.Find(id);
            if (feedBackType == null)
            {
                return HttpNotFound();
            }
            return View(feedBackType);
        }

        // POST: FeedBackTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FeedBackType feedBackType = db.FeedbackTypes.Find(id);
            db.FeedbackTypes.Remove(feedBackType);
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
