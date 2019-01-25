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
    public class ScheduleforCriticalEscalation : IJob, IRegisteredObject
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly object _lock = new object();

        private bool _shuttingDown;


        public ScheduleforCriticalEscalation()
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

                    Debug.WriteLine(DateTime.Now + "---------" + db.Feedbacks.First().assignedDate);
                   
                    //string text = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Views/EmailTemplate.cshtml"));

                    //Debug.WriteLine(text + "0--00--0-0-0-0-00--0-00-0-00-0");
                    var level1query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED && 
                                f.priorityId == 1 && f.escalationlevel is null && (DateTime.Now- (DateTime)f.assignedDate).TotalHours > 4 &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < 8 
                                select f;

                    foreach (Feedback f in level1query) {
                        
                        f.escalationlevel = "level1";
                        sendEmailAsync(f, "kmiraste@gmail.com");
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                    var level2query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED &&
                                f.priorityId == 1 && f.escalationlevel == "level1" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > 8 &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < 12
                                select f;

                    foreach (Feedback f in level2query)
                    {
                        f.escalationlevel = "level2";
                        sendEmailAsync(f, "mymdamin@gmail.com");
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();


                    var level3query = from f in db.Feedbacks.ToList()
                                      where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED &&
                                      f.priorityId == 1 && f.escalationlevel == "level2" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > 12
                                      select f;

                    foreach (Feedback f in level3query)
                    {
                        f.escalationlevel = "level3";
                        sendEmailAsync(f, "amin@stie.com.sg");
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                    //foreach (Feedback f in query)
                    //{

                    //        TimeSpan diff = DateTime.Now - (DateTime)f.departmentAssignedDate;
                    //        double hours = diff.TotalHours;
                    //        if (hours > 4 || hours < 8)
                    //        { 
                    //            if (f.escalationlevel != null)
                    //            {
                    //                f.escalationlevel = "level1";
                    //                db.Feedbacks.Add(f);
                    //                db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    //                db.SaveChanges();
                    //            }

                    //        }
                    //       else if (hours > 8 || hours < 12)
                    //        {
                    //            if (f.escalationlevel != null)
                    //            { 
                    //                f.escalationlevel = "level2";
                    //                db.Feedbacks.Add(f);
                    //                db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    //                db.SaveChanges();
                    //            }

                    //        }
                    //        else if(hours > 12 )
                    //        {
                    //            if (f.escalationlevel != null)
                    //            {
                    //                f.escalationlevel = "level3";
                    //                db.Feedbacks.Add(f);
                    //                db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    //                db.SaveChanges();
                    //            }
                    //    }
                    //}


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
            message.Body = string.Format(body, "icrm", "tufail.b.n@gmail.com", "");
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
               
            }
         }

        
    }


}
