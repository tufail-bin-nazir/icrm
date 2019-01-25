using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace icrm.WebApi
{
    public class RegisterController : ApiController
    {


        private ApplicationUserManager _userManager;

        ApplicationDbContext db = new ApplicationDbContext();
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager < ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> User(RegisterViewModel model)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationUser user = db.Users.Where(e => e.EmployeeId == model.EmployeeId).SingleOrDefault();
            if (user == null)
            {
                return BadRequest(" NO User Found");

            }
            else
            {
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = Convert.ToString(model.EmployeeId);
                user.PasswordHash = HashPassword(model.Password);
                user.SecurityStamp = Guid.NewGuid().ToString("D");
                db.Users.Add(user);
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                UserManager.AddToRole(user.Id, roleManager.FindByName("User").Name);
                return Ok();

            }
        }

        public string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }
}