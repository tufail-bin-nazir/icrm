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
        Feedback Find(int? id);
        IPagedList<Feedback> Pagination(int pageIndex, int pageSize,string userId);
        IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize);
        IPagedList<Feedback> search(DateTime d1,DateTime d2,int pageIndex, int pageSize);
        IEnumerable<Feedback> getAll();
        IPagedList<Feedback> getAllWithDepartment(string id, int pageIndex, int pageSize);
        IPagedList<Feedback> getAllOpenWithDepart(int pageIndex, int pageSize);
    }
}
