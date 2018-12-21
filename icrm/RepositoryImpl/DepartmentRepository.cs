using icrm.Models;
using icrm.RepositoryInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.RepositoryImpl 
{
    public class DepartmentRepository : IDepartment
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public List<Department> getAll()
        {
            return db.Departments.OrderByDescending(m => m.ID).ToList();
        }
    }
}