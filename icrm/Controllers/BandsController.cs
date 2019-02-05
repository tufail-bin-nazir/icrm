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
    public class BandsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bands
        public ActionResult Index()
        {
            return View(db.bands.ToList());
        }

        // GET: Bands/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Band band = db.bands.Find(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            return View(band);
        }

        // GET: Bands/Create
        public ActionResult Create()
        {
            ViewBag.Status = "Add";
            return View("CreateList", new BandViewModel { Bands = db.bands.ToList()});
        }

        // POST: Bands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Band band)
        {
            if (ModelState.IsValid)
            {
                db.bands.Add(band);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Status = "Add";
            return View("CreateList", new BandViewModel { Bands = db.bands.ToList() });
        }

        // GET: Bands/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Band band = db.bands.Find(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            return View("CreateList", new BandViewModel {Band = band, Bands = db.bands.ToList() });
        }

        // POST: Bands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Band band)
        {
            if (ModelState.IsValid)
            {
                db.Entry(band).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.Status = "Update";
            return View("CreateList", new BandViewModel { Bands = db.bands.ToList() });
        }

        // GET: Bands/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Band band = db.bands.Find(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            return View("CreateList", new BandViewModel {Band = band, Bands = db.bands.ToList() });
        }

        // POST: Bands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Band band = db.bands.Find(id);
            db.bands.Remove(band);
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
