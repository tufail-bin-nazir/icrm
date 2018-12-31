using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
    public class FeedBackApiController : ApiController
    {
        private IFeedback feedInterface;

        private ApplicationUserManager _userManager;

        public FeedBackApiController()
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
        public IHttpActionResult PostFeedback(FeedBackViewModel feedBackmodel) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

          
            //getting extension of the base 64 file
            String ext = GetFileExtension(feedBackmodel.Attachment);

            var Name1 = User.Identity.Name;
           Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
           Feedback feedBack = new Feedback { title = feedBackmodel.Title, attachment = $@"{Guid.NewGuid()}." + ext, description = feedBackmodel.Description, userId = user.Result.Id };
            string path = @"F:\Files\"+ feedBack.attachment;
            if (!File.Exists(path))
            {
               FileStream fileStream =  File.Create(path);
                fileStream.Close();
            }

            File.WriteAllBytes(path, getfile(feedBackmodel.Attachment));
            
            feedInterface.Save(feedBack);
            return Ok(feedBack);

        }

        private Byte[] getfile(string stringimage) {
            // Convert base 64 string to byte[]
            byte[] file = Convert.FromBase64String(stringimage);
            return file;
           

        }

        private string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png";
                case "/9J/4":
                    return "jpg";
                case "AAAAF":
                    return "mp4";
                case "JVBER":
                    return "pdf";
                case "AAABA":
                    return "ico";
                case "UMFYI":
                    return "rar";
                case "E1XYD":
                    return "rtf";
                case "U1PKC":
                    return "txt";
                case "MQOWM":
                case "77U/M":
                    return "srt";
                default:
                    return string.Empty;
            }
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
