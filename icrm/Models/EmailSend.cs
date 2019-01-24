using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace icrm.Models
{
    public class EmailSend
    {

        public EmailSend() {


        }


        public async System.Threading.Tasks.Task sendEmailAsync()
        {
            
            System.Diagnostics.Debug.WriteLine("kuch b nahin hai ye jahaaaaa2");
            var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();
            message.To.Add(new MailAddress("iram.8859@gmail.com")); //replace with valid value
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