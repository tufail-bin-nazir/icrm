using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using icrm.Models;
using icrm.RepositoryInterface;
using icrm.RepositoryImpl;
using System.Net.Http.Headers;
using System.Data.Entity;

namespace icrm.WebApi
{


    [Authorize]
    public class HrApiController : ApiController
    {
        private IFeedback feedInterface;

        ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _HrManager;

        public HrApiController()
        {
            feedInterface = new FeedbackRepository();
        }


        [HttpGet]
        //Get /api/ HrApi
        public IHttpActionResult HrTicketslist()
        {
            
                var Query = from f in feedInterface.getAllOpen()

                            where f.status == "Open"
                            select new {f.id,f.title,f.description, f.createDate, f.status,f.user.EmployeeId,};

                if (Query != null)
                {

                    return Ok(Query.ToList());

                }
                else
                {

                    return BadRequest("hr list not found");

                }
            
        }


        [HttpGet]
        [Route("api/HR/HrTicket/{id}")]
        //Get /api/ HR / id
        public IHttpActionResult HrTicket(string id)
        {

            var Query = from f in feedInterface.getAllOpen()

                        where f.id == id
                        select new { f.id, f.title,f.attachment, f.description, f.user.EmployeeId,f.user.Email,f.user.FirstName };

            if (Query != null)
            {
                
                return Ok(Query.SingleOrDefault());

            }
            else
            {

                 return  BadRequest("hr Ticket not found");

            }


        }

        [HttpGet]
        [Route("api/HR/getFile/{filename}")]
        public IHttpActionResult getFile(string filename)
        {
            Feedback f = feedInterface.Find(filename);
            string path = Constants.PATH + f.attachment;

            if (path != null)
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(path);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                return Ok(base64ImageRepresentation);
            }

            else
            {

                return BadRequest("File Not found");

            }
        }


       [HttpPost]
       [Route("api/HR/Resolve/{id}")]
        public IHttpActionResult Resolve(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest("Employee id not found");

            }

            else
            {
                    
                    f.status = feedback.status;
                    f.response = feedback.response;
                    db.Entry(f).State = EntityState.Modified;
                    db.SaveChanges();
                
                return Ok();

            }


        }



    }
}