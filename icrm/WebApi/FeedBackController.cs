using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace icrm.WebApi
{
    [Authorize]
    public class FeedBackController : ApiController
    {
        private IFeedback feedInterface;

        private ApplicationUserManager _userManager;

        public FeedBackController()
        {
            feedInterface = new FeedbackRepository();
        }



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
        public IHttpActionResult PostFeedback(Feedback feedBack) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            feedBack.userId = user.Result.Id;
            feedInterface.Save(feedBack);
            return Ok(feedBack);

        }

        //[HttpGet]
        //public IHttpActionResult GetFeedbacks() {
        //    var query = from f in feedInterface.getAll()
        //                select new { f.email, f.contactNo };
        //    return Ok(query.ToList());

        //}

        //[HttpGet]
        //public IHttpActionResult SearchResult() {
        //    //Created A List Of System Defined Delegates
        //    List<Func<Feedback, bool>> filters = new List<Func<Feedback, bool>>();
        //       filters.Add(p => p.email.ToLower().Contains("tufail@gmail.com"));
        //       filters.Add(p => p.typeOfFeedback.Equals("2"));

        //     var list =  feedInterface.getAll().Where(x => filters.All(f => f(x)));
        //     return Ok(list);
        //}

    }
}
