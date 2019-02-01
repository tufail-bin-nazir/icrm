using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
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
        private EventService eventService;
        public BroadcastMessageController()
        {
            broadcastMessageService = new BroadcastMessageRepository();
            eventService = new EventService();
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
        public  IHttpActionResult send([FromBody] BroadcastMessage broadcastMessage)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;

            broadcastMessage.SenderId=sender.Id;
            broadcastMessage.SentTime=DateTime.Now;
            if (broadcastMessageService.Save(broadcastMessage))
            {
                 eventService.sendmessage(broadcastMessage);
            }

            return Ok();
        }
        [HttpGet]
        [Route("api/broadcast/message/all")]
        public IHttpActionResult FindAll()
        {
            List<BroadcastMessage> broadcastMessages = broadcastMessageService.FindAll();
            foreach (var VARIABLE in broadcastMessages)
            {
                Debug.Print("--"+VARIABLE.Text+"-----"+VARIABLE.SentTime);
            }
            return Ok(broadcastMessageService.FindAll());
        }

        [HttpGet]
        [Route("api/broadcast/message/{id}")]
        public IHttpActionResult FindOne(int? id)
        {
            return Ok(broadcastMessageService.FindOne(id));
        }

    }
}
