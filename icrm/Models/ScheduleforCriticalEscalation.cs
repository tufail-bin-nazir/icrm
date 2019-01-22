using FluentScheduler;
using System;
using System.Collections.Generic;
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
                    var query = from f in db.Feedbacks.ToList()
                                where f.departmentAssignedDate != null && f.internalstatus == "assigned" && f.priorityId== 1
                                select f;
                    foreach (Feedback f in query)
                    {
                            TimeSpan diff = new DateTime() - f.departmentAssignedDate;
                            double hours = diff.TotalHours;
                            if (hours > 4)
                            {
                                f.escalationlevel = "level1";
                                db.Feedbacks.Add(f);
                                db.SaveChanges();
                                 
                            }
                            if (hours > 8)
                            {
                                f.escalationlevel = "level2";
                                db.Feedbacks.Add(f);
                                db.SaveChanges();

                            }
                            if (hours > 12)
                            {
                                f.escalationlevel = "level3";
                                db.Feedbacks.Add(f);
                                db.SaveChanges();

                            }
                    }


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


        private async System.Threading.Tasks.Task sendEmailAsync() {

            var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();
            message.To.Add(new MailAddress("name@gmail.com")); //replace with valid value
            message.Subject = "Your email subject";
            //message.Body = string.Format(body, model.FromName, model.FromEmail, model.Message);
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
               
            }
        }
    }


}
