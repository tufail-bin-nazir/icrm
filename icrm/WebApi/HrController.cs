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
using Microsoft.AspNet.Identity.Owin;

namespace icrm.WebApi
{


    [Authorize]
    public class HrApiController : ApiController
    {
        private IFeedback feedInterface;

        ApplicationDbContext db = new ApplicationDbContext();

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

        public HrApiController()
        {
            feedInterface = new FeedbackRepository();
        }

        /// <summary>
        /// /////////////////////////////////////************* HrTicketslist *****************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/HrTicketslist")]
        public IHttpActionResult HrTicketslist()
        {

            var Query = from f in feedInterface.getAllOpen()

                        where f.status == "Open"
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, };

            if (Query != null)
            {

                return Ok(Query.ToList());

            }
            else
            {
                
                return BadRequest("hr list not found");

            }

        }
        /// <summary>
        /// /////////////////////////////////////************* HrTicket by id *****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/HrTicket/{id}")]
        //Get /api/ HR / id
        public IHttpActionResult HrTicket(string id)
        {

            var Query = from f in feedInterface.getAllOpen()

                        where f.id == id
                        select new { f.id, f.title, f.attachment, f.description, f.user.EmployeeId, f.user.Email, f.user.FirstName };

            if (Query != null)
            {

                return Ok(Query.SingleOrDefault());

            }
            else
            {

                return BadRequest("hr Ticket not found");

            }
        }
        /// <summary>
        /// /////////////////////////////////////************* GetFile *****************/////////////////
        /// </summary>
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

        /// <summary>
        /// /////////////////////////////////////*************Resolve By Id *****************/////////////////
        /// </summary>
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

        /// <summary>
        /// /////////////////////////////////////*************Priority*****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/priority")]
        public IHttpActionResult priority()
        {

            var entity = db.Priorities.ToList();

            if (entity != null)
            {


                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Prioritylist not found");

            }
        }

        /// <summary>
        /// /////////////////////////////////////*************catagorey*****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/catagorey")]
        public IHttpActionResult catagorey()
        {

            var entity = db.Categories.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Catagoreylist  not found");

            }
        }

        /// <summary>
        /// /////////////////////////////////////*************Department*****************/////////////////
        /// </summary>


        [HttpGet]
        [Route("api/HR/Department")]
        public IHttpActionResult Department()
        {

            var entity = db.Departments.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Departmentlist  not found");

            }
        }


        /// <summary>
        /// /////////////////////////////////////*************forwardTicket*****************/////////////////
        /// </summary>

        [HttpPost]
        [Route("api/HR/forwardTicket/{id}")]
        public IHttpActionResult forwardTicket(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                f.categoryId = feedback.categoryId;
                f.priorityId = feedback.priorityId;
                f.departmentID = feedback.departmentID;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return Ok();

            }
        }


        /// <summary>
        /// /////////////////////////////////////*************Departmentlist*****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/Departmentlist")]
        public IHttpActionResult Departmentlist()
        {


            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            var Query = from f in feedInterface.getAllOpen()

                        where f.departmentID==user.Result.DepartmentId
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId};

            if (Query != null)
            {

                return Ok(Query.ToList());

            }
            else
            {

                return BadRequest("Department list not found");

            }

        }
        /// <summary>
        /// /////////////////////////////////////*************DepartmentbyId*****************/////////////////
        /// </summary>


        [HttpGet]
        [Route("api/HR/DepartmentbyId/{id}")]
        public IHttpActionResult DepartmentbyId(string id)
        {

            var Query = from f in feedInterface.getAllOpen()

                        where f.id == id
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId,f.user.FirstName,f.user.Email,f.category,f.priority,};

            if (Query != null)
            {

                return Ok(Query.FirstOrDefault());

            }
            else
            {

                return BadRequest("Department id not found");

            }

        }

        /// <summary>
        /// ////////////////////////******************* updateTicketDepartment***************////////////////////
        /// </summary>

        [HttpPost]
        [Route("api/HR/updateTicketDepartment/{id}")]
        public IHttpActionResult updateTicketDepartment(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                f.response = feedback.response;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return Ok();

            }
        }
        /// <summary>
        /// ////////////////////////////****************RespondedTicketList*******************////////////////////////////
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/HR/respondedTicketList")]
        public IHttpActionResult respondedTicketList()
        {

            var Query = from f in feedInterface.getAllResponded()

                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, };

            if (Query != null)
            {

                return Ok(Query.ToList());
                
            }
            else
            {

                return BadRequest("RespondedTicketList not found");

            }

        }
        /// <summary>
        ////////////////////////////////////**************** respondedTicketItem*************/////////////////
        /// </summary>


        [HttpGet]
        [Route("api/HR/respondedTicketItem/{id}")]
        public IHttpActionResult respondedTicketItem(string id)
        {

            var Query = from f in feedInterface.getAllOpen()

                        where f.id == id
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, f.user.FirstName, f.user.Email,f.response ,f.category, f.priority, };

            if (Query != null)
            {

                return Ok(Query.FirstOrDefault());

            }
            else
            {

                return BadRequest(" Id not found");

            }

        }

        /// <summary>
        //////////////////////////*************** update Ticket which is responded***************////////////////////////
        /// </summary>
       

        [HttpPost]
        [Route("api/HR/updateTicketResponded/{id}")]
        public IHttpActionResult updateTicketResponded(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                f.status = feedback.status;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return Ok();

            }
        }

        /// <summary>
        //////////////////////////////**************** closed list****************//////////////////////////////////////////////////////////// 
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/HR/Closed")]
        public IHttpActionResult Closed()
        {

            var Query = from f in feedInterface.getAllClosed()
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, f.user.FirstName, f.user.Email, f.category, f.priority, };

            if (Query != null)
            {

                return Ok(Query.ToList());

            }
            else
            {

                return BadRequest("No closed list  found");

            }
        }
        /// <summary>
        /// ///////////////////*********************** userTicketView************//////////////////////////////////
        /// </summary>
       
        [HttpGet]
        [Route("api/HR/userTicketView/{id}")]
        public IHttpActionResult userTicketView(string id)
        {
            var f = db.Feedbacks.Find(id);
                   

            if (f != null)
            {
                //var obj =new {f.createDate, f.title, f.description, f.response, f.satisfaction, f.status }.ToString();
                return Ok(new { f.createDate, f.title, f.description, f.response, f.satisfaction, f.status});

            }
            else
            {

                return BadRequest("User list  not found");

            }
        }


       
       


    }
}