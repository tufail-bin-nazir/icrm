using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using icrm.Events;
using Newtonsoft.Json;

namespace icrm.Models
{
    public class Notification
    {

        public async void OnMessageBroadcasted(object sender,BroadcastMessageEventArgs args)
        {
            Debug.Print("-----------in notificationbroadcasting message-------");
            //RegisterId you got from Android Developer.
            String recieverId = "fo8m-sJVhSk:APA91bFd-bqtdW_j_6olexzPII4_KVxgIRtOWD2ZPCO900WjjyycfU3Gy7Q6-iUOeYQ3_Q-MmhgMEHvuZxEiImcr6bjVxqki-vhE_vJmRviM2xY3VsMYlAPckZ2lM0_Bh7zCnTnBWH4H";
            string Google_App_ID = "AIzaSyBJG69jVZWgFt7ayf-FC3Wervecxfjm0Dg";
            string Sender_ID = "AIzaSyBJG69jVZWgFt7ayf-FC3Wervecxfjm0Dg";
            string title = "Demo Notification";
            string tickerText = "Message Recieved";
            string contentTitle = args.BroadcastMessage.Text;
            /*string postData =
            "{ \"registration_ids\": [ \"" + recieverId + "\" ], " +
              "\"data\": {\"tickerText\":\"" + tickerText + "\", " +
                         "\"contentTitle\":\"" + contentTitle + "\", " +
                         "\"message\": \"" + contentTitle + "\"}}";*/
            
            var postData = new
            {
                to = "/topics/userBroadcast",
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = contentTitle,
                    title = "Messages from HR",
                    badge = 1
                },
            };

            string postbody = JsonConvert.SerializeObject(postData).ToString();
            string response = await SendGCMNotification(Google_App_ID,Sender_ID, postbody);

            
            

        }

        private async Task<string> SendGCMNotification(string apiKey, string sender,string postData, string postDataContentType = "application/json")
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);

            //  
            //  MESSAGE CONTENT  
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            //  
            //  CREATE REQUEST  
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            Request.Method = "POST";
            //  Request.KeepAlive = false;  

            Request.ContentType = postDataContentType;
            Request.Headers.Add(HttpRequestHeader.Authorization, string.Format("key={0}", apiKey));
            //Request.Headers.Add(string.Format(""));
            Request.Headers.Add(string.Format("Sender: id={0}", sender));
            Request.ContentLength = byteArray.Length;

            Stream dataStream =await Request.GetRequestStreamAsync();
            await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //  
            //  SEND MESSAGE  
            try
            {
              //  Debug.Print(Request.ToString()+"-----"+Request.GetResponse());
                WebResponse Response = await Request.GetResponseAsync();
                
                HttpWebResponse HttpResponse = ((HttpWebResponse)Response);
                if (HttpResponse.StatusCode.Equals(HttpStatusCode.Unauthorized) || HttpResponse.StatusCode.Equals(HttpStatusCode.Forbidden))
                {
                    
                    var text = "Unauthorized - need new token";
                    Debug.Print(text);
                }
                else if (!HttpResponse.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var text = "Response from web service isn't OK";
                    Debug.Print(text);
                }
                else if(HttpResponse.StatusCode.Equals((HttpStatusCode.OK)))
                {
                    var text = "status ok";
                    Debug.Print(HttpResponse.StatusDescription+"-----statusdesc------"+HttpResponse.ResponseUri);
                }

                StreamReader Reader = new StreamReader(Response.GetResponseStream());
                string responseLine = await Reader.ReadToEndAsync();
                Reader.Close();

                return responseLine;
            }
            catch (WebException e)
            {
                Debug.Print(e.Status+"-----status----"+e.Message+"--------"+e.Response);
            }
            return "error";
        }


        public static bool ValidateServerCertificate(
                                                     object sender,
                                                     X509Certificate certificate,
                                                     X509Chain chain,
                                                     SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


    }
}