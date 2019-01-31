using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity.EntityFramework;
namespace icrm.Models
{
    using System.Threading.Tasks;
    public class MessageHub : Hub
    {
        private int? msgId = null;

        //readonly  static Dictionary<string,string> connections =  new Dictionary<string,string>();
        ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> userManager;

        public MessageHub()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        public void OnMessageNotified(object o,MessageEventArgs messageEventArgs)
        {
            if(this.msgId != messageEventArgs.message.Id) { 
            //Debug.Print("----in msg hub-----");
            Debug.Print(messageEventArgs.message.Text+"---=--===in m hub-=-=-==--="+messageEventArgs.message.Sender.UserName);
            var messageHub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            ApplicationUser user = messageEventArgs.message.Reciever;
            Debug.Print("user id------"+user.UserName+"---------"+ FindConnectionIdOnUsername(user.UserName)+"-------roles is==="+ userManager.IsInRole(user.Id, "User"));

                if (userManager.IsInRole(user.Id,"User"))
                {
                   messageHub.Clients.Client(FindConnectionIdOnUsername(user.UserName)).recieve(messageEventArgs.message);
                }
                else
                 {
                    messageHub.Clients.User(user.UserName).recieve(messageEventArgs.message);
                }
                this.msgId = messageEventArgs.message.Id;  
            }
        }

        public void NotifyHRAboutChat(Message message)
        {
            var messageHub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            ApplicationUser user = message.Reciever;
            messageHub.Clients.User(user.UserName).notification(message);
        }

        public void userClosedChat(string username)
        {
            var messageHub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            string message = "User has closed the chat";
            messageHub.Clients.User(username).userclosedchat(message);
        }

        public override Task OnConnected()
        {
            IRequest request = Context.Request;
            Debug.Print(request.Headers.Get("EmployeeId") + "--employee id");
            var username = request.Headers.Get("EmployeeId") != null
                ? request.Headers.Get("EmployeeId")
                : Context.User.Identity.Name;

            //Context.User.Identity.Name = username;
                AddOrUpdateHubConnection(new HubConnectionMap(){UserName = username,ConnectionId = Context.ConnectionId});

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            IRequest request = Context.Request;
            Debug.Print(request.Headers.Get("EmployeeId") + "--employee id");
            var username = request.Headers.Get("EmployeeId") != null
                ? request.Headers.Get("EmployeeId")
                : Context.User.Identity.Name;

            //Context.User.Identity.Name = username;
            AddOrUpdateHubConnection(new HubConnectionMap() { UserName = username, ConnectionId = Context.ConnectionId });

            return base.OnReconnected();
        }

        public void AddOrUpdateHubConnection(HubConnectionMap hubConnectionMap)
        {
            HubConnectionMap hcp = db.HubConnectionMap.Where(hc => hc.UserName == hubConnectionMap.UserName)
                .FirstOrDefault();
            if ( hcp == null)
            {
                db.HubConnectionMap.Add(hubConnectionMap);
            }
            else
            {
                hcp.ConnectionId = hubConnectionMap.ConnectionId;
                db.Entry(hcp).State = EntityState.Modified;
                
            }

            db.SaveChanges();
        }

        public string FindConnectionIdOnUsername(string username)
        {
            HubConnectionMap hbc =db.HubConnectionMap.Where(hc => hc.UserName == username)
                .FirstOrDefault();
            Debug.Print(hbc.ConnectionId+"------con on usname");
            return hbc.ConnectionId;
        }
    }
}