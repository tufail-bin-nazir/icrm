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
using System.Data.SqlClient;
using System.Data;

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

        public IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize)
        {
            return db.Feedbacks.Where(m=>m.status=="Open").OrderByDescending(m => m.user.Id).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> search(DateTime d1, DateTime d2,int pageIndex, int pageSize)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@Date";
           
            param1.SqlDbType = SqlDbType.DateTime;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@Date2";
            param2.SqlDbType = SqlDbType.DateTime;
            param2.SqlValue = d2;

            var result = db.Feedbacks.SqlQuery("searchcriteria @Date1,@Date2", param1, param2).ToPagedList(pageIndex, pageSize);

            return result;
          //  return db.Feedbacks.OrderByDescending(x => x.user.Id).Where(x => x.title.StartsWith(search) || search == null).ToPagedList(pageIndex, pageSize);
        }

        public IEnumerable<Feedback> getAll()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToList();

        }

        public IPagedList<Feedback> getAllWithDepartment(string departId, int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID== departId).ToPagedList(pageIndex, pageSize);
        }

        public IPagedList<Feedback> getAllOpenWithDepart(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}