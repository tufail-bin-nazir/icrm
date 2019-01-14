using icrm.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace icrm.WebApi
{
    public class RegisterController : ApiController
    {
      

        private ApplicationUserManager _userManager;
        
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> User(RegisterViewModel model) {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var user = new ApplicationUser { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName, Email = model.Email, PhoneNumber = model.PhoneNumber,EmployeeId=model.EmployeeId};
            var result = await UserManager.CreateAsync(user, model.Password);
            UserManager.AddToRole(user.Id, roleManager.FindByName("User").Name);

            if (result != null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
            


        }
    }
}
