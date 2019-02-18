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
                   // sendEmailAsync(db.Feedbacks.FirstOrDefault(), "tufail.b.n@gmail.com");

                    var level1query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED && f.type.name == Constants.Complaints &&
                                 f.priority.priorityId == 1 && f.escalationlevel == null && (DateTime.Now- (DateTime)f.assignedDate).TotalHours > Constants.criticalescelationtime &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < (Constants.criticalescelationtime)*2
                                      select f;

                    foreach (Feedback f in level1query) {
                        
                        f.escalationlevel = "level1";
                        if (f.department.name.Equals(Constants.OPERATIONS))
                        {
                            sendEmailAsync(f, getOperationsEscalationUser(f.user.CostCenter.Id).secondEscalationUser.bussinessEmail);
                        }
                        else {
                            sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).secondEscalationUserId).bussinessEmail);
                        }
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;

                    }
                    db.SaveChanges();

                    var level2query = from f in db.Feedbacks.ToList()
                                where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED && f.type.name == Constants.Complaints &&
                                 f.priority.priorityId == 1 && f.escalationlevel == "level1" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > (Constants.criticalescelationtime)*2 &&
                                (DateTime.Now - (DateTime)f.assignedDate).TotalHours < (Constants.criticalescelationtime) * 3
                                      select f;

                    foreach (Feedback f in level2query)
                    {
                        f.escalationlevel = "level2";
                        if (f.department.name.Equals(Constants.OPERATIONS))
                        {
                            sendEmailAsync(f, getOperationsEscalationUser(f.user.CostCenter.Id).secondEscalationUser.bussinessEmail);
                        }
                        else {
                            sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).thirdEscalationUserId).bussinessEmail);
                        }
                        
                        db.Feedbacks.Add(f);
                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();


                    //var level3query = from f in db.Feedbacks.ToList()
                    //                  where f.assignedDate != null && f.checkStatus == Constants.ASSIGNED && f.type.name != Constants.Enquiry &&
                    //                   f.priority.priorityId == 1 && f.escalationlevel == "level2" && (DateTime.Now - (DateTime)f.assignedDate).TotalHours > (Constants.criticalescelationtime) * 3
                    //                  select f;

                    //foreach (Feedback f in level3query)
                    //{
                    //    f.escalationlevel = "level3";
                    //    sendEmailAsync(f, db.Users.Find(getEmailOfUser(f).thirdEscalationUserId).bussinessEmail);
                    //    db.Feedbacks.Add(f);
                    //    db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    //}
                    //db.SaveChanges();

                  
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

            Debug.WriteLine(f +" i am sending mail to " + emailto);
            string text = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Views/EmailTemplate.cshtml"));
            string renderedText = Razor.Parse(text, new { ticketnumber = f.id,
                employeenumber = f.user.EmployeeId, locationname = f.user.Location.name,
                jobtitlename = f.user.JobTitle.name, email = f.user.personalEmail,
                categoryname = f.category.name, title = f.title, description = f.description
                , footer = f.escalationlevel.Equals("level2") ? "Please Note:This Issue has Reached to Highest Level" :" Please Note: This issue should be solved within 4 hrs., otherwise it will be escalated to the next level"} );
            var body = renderedText;


            MailMessage mail = new MailMessage("employee.relation@mcdonalds.com.sa", emailto);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.EnableSsl = false;
            client.Host = "email.mcdonalds.com.sa";
            mail.Subject = "Notification";
            mail.Body = body;
            mail.IsBodyHtml = true;
            await client.SendMailAsync(mail);

           

        }

        public EscalationUser getEmailOfUser(Feedback f) {
            var query = from e in db.EscalationUsers
                        join Category  in db.Categories on e.Id equals Category.EscalationUserId
                        where e.DepartmentId == f.departmentID && Category.Id == f.categoryId
                        select e;
            return query.FirstOrDefault();

        }

        public EscalationUser getOperationsEscalationUser(int? costCenterId)
        {
            EscalationUser escUser = db.EscalationUsers.Where(m => m.CostCenterId == costCenterId).FirstOrDefault();

            return escUser;
        }


    }


}
