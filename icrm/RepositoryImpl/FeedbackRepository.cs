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

        public IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize)
        {
            return db.Feedbacks.Where(m=>m.status=="Open" && m.departmentID !=null && m.response ==null).OrderByDescending(m => m.user.Id).ToPagedList(pageIndex, pageSize);

        }

        public IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2) {

            
            IEnumerable<Feedback> feedbacks = db.Feedbacks.ToList();
            var query = from f in feedbacks
                        where (f.createDate >= d1 && f.createDate <= d2)
                        select f;
            return query.ToList();
                        

        }
        public IPagedList<Feedback> search(string d1, string d2, string status, string id,int pageIndex, int pageSize)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";
         
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = status;


            var param4 = new SqlParameter();
            param4.ParameterName = "@id";
            param4.SqlDbType = SqlDbType.VarChar;
            param4.SqlValue = id;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("search @D1,@D2,@Status,@id", param1, param2, param3, param4).ToList();
            foreach (var r in result) {
                feedlist.Add(r);
            }
            
            return feedlist.ToPagedList(pageIndex,pageSize);
        }

        public IEnumerable<Feedback> getAll()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToList();

        }

        public IEnumerable<Feedback> getAllOpen()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m=>m.status=="Open").ToList();

        }
        public IPagedList<Feedback> getAllOpenWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.userId== usrid && m.status=="Open" && m.departmentID != null && m.response==null).ToPagedList(pageIndex, pageSize);
        }

        public IPagedList<Feedback> getAllRespondedWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.userId == usrid  && m.departmentID != null && m.response != null).ToPagedList(pageIndex, pageSize);
        }

        public IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == null && m.response==null && m.status=="Open").ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == "Resolved").ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID != null && m.response != null && m.status == "Open").ToPagedList(pageIndex, pageSize);
        }

        public IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == "Closed").ToPagedList(pageIndex, pageSize);
        }
        public IEnumerable<Feedback> getAllClosed()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == "Closed").ToList();
        }
        public IEnumerable<Feedback> getAllResolved()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == "Resolved").ToList();
        }

        public IPagedList<Feedback> searchHR(string d1, string d2, string status, int pageIndex, int pageSize)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = status;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("searchHR @D1,@D2,@Status", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToPagedList(pageIndex, pageSize);
        }
    }
}