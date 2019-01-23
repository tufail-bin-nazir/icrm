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
using Constants = icrm.Models.Constants;

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

        public Feedback Find(string id)
        {
            return db.Feedbacks.Find(id);
        }


        public IPagedList<Feedback> Pagination(int pageIndex, int pageSize, string userId)
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m => m.user.Id == userId).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize)
        {
            var param = new SqlParameter();
            param.ParameterName = "@status";
            param.SqlDbType = SqlDbType.VarChar;
            param.SqlValue = Constants.ASSIGNED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllAssigned @status",param).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToPagedList(pageIndex, pageSize);
            // return db.Feedbacks.Where(m=>m.status=="Open" && m.departmentID !=null && m.response ==null).OrderByDescending(m => m.user.Id).ToPagedList(pageIndex, pageSize);

        }

        public IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2) {

            
            IEnumerable<Feedback> feedbacks = db.Feedbacks.ToList();
            var query = from f in feedbacks
                        where (f.createDate >= d1 && f.createDate <= d2)
                        select f;
            return query.ToList();
                        

        }
        public IPagedList<Feedback> search(string d1, string d2, string status, int id,int pageIndex, int pageSize)
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
            param4.SqlDbType = SqlDbType.Int;
            param4.SqlValue = id;

            var param5 = new SqlParameter();
            param5.ParameterName = "@checkstatus";
            param5.SqlDbType = SqlDbType.VarChar;
            param5.SqlValue = Constants.RESPONDED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("search @D1,@D2,@Status,@id,@checkstatus", param1,param2,param3,param4,param5).ToList();
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
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m=>m.status==Constants.OPEN).ToList();

        }
        public IPagedList<Feedback> getAllOpenWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            ApplicationUser user = db.Users.Find(usrid);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@status";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = Constants.ASSIGNED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllOpenWithDepart @depID,@status", param1,param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToPagedList(pageIndex, pageSize);            
            // return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID== user.DepartmentId && m.status=="Open" && m.departmentID != null && m.response==null && m.responseById==null).ToPagedList(pageIndex, pageSize);
        }

        public IPagedList<Feedback> getAllRespondedWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            ApplicationUser user = db.Users.Find(usrid);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@CommentedByID";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = usrid;

            var param3 = new SqlParameter();
            param3.ParameterName = "@status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = Constants.RESPONDED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllRespondedWithDepart @depID,@CommentedByID,@status", param1,param2,param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToPagedList(pageIndex, pageSize);

            //  return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == user.DepartmentId && m.departmentID != null && m.response != null && m.responseById==usrid).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize)
        {
            //return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == null && m.response==null && m.status=="Open").ToPagedList(pageIndex, pageSize);
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == null  && m.checkStatus==Constants.OPEN).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == Constants.RESOLVED).ToPagedList(pageIndex, pageSize);

        }
        public IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.OPEN).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize)
        {
            

            return db.Feedbacks.OrderByDescending(m=>m.id).Where(m=>m.checkStatus==Constants.RESPONDED).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.CLOSED).ToPagedList(pageIndex, pageSize);
        }
        public IEnumerable<Feedback> getAllClosed()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.CLOSED).ToList();
        }
        public IEnumerable<Feedback> getAllResolved()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == Constants.RESOLVED).ToList();
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


        public IEnumerable<Feedback> searchHR(string d1, string d2, string status)
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

            return feedlist.ToList();
        }


        public IEnumerable<Feedback> getAllAssigned()
        {
            var param = new SqlParameter();
            param.ParameterName = "@status";
            param.SqlDbType = SqlDbType.VarChar;
            param.SqlValue = Models.Constants.ASSIGNED;
            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllAssigned @status",param).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist;
          //  return db.Feedbacks.Where(m => m.status == "Open" && m.departmentID != null && m.response == null).OrderByDescending(m => m.user.Id).ToList();
        }

        public IEnumerable<Feedback> getAllResponded()
        {
            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllResponded").ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist;
           //return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID != null && m.response != null && m.status == "Open").ToList();
        }

        public IEnumerable<Feedback> OpenWithoutDepart()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == null && m.checkStatus == Constants.OPEN).ToList();
        }

        public IPagedList<Feedback> getAllRejected(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Constants.REJECTED).ToPagedList(pageIndex,pageSize);

        }

      

        public IEnumerable<Feedback> getAllRespondedWithDepartment(string usrid)
        {
            ApplicationUser user = db.Users.Find(usrid);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@CommentedByID";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = usrid;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllRespondedWithDepart @depID,@CommentedByID", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist;

        }

        public List<Comments> getCOmments(string id)
        {
            return db.comments.Where(m => m.feedbackId == id).ToList();
        }
    }
}