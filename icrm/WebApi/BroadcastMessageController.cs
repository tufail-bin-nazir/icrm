using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using icrm.Events;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity.Owin;

namespace icrm.WebApi
{
    public class BroadcastMessageController : ApiController
    {
        private BroadcastMessageInterface broadcastMessageService;
        private BroadcastMessageEvent broadcastEvent;
        private Notification notification;
        private UserInterface userService;
        public BroadcastMessageController()
        {
            broadcastEvent = new BroadcastMessageEvent();
            notification = new Notification();
            broadcastMessageService = new BroadcastMessageRepository();
            userService = new UserRepository();
            
        }

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
        [HttpPost]
        [Route("api/broadcast/message")]
        public IHttpActionResult send([FromBody] BroadcastMessage broadcastMessage)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;

            broadcastMessage.SenderId=sender.Id;
            broadcastMessage.SentTime=DateTime.Now;
            if (broadcastMessageService.Save(broadcastMessage))
            {
                Debug.Print("sending messages after saving---------");
                sendMessage(broadcastMessage);
                Debug.Print("after sending-------");
            }

            return Ok();
        }

         async void sendMessage(BroadcastMessage broadcastMessage)
        {
            broadcastEvent.MessageBroadcasted += notification.OnMessageBroadcasted;
            broadcastEvent.broadcast(broadcastMessage);
        }
    }
}
