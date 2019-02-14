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
using System.Data.Entity;

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
            var result = db.Feedbacks.SqlQuery("search @D1,@D2,@Status,@id", param1,param2,param3,param4).ToList();
            foreach (var r in result) {
                feedlist.Add(r);
            }
            
            return feedlist.ToPagedList(pageIndex,pageSize);
        }
      
        public IEnumerable<Feedback> getAll()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToList();

        }

        public IPagedList<Feedback> getAll(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).ToPagedList(pageIndex,pageSize);

        }

        public IEnumerable<Feedback> getAllOpen()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m=>m.checkStatus== Models.Constants.OPEN).ToList();

        }

        public IEnumerable<Feedback> getAllOpenMobile()
        {
            return db.Feedbacks.OrderByDescending(m => m.user.Id).Where(m => m.checkStatus == Models.Constants.OPEN).ToList();

        }



        public IPagedList<Feedback> getAllOpenWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            //ApplicationUser user = db.Users.Find(usrid);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.ASSIGNED;

            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllOpenWithDepart @depID,@status", param1,param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}

           // return feedlist.ToPagedList(pageIndex, pageSize);

            return db.Feedbacks.OrderByDescending(m=>m.id).Where(m=>m.departUserId==usrid && m.checkStatus==Constants.ASSIGNED).ToPagedList(pageIndex, pageSize);

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

        }

        public IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.departmentID == null  && m.checkStatus==Constants.OPEN).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == Constants.RESOLVED).ToPagedList(pageIndex, pageSize);

        }
        public IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Constants.OPEN).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize)
        {
            

            return db.Feedbacks.OrderByDescending(m=>m.id).Where(m=>m.checkStatus==Constants.RESPONDED).ToPagedList(pageIndex, pageSize);

        }

        public IEnumerable<Feedback> GetAllResponded()
        {


            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Constants.RESPONDED).ToList();

        }

        public IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.CLOSED && m.type.name == Constants.Complaints).ToPagedList(pageIndex, pageSize);
        }
        public IEnumerable<Feedback> getAllClosed()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.CLOSED && m.type.name==Constants.Complaints).ToList();
        }
        public IEnumerable<Feedback> getAllResolved()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m =>  m.status == Constants.RESOLVED).ToList();
        }

        public IEnumerable<Feedback> GetAllAssigned()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Constants.ASSIGNED).ToList();
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

        public List<Category> getCategories(int deptId,int type)
        {
          return  db.Categories.Where(m => m.DepartmentId == deptId && m.FeedBackTypeId==type).ToList();
        }

        public List<SubCategory> getSubCategories(int categoryId,int type)
        {
            return db.SubCategories.Where(m => m.CategoryId == categoryId && m.FeedBackTypeId==type).ToList();
        }

        public List<string> getEmails()
        {
            List<string> emailList = new List<string>();
             List<ApplicationUser> u= db.Users.OrderBy(m => m.Id).Where(m=>m.forwarDeptEmailCCStatus==true).ToList();

            
            foreach (ApplicationUser uu in u) {
                emailList.Add(uu.bussinessEmail);
                System.Diagnostics.Debug.WriteLine(uu.Email+"---------------------jjjj");
            }
            return emailList;
        }

        public IPagedList<Feedback> getListBasedOnType(int pageIndex, int pageSize,string typeId)
        {
            return db.Feedbacks.OrderBy(m=>m.id).Where(m => m.type.name==typeId).ToPagedList(pageIndex,pageSize);
        }


        public IEnumerable<Feedback> GetListBasedOnType(string type)
        {
            return db.Feedbacks.OrderBy(m => m.id).Where(m => m.type.name == type).ToList();
        }
        public List<Department> getDepartmentsOnType(string fORWARD)
        {
            return db.Departments.OrderBy(m => m.name).Where(m => m.type == fORWARD).ToList();
        }

        public List<Priority> getPriorties()
        {
            return db.Priorities.OrderBy(m => m.priorityId).ToList();
        }

        public List<FeedBackType> getFeedbackTypes()
        {
            return db.FeedbackTypes.OrderBy(m=>m.name).ToList();
        }

        public ApplicationUser getEmpDetails(string id)
        {
            return db.Users.Where(u => u.Id == id).FirstOrDefault();
        }

        public IEnumerable<Feedback> getAllByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;


            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepart @depID", param1).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}

            return db.Feedbacks.Where(m => m.departUserId == id).ToList();

        }

        public IEnumerable<Feedback> getAllOpenByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.OPEN;



            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}
            //return feedlist;

            return db.Feedbacks.Where(m => m.departUserId == id && m.status == Constants.OPEN).ToList();
        }

        public IEnumerable<Feedback> getAllClosedByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@Status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.CLOSED;



            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@Status", param1, param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}
            //return feedlist;

            return db.Feedbacks.Where(m=>m.departUserId==id && m.status==Constants.CLOSED).ToList();

        }

        public IEnumerable<Feedback> getAllResolvedByDept(string id)
        {

            // ApplicationUser user = db.Users.Find(id);
            // var param1 = new SqlParameter();
            // param1.ParameterName = "@depID";
            // param1.SqlDbType = SqlDbType.VarChar;
            // param1.SqlValue = user.DepartmentId;

            // var param2 = new SqlParameter();
            // param2.ParameterName = "@status";
            // param2.SqlDbType = SqlDbType.VarChar;
            // param2.SqlValue = Constants.RESOLVED;

            ///* var param3 = new SqlParameter();
            // param3.ParameterName = "@CommentedByID";
            // param3.SqlDbType = SqlDbType.VarChar;
            // param3.SqlValue = id;*/

            // List<Feedback> feedlist = new List<Feedback>();
            // var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            // foreach (var r in result)
            // {
            //     feedlist.Add(r);
            // }
            // return feedlist;
            return db.Feedbacks.Where(m=>m.departUserId==id && m.status==Constants.RESOLVED).ToList();
        }

        public IPagedList<Feedback> getAllOpenByDept(string v, int pageIndex, int pageSize)
        {
            
            ApplicationUser user = db.Users.Find(v);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@status";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = Constants.OPEN;

          /*  var param3 = new SqlParameter();
            param3.ParameterName = "@CommentedByID";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = v;*/

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist.ToPagedList(pageIndex, pageSize);
        }

        public ApplicationUser getEscalationUser(int? departmentId, int? categoryId)
        {
            var query = from e in db.EscalationUsers
                        join Category in db.Categories on e.Id equals Category.EscalationUserId
                        where e.DepartmentId == departmentId && Category.Id == categoryId
                        select e;
                  EscalationUser user=(EscalationUser)query.FirstOrDefault();
                  return db.Users.Find(user.firstEscalationUserId);
        }

       
    }
}