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
    public class PayScaleTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PayScaleTypes
        public ActionResult Index()
        {
            return View(db.PayScaleTypes.ToList());
        }

        // GET: PayScaleTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayScaleType payScaleType = db.PayScaleTypes.Find(id);
            if (payScaleType == null)
            {
                return HttpNotFound();
            }
            return View(payScaleType);
        }

        // GET: PayScaleTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PayScaleTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] PayScaleType payScaleType)
        {
            if (ModelState.IsValid)
            {
                db.PayScaleTypes.Add(payScaleType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(payScaleType);
        }

        // GET: PayScaleTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayScaleType payScaleType = db.PayScaleTypes.Find(id);
            if (payScaleType == null)
            {
                return HttpNotFound();
            }
            return View(payScaleType);
        }

        // POST: PayScaleTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] PayScaleType payScaleType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payScaleType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(payScaleType);
        }

        // GET: PayScaleTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayScaleType payScaleType = db.PayScaleTypes.Find(id);
            if (payScaleType == null)
            {
                return HttpNotFound();
            }
            return View(payScaleType);
        }

        // POST: PayScaleTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PayScaleType payScaleType = db.PayScaleTypes.Find(id);
            db.PayScaleTypes.Remove(payScaleType);
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
