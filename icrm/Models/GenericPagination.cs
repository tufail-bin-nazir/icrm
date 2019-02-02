using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class GenericPagination<TEntity>
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public IPagedList<TEntity> GetAll<TEntity>(int pageIndex,int pageSize)
         where TEntity : class 
        {
            return db.Set<TEntity>().ToPagedList(pageIndex,pageSize);
        }
    }
}