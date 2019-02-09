using FluentScheduler;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;

namespace icrm.Models
{
    public class ScheduleforMediumEscalation : IJob, IRegisteredObject
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly object _lock = new object();

        private bool _shuttingDown;


        public ScheduleforMediumEscalation()
        {
            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of IIS shutting down.
            HostingEnvironment.RegisterObject(this);
        }

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (_shuttingDown)
                        return;
 
                    var level1query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED &&
                                 f.priority.priorityId == 3 && f.escalationlevel == null && (DateTime.Now- (DateTime)f.assignedDate).TotalHours > Constants.mediumescelationtime &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < (Constants.mediumescelationtime) *2
                                      select f;

                    foreach (Feedback f in level1query) {
                        
                        f.escalationlevel = "level1";
                        sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).firstEscalationUserId).bussinessEmail);
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                    var level2query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED &&
                                 f.priority.priorityId == 3 && f.escalationlevel == "level1" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > (Constants.mediumescelationtime) *2 &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < (Constants.mediumescelationtime) * 3
                                      select f;

                    foreach (Feedback f in level2query)
                    {
                        f.escalationlevel = "level2";
                        sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).firstEscalationUserId).bussinessEmail);
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();


                    var level3query = from f in db.Feedbacks.ToList()
                                      where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED &&
                                      f.priority.priorityId == 3 && f.escalationlevel == "level2" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > (Constants.mediumescelationtime) * 3
                                      select f;

                    foreach (Feedback f in level3query)
                    {
                        f.escalationlevel = "level3";
                        sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).firstEscalationUserId).bussinessEmail);
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                  
                }
            }
            finally
            {
                // Always unregister the job when done.
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public void Stop(bool immediate)
        {
            // Locking here will wait for the lock in Execute to be released until this code can continue.
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }


        private async System.Threading.Tasks.Task sendEmailAsync(Feedback f, String emailto) {

            
            string text = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Views/EmailTemplate.cshtml"));
            string renderedText = Razor.Parse(text, new { ticketnumber = f.id,
                employeenumber = f.user.EmployeeId, locationname = f.user.Location.name,
                jobtitlename = f.user.JobTitle.name, email = f.user.personalEmail,
                categoryname = f.category.name, title = f.title, description = f.description
                , footer = f.escalationlevel.Equals("level3") ? "Please Note:This Issue has Reached to Highest Level" :" Please Note: This issue should be solved within 4 hrs., otherwise it will be escalated to the next level"} );
            var body = renderedText;
            var message = new MailMessage();
            message.To.Add(new MailAddress(emailto)); //replace with valid value
            message.Subject = "Notification";
            message.Body = string.Format(body, "icrm", "info@stie.com.sg", "");
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
               
            }
         }

        public EscalationUser getEmailOfUser(Feedback f)
        {
            var query = from e in db.EscalationUsers
                        join Category in db.Categories on e.Id equals Category.EscalationUserId
                        where e.DepartmentId == f.departmentID && Category.Id == f.categoryId
                        select e;
            return query.FirstOrDefault();

        }


    }


}
