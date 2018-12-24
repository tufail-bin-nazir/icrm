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

    public class CostCentersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CostCenters
        public ActionResult Index()
        {
            return View(db.CostCenters.ToList());
        }

        // GET: CostCenters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CostCenter costCenter = db.CostCenters.Find(id);
            if (costCenter == null)
            {
                return HttpNotFound();
            }
            return View(costCenter);
        }

        // GET: CostCenters/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CostCenters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,code,name")] CostCenter costCenter)
        {
            if (ModelState.IsValid)
            {
                db.CostCenters.Add(costCenter);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(costCenter);
        }

        // GET: CostCenters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CostCenter costCenter = db.CostCenters.Find(id);
            if (costCenter == null)
            {
                return HttpNotFound();
            }
            return View(costCenter);
        }

        // POST: CostCenters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,code,name")] CostCenter costCenter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(costCenter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(costCenter);
        }

        // GET: CostCenters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CostCenter costCenter = db.CostCenters.Find(id);
            if (costCenter == null)
            {
                return HttpNotFound();
            }
            return View(costCenter);
        }

        // POST: CostCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CostCenter costCenter = db.CostCenters.Find(id);
            db.CostCenters.Remove(costCenter);
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
