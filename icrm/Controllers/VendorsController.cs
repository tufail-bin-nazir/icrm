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
    public class VendorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Vendors
        public ActionResult Index()
        {
            return View(db.vendors.ToList());
        }

        // GET: Vendors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            return View(vendor);
        }

        // GET: Vendors/Create
        public ActionResult Create()
        {
            ViewBag.Status = "Add";
            return View("CreateList", new VendorViewModel { Vendors = db.vendors.ToList()});
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                db.vendors.Add(vendor);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Status = "Add";
            return View("CreateList", new VendorViewModel { Vendors = db.vendors.ToList() });
        }

        // GET: Vendors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            return View("CreateList", new VendorViewModel {Vendor = vendor, Vendors = db.vendors.ToList() });
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vendor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.Status = "Update";
            return View("CreateList", new VendorViewModel { Vendors = db.vendors.ToList() });
        }

        // GET: Vendors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            return View("CreateList", new VendorViewModel {Vendor = vendor, Vendors = db.vendors.ToList() });
        }

        // POST: Vendors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vendor vendor = db.vendors.Find(id);
            db.vendors.Remove(vendor);
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
