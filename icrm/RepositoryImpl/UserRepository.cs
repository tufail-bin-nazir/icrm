using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using DocumentFormat.OpenXml.Office2010.Excel;
using icrm.Models;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Constants = icrm.Models.Constants;

namespace icrm.RepositoryImpl
{
    public class UserRepository : UserInterface,IDisposable
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private RoleManager<IdentityRole> roleManager;
        public UserRepository()
        {
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ApplicationUser> GetAllAvailableUsers(String role)
        {
            var userRole = roleManager.FindByName(role).Users.FirstOrDefault();
            return db.Users.Where(u => u.available == true && u.Roles.Any(r => r.RoleId == userRole.RoleId)).ToList();
        }


        public ApplicationUser findUserOnId(string id)
        {
            return db.Users.Find(id);
        }

        public bool Update(ApplicationUser user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                return false;
            }
        }


        public List<string> DeviceIds()
        {
            var roleId = roleManager.FindByName("User").Id;
           return  db.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId)).Select(u=>u.DeviceCode).ToList();
        }
    }

}