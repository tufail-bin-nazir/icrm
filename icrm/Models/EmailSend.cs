using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Data.Entity.Core.Objects;
using System.Net.Http;
using System.Web.UI.WebControls;
using System.Web.UI;
using Constants = icrm.Models.Constants;
using Comments = icrm.Models.Comments;
using System.Net;
using System.Net.Mime;
using System.Diagnostics;
using System.Threading.Tasks;

namespace icrm.Models
{
    public class EmailSend
    {

        public EmailSend()
        {


        }


        public async Task sendEmailAsync(string emails,string body)
        {

            string[] Multi = emails.Split(',');
            foreach (string email in Multi)
            {
                Debug.WriteLine(email + "hhhhhhhhhhhhhhgsssssssss");
            }

            string b = body;


          //  EmailSend e = new EmailSend();
           // var message = new MailMessage();

            MailMessage message = new MailMessage("employee.relation@mcdonalds.com.sa", "tufail.b.n@gmail.com");
           
           



            foreach (string email in Multi)
            {
                message.To.Add(new MailAddress(email));
            }
           
            //message.To.Add(new MailAddress("iram.8859@gmail.com")); //replace with valid value
            message.Subject = "Ticket has been Forwarded ";

            message.Body = string.Format(b, "Owa.mcdonalds.com.sa", "Owa.mcdonalds.com.sa", "Owa.mcdonalds.com.sa");

            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                smtp.Port = 25;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = true;
                smtp.EnableSsl = false;
                smtp.Host = "email.mcdonalds.com.sa";
                await smtp.SendMailAsync(message);

            }

        }


    }
    }


