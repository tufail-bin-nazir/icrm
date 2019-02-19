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
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using Constants = icrm.Models.Constants;
using System.Net.Mail;
using System.Net.Mime;
using icrm.Controllers;
using Microsoft.AspNet.Identity.EntityFramework;

namespace icrm.WebApi
{


    [Authorize]
    public class HrApiController : ApiController
    {



        public HRController c;

        public EmailSend emailsend;

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
             c = new HRController();
            emailsend = new EmailSend();

            feedInterface = new FeedbackRepository();
        }

        //1/ <summary>
        /// /////////////////////////////////////************* HrTicketslist *****************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/HrTicketslist")]
        public IHttpActionResult HrTicketslist()
        {
            

            var Query = from f in feedInterface.getAllOpenMobile()
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId,};

            if (Query != null)
            {

                return Ok(Query.ToList());

            }
            else
            {

                return BadRequest("hr list not found");

            }

        }
        //2/ <summary>
        /// /////////////////////////////////////************* HrTicket by id *****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/HrTicket/{id}")]
        public IHttpActionResult HrTicket(string id)
        {
            var quer = from n in feedInterface.getCOmments(id)
                       select n;

            var f = feedInterface.Find(id);
                        //select new { f.id,f.checkStatus, f.title, f.attachment, f.description, f.user.EmployeeId, f.user.Email, f.user.FirstName, f.type.name,quer };

            if (f != null)
            {

                return Ok(new { f.id, f.checkStatus, f.title, f.attachment, f.description, f.user.EmployeeId, f.user.Email, f.user.FirstName, f.type.name, quer });

            }
            else
            {

                return BadRequest("hr Ticket not found");

            }
        }
        //3/ <summary>
        /// /////////////////////////////////////************* GetFile *****************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/getFile/{filename}")]
        public IHttpActionResult getFile(string filename)
        {
            Feedback f = feedInterface.Find(filename);
            string path = Models.Constants.PATH + f.attachment;

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

        //4/ <summary>
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

                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
                f.checkStatus = Models.Constants.RESOLVED;
                f.status = feedback.status;
                f.response = feedback.response;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                /////////////////////////////////////////////////////////////
                Comments c = new Comments();
                c.date = DateTime.Now;
                c.feedbackId = Id;
                c.commentedById = user.Result.Id;
                c.text = feedback.response;
                db.comments.Add(c);
                db.SaveChanges();
                return Ok();


              

            }
        }

        //5/ <summary>
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

        //6/ <summary>
        /// /////////////////////////////////////*************catagorey*****************/////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/catagorey/{deptId}/{type}")]
        public IHttpActionResult catagorey(int deptId, string type)
        {
            var typeid = db.FeedbackTypes.Where(e => e.name == type).SingleOrDefault().Id;
            var entity = from f in feedInterface.getCategories(deptId, typeid)
                         select f;

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Catagoreylist  not found");

            }
        }

        //7/ <summary>
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


        //8/ <summary>
        /// /////////////////////////////////////*************forwardTicket*****************/////////////////
        /// </summary>
        [HttpPost]
        [Route("api/HR/forwardTicket/{id}")]
        public IHttpActionResult forwardTicket(string Id, Forwardmodel forward)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else if (db.Departments.Find(forward.departmentID).name==Constants.OPERATIONS)
            {
                ApplicationUser user1 = feedInterface.getOperationsEscalationUser(f.user.CostCenterId);
                f.departUserId = user1.Id;
            }
            
            else
            {
                ApplicationUser user = feedInterface.getEscalationUser(f.departmentID, f.categoryId);
                f.departUserId = user.Id;
               

            }
            f.checkStatus = Models.Constants.ASSIGNED;
            f.subcategoryId = forward.subcategoryId;
            if (forward.priorityId != 0)
            {
                f.priorityId = forward.priorityId;

            }
            f.categoryId = forward.categoryId;
            f.departmentID = forward.departmentID;
            f.assignedDate = DateTime.Now;

            db.Entry(f).State = EntityState.Modified;
            db.SaveChanges();

            return Ok();

        }


        //9/ <summary>
        /// /////////////////////////////////////*************Departmentlist*****************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/Departmentlist")]
        public IHttpActionResult Departmentlist()
        {

            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
              var query = from f in feedInterface.GetAllAssigned()

                          where f.departUserId == user.Result.Id
                          select new { f.id, f.title, f.description, f.createDate, f.status,f.type.name, f.user.EmployeeId,f.user.FirstName};
                                   

              if (query != null)
              {

                  return Ok(query.ToList());

              }
              else
              {

                  return BadRequest("Department list not found");

              }
          
        }

        //10/ <summary>
        /// /////////////////////////////////////*************DepartmentbyId*****************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/DepartmentbyId/{id}")]
        public IHttpActionResult DepartmentbyId(string id)
        {

            var quer = from n in feedInterface.getCOmments(id)
                       select n;


            var f = db.Feedbacks.Find(id);

            if (f != null)
            {
               
                return Ok(new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, f.user.FirstName, f.user.Email, f.category, f.priority, f.type.name, quer });

            }

            else
            {

                return BadRequest("Department id not found");

            }

        }

        //11/ <summary>
        /// ////////////////////////******************* update Ticket Department***************////////////////////
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

                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
                f.checkStatus = Models.Constants.RESPONDED;
                f.responseDate = DateTime.Now;
                f.timeHours = System.Math.Round((DateTime.Now - (DateTime)f.assignedDate).TotalHours, 2);
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                /////////////////////////////////////////////////////////////
                Comments c = new Comments();
                c.date = DateTime.Now;
                c.feedbackId = Id;
                c.commentedById = user.Result.Id;
                c.text = feedback.response;
                db.comments.Add(c);
                db.SaveChanges();

              
                return Ok();


            }
        }
        //12/ <summary>
        /// ////////////////////////////****************RespondedTicketList*******************////////////////////////////
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/HR/respondedTicketList")]
        public IHttpActionResult respondedTicketList()
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);

            var Query = from f in feedInterface.GetAllResponded()
                        where f.departUserId == user.Result.Id
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
        //13/ <summary>
        ////////////////////////////////////**************** respondedTicketItem*************/////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/respondedTicketItem/{id}")]
        public IHttpActionResult respondedTicketItem(string id)
        {
            var quer = from n in feedInterface.getCOmments(id)
                       select n;


            var Query = from f in feedInterface.GetAllResponded()
                        where f.id == id
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, f.user.FirstName, f.user.Email, f.response, f.category, f.priority,quer };

            if (Query != null)
            {

                return Ok(Query.FirstOrDefault());

            }
            else
            {

                return BadRequest(" Id not found");

            }

        }

        //14/ <summary>
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
                if (f.status == "Open")
                {
                    f.checkStatus = Models.Constants.OPEN;


                }
                else if (f.status == "Resolved")
                {
                    f.checkStatus = Models.Constants.RESOLVED;


                }
                else if(f.status=="Closed")
                {
                    f.checkStatus = Models.Constants.CLOSED;


                }
                if (feedback.satisfaction != null)
                {

                    f.satisfaction = feedback.satisfaction;
                }

                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                //////////////////////////////////////////////
                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
                Comments c = new Comments();
                c.date = DateTime.Now;
                c.feedbackId = Id;
                c.commentedById = user.Result.Id;
                c.text = feedback.description;
                db.comments.Add(c);
                db.SaveChanges();
                return Ok();

            }
        }

        //15/ <summary>
        //////////////////////////////**************** closed list****************//////////////////////////////////////////////////////////// 
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/HR/Closed")]
        public IHttpActionResult Closed()
        {

            var Query = from f in feedInterface.GETAllClosed()
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
        //16/ <summary>
        /// ///////////////////*********************** userTicketView************//////////////////////////////////
        /// </summary>

        [HttpGet]
        [Route("api/HR/userTicketView/{id}")]
        public IHttpActionResult userTicketView(string id)
        {
            
            var quer = from n in feedInterface.getCOmments(id)
                       select n;
                       

            var f = db.Feedbacks.Find(id);
            
            if (f != null)
            {
               
                return Ok(new { f.createDate,f.priority,f.checkStatus, f.title, f.description, f.response, f.satisfaction, f.status, f.type ,quer});

            }
            else
            {

                return BadRequest("User list  not found");

            }
        }

        //17/ <summary>
        /// //////////////////////////********************update satisfaction***************//////////////////////
        /// </summary>
        [HttpPost]
        [Route("api/HR/updatesatisfaction/{id}")]
        public IHttpActionResult updatesatisfaction(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);
            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                f.satisfaction = feedback.satisfaction;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return Ok();

            }
        }


        //18/ <summary>
        /// ********************************************   Resolved tickets Here ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/Resolved")]
        public IHttpActionResult Resolved()
        {

            var Query = from f in feedInterface.GetAllResolvedmobile()
                        
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId, f.user.FirstName, f.user.Email, f.category, f.priority, };

            if (Query != null)
            {
               
                return Ok(Query.ToList());

            }
            else
            {

                return BadRequest("No Resolved list  found");

            }
        }


        //19/ <summary>
        /// ********************************************   update  Resolve  by id  ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/HR/updateResolve/{id}")]
        public IHttpActionResult updateResolve(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);         
                f.status = feedback.status;
                if (f.status == "Closed")
                {
                    f.checkStatus = Models.Constants.CLOSED;

                }
                else if(f.status=="Open")
                {

                    f.checkStatus = Models.Constants.OPEN;

                }

                else
                {
                    f.checkStatus = Models.Constants.RESOLVED;

                }

                if (feedback.satisfaction != null)
                {

                    f.satisfaction = feedback.satisfaction;

                }
               
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                ////////////////////////////////////////////////////////// 
                if (feedback.response != null)   
                {
                    Comments c = new Comments();
                    c.date = DateTime.Now;
                    c.feedbackId = Id;
                    c.commentedById = user.Result.Id;
                    c.text = feedback.response;
                    db.comments.Add(c);
                    db.SaveChanges();   
                }
               
                return Ok();

            }
        }




        //20/ <summary>
        /// ******************************************** Departments list    ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/departments/list")]
        public IHttpActionResult departments()
        {

            var entity = db.Departments.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Departments list not found");

            }
        }




        //21/ <summary>
        /// ********************************************  Religion List    ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/Religion/list")]
        public IHttpActionResult Religion()
        {

            var entity = db.Religions.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Religion list  not found");

            }
        }


        //22/ <summary>
        /// ********************************************  Nationalities list   ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/nationalities/list")]
        public IHttpActionResult nationalities()
        {

            var entity = db.Nationalities.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Nationalities list not found");

            }
        }

        //23/ <summary>
        /// ******************************************** Positions list    ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/Positions/list")]
        public IHttpActionResult Positions()
        {

            var entity = db.Positions.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Positions list not found");

            }
        }


        //24/ <summary>
        /// ********************************************  Locations list   ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/Locations/list")]
        public IHttpActionResult Locations()
        {

            var entity = db.Locations.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Locations list not found");

            }
        }


        //25/ <summary>
        /// ********************************************  SubLocations list   ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/SubLocations/list")]
        public IHttpActionResult SubLocations()
        {

            var entity = db.SubLocations.ToList();

            if (entity != null)
            {

                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" SubLocations List  not found");

            }
        }

        //26/ <summary>
        /// ******************************************** JobTitles  ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/jobTitles/list")]
        public IHttpActionResult jobTitles()
        {

            var entity = db.JobTitles.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" JobTitles List  not found");

            }
        }

        //27/ <summary>
        /// ******************************************** PayScale list ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/payScale/list")]
        public IHttpActionResult payScale()
        {

            var entity = db.PayScaleTypes.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" PayScale List  not found");

            }
        }


        //28/ <summary>
        /// ******************************************** Get Profile ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/getProfile")]
        public IHttpActionResult getProfile()
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);

            if (user != null)
            {

                return Ok(new { user.Result.FirstName, user.Result.LastName, user.Result.Email, user.Result.EmployeeId, user.Result.PhoneNumber,
                    user.Result.JobTitle, user.Result.Location, user.Result.Department, user.Result.Religion, user.Result.Nationality,
                    user.Result.PayScaleType, user.Result.Position, user.Result.SubLocation, });

            }
            else
            {

                return BadRequest(" Profile not found");

            }

        }


        //29/ <summary>
        /// ******************************************** update Profile by id ***************************////
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [Route("api/HR/updateProfile")]
        public IHttpActionResult updateProfile(UserProfileViewModel model)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);

            Debug.WriteLine("location id: " + model.LocationId);
            if (user == null)
            {

                return BadRequest(" not found");

            }

            else
            {
                user.Result.LocationId = model.LocationId;
                user.Result.JobTitleId = model.JobTitleId;
                user.Result.DepartmentId = model.DepartmentId;
                user.Result.SubLocationId = model.SubLocationId;
                user.Result.ReligionId = model.ReligionId;
                user.Result.NationalityId = model.NationalityId;
                user.Result.PayScaleTypeId = model.PayScaleTypeId;
                user.Result.PositionId = model.PositionId;
                UserManager.Update(user.Result);
                return Ok();

            }
        }

        //30/ <summary>
        /// ////////////////////////////**************** Assigned Ticket List*******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/assignedTicketList")]
        public IHttpActionResult assignedTicketList()
        {


            var Query = from f in feedInterface.GetAllAssigned()
                       
                       
                        select new { f.id, f.title, f.description, f.createDate, f.status, f.user.EmployeeId,};

            if (Query != null)
            {

                return Ok(Query.ToList());
            }
            else
            {

                return BadRequest(" Assigned List not found");

            }

        }

        //31/ <summary>
        /// ////////////////////////////**************** FeedBackTypes Ticket List *******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("api/HR/feedBackType/list")]
        public IHttpActionResult feedBackType()
        {

            var entity = db.FeedbackTypes.ToList();

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" FeedbackTypes  not found");

            }
        }
        
        //32/ <summary>
        /// ////////////////////////////**************** image Save *******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("api/HR/imageSave")]
        public IHttpActionResult imageSave([FromBody]SaveFile file)
        {

            Feedback feedBack = db.Feedbacks.Find(file.id);
            Debug.WriteLine(file.ImageSave);

            if (!string.IsNullOrWhiteSpace(file.ImageSave))
            { 
                string ext = GetFileExtension(file.ImageSave);
                feedBack.attachment = feedBack.id + "." + ext;
                string path = Models.Constants.PATH + feedBack.attachment;
                File.WriteAllBytes(path, getfile(file.ImageSave));
                db.Entry(feedBack).State = EntityState.Modified;
                db.SaveChanges();
                return Ok();

            }
            else
            {

                return BadRequest("file not found");

            }

        }

        private Byte[] getfile(string stringimage)
        {

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

        //34/ <summary>
        /// /////////////////////////////////////*************Rejected By Id *****************/////////////////
        /// </summary>
        [HttpPost]
        [Route("api/HR/Rejected/{id}")]
        public IHttpActionResult Rejected(string Id, Feedback feedback)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest("Id not found");

            }

            else
            {
                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
                f.checkStatus = Models.Constants.REJECTED;
                f.status= Models.Constants.REJECTED;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                /////////************** New code******************////////////////////
                Comments c = new Comments();
                c.date = DateTime.Now;
                c.feedbackId = Id;
                c.commentedById = user.Result.Id;
                c.text = feedback.checkStatus;
                db.comments.Add(c);
                db.SaveChanges();
                return Ok();

            }
        }


        //33/ <summary>
        /// ******************************************** Subcatagorey list    ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/Subcategories/{categoryId}/{type}")]
        public IHttpActionResult Subcategories(int categoryId,string type)
        {
            var typeid = db.FeedbackTypes.Where(e => e.name == type).SingleOrDefault().Id;
            var entity = from f in feedInterface.getSubCategories(categoryId,typeid )
                         select f;
            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Subcatagorey list not found");

            }
        }

        //34/ <summary>
        /// ******************************************** updateTicket by id ***************************////
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/HR/updateTicket/{id}")]
        public IHttpActionResult updateTicket(string Id, FeedBackViewModel feedBackmodel)
        {
            Feedback f = db.Feedbacks.Find(Id);

            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {
                f.checkStatus = Models.Constants.OPEN;
                f.status = Models.Constants.OPEN;
                f.typeId = feedBackmodel.Typeid;
                f.title = feedBackmodel.Title;
                f.description = feedBackmodel.Description;
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                return Ok();

            }
        }

        //35/ <summary>
        /// ////////////////////////////**************** Email list *******************////////////////////////////
        /// </summary>
        // <returns></returns> 
        [HttpGet]
        [Route("api/Hr/Emaillist")]
        public IHttpActionResult Emaillist()
        {

            var entity = from f in feedInterface.getEmails()
                         select f;

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Email list  not found");

            }
        }

        //36/ <summary>
        /// ////////////////////////////**************** Recieve Emails *******************////////////////////////////
        /// </summary>
        // <returns></returns> 
        [HttpPost]
        [Route("api/Hr/recieveEmails/{id}")]
        public IHttpActionResult recieveEmails(string id, RecieveEmails emails )
        {
            Debug.WriteLine(emails);
          

            string combindedString = "";
            combindedString = string.Join(",", emails.EmailList.ToArray());
            combindedString = combindedString.Replace("[", " ");
            combindedString = combindedString.Replace("]", "");
            emailsend.sendEmailAsync(combindedString, id);
            return Ok();


        }

      

        //37/ <summary>
        /// ////////////////////////////****************Get HR Responded TicketList*******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HR/respondedticket")]
        public IHttpActionResult respondedticket()
        {
          
            var Query = from f in feedInterface.GetAllRespondedMobile()
                       
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

        //38/ <summary>
        /// ////////////////////////////**************** Get All Suggestions*******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        [Route("api/HR/getsuggestionlist")]
        public IHttpActionResult getsuggestionlist()
        {
            string type = Models.Constants.Suggestion;

            var entity = from f in feedInterface.GetListBasedOnType(type)
                         select f;

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Suggestion list  not found");

            }
        }

        //38/ <summary>
        /// ////////////////////////////****************Get All Enquirey list*******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        [Route("api/HR/getenquireslist")]
        public IHttpActionResult getenquireslist()
        {
            string type = Models.Constants.Enquiry;

            var entity = from f in feedInterface.GetListBasedOnType(type)
                         select f;

            if (entity != null)
            {
                return Ok(entity.ToList());

            }
            else
            {

                return BadRequest(" Enquirey list  not found");

            }
        }

        //39/ <summary>
        /// ////////////////////////////**************** Forget Password*******************////////////////////////////
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/HR/ForgetPassword/{id}")]
        public async Task<IHttpActionResult> ForgotPassword(string id)
        {
           
                var user = await UserManager.FindByNameAsync(id);
                if (user == null )
                {
                  
                    return BadRequest("ForgotPasswordConfirmation");
                }
                else
                {
                   
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    sendemailAsync(user.Email, user.Id, code);
                    return Ok();
                }
        }

        private async void sendemailAsync(String emailto, String userid, String code)
        {
             MailMessage mail = new MailMessage("employee.relation@mcdonalds.com.sa", emailto);
           // MailMessage mail = new MailMessage("tufail.b.n@gmail.com", "wajahatnabi90@gmail.com");
            SmtpClient client = new SmtpClient();           
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.EnableSsl = false;
            client.Host = "email.mcdonalds.com.sa";  
            mail.Subject = "Reset Your Password";
            string strBody3 = "<html><body><a href='http://37.76.254.20/Account/ResetPassword?code=' " + code + "'" + " > Click On The Link To Reset Your Password </ a ></ body ></ html > ";
            mail.Body = strBody3;
            mail.IsBodyHtml = true;
            await client.SendMailAsync(mail);
        }

        //40/ <summary>
        /// ///////////////////*********************** enduserTicketView************//////////////////////////////////
        /// </summary>
        [HttpGet]
        [Route("api/HR/enduserTicketView/{id}")]
        public IHttpActionResult enduserTicketView(string id)
        {
            var comments =feedInterface.getCOmments(id);
            Debug.Print(comments.Count+"--------------commm count");               
                      
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            Debug.Print("-----" + roleManager);
            var userRole = roleManager.FindByName(Constants.ROLE_HR).Users.FirstOrDefault();
            Debug.Print("-----" + userRole.RoleId);
            foreach (var comm in comments) {
               // Debug.Print(comm.commentedBy.FirstName+"-------name");
                Debug.Print(comm.commentedBy.Roles.FirstOrDefault()+"-------default");
            }
            comments.ForEach(c=>Debug.Print(c.commentedBy.FirstName+"-------"+c.commentedBy.Roles.FirstOrDefault().RoleId));
            List<Comments> quer = comments.Where(c=>c.commentedBy.Roles.Any(r => r.RoleId == userRole.RoleId)).ToList();
            Debug.Print(roleManager + "----rolemanager----" + userRole + "-----ur---" + quer + "----qr");
            Debug.Print(quer.Count+"------------size");
            foreach (var item in quer)
            {
                Debug.Print(item.commentedBy.FirstName+"---name");
            }

            var f = db.Feedbacks.Find(id);

            if (f != null)
            {
              
                return Ok(new { f.createDate, f.checkStatus, f.title, f.description, f.response, f.satisfaction, f.status, f.type, quer });

            }
            else
            {

                return BadRequest("User list  not found");

            }
        }

        //41/ <summary>
        /// //////////////////////////********************closed update***************//////////////////////
        /// </summary>
        [HttpPost]
        [Route("api/HR/closedupdate/{id}")]
        public IHttpActionResult closedupdate(string Id, ClosedFeedbackModel forward)
        {
            Feedback f = db.Feedbacks.Find(Id);
            if (f == null)
            {

                return BadRequest(" id not found");

            }

            else
            {

                f.status = forward.status;
                if (f.status == "Open")
                {
                    f.checkStatus = Models.Constants.OPEN;

                }
                if (f.satisfaction != null)
                {

                    f.satisfaction = forward.satisfaction;
                }

                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return Ok();

            }
        }




    }
}