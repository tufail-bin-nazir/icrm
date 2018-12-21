using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using icrm.Models;
using icrm.RepositoryInterface;
using PagedList.Mvc;
using PagedList;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;


namespace icrm.RepositoryImpl
{
    public class FeedbackRepository : IFeedback
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public void Save(Feedback feedback)
        {

            db.Feedbacks.Add(feedback);
            db.SaveChanges();

        }

        public Feedback Find(int? id)
        {
            return db.Feedbacks.Find(id);
        }


        public IPagedList<Feedback> Pagination(int pageIndex, int pageSize, string userId)
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m => m.user.Id == userId).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAll(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> search(string search, int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(x => x.user.Id).Where(x => x.name.StartsWith(search) || search == null).ToPagedList(pageIndex, pageSize);
        }

        public IEnumerable<Feedback> getAll()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToList();

        }
    }
}