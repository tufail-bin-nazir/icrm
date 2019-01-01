using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using icrm.Models;
using PagedList.Mvc;
using PagedList;

namespace icrm.RepositoryInterface
{
    interface IFeedback : IDisposable
    {
        void Save(Feedback feedback);
        Feedback Find(string id);
        IPagedList<Feedback> Pagination(int pageIndex, int pageSize,string userId);
        IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize);
        IPagedList<Feedback> search(string d1,string d2,string status,string id,int pageIndex, int pageSize);
        IEnumerable<Feedback> getAll();
        IPagedList<Feedback> getAllOpenWithDepartment(string id, int pageIndex, int pageSize);
        IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRespondedWithDepartment(string id, int pageIndex, int pageSize);
        IEnumerable<Feedback> getAllOpen();
        IEnumerable<Feedback> getAllClosed();
        IEnumerable<Feedback> getAllResolved();
        IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2);
        IPagedList<Feedback> searchHR(string v1, string v2, string status, int pageIndex, int pageSize);
    }
}
