using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity.Owin;
using RabbitMQ.Client;

namespace icrm.WebApi
{
    [Authorize]
    public class ChatApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        private UserInterface userService;
        private ChatInterface chatService;
        private MessageInterface messageService;
        private ChatRequestInterface chatRequestService;

        public ChatApiController()
        {
            userService = new UserRepository();
            chatService = new ChatRepository();
            messageService = new MessageRepository();
            chatRequestService = new ChatRequestRepository();
            
        }

        [HttpPost]
        [Route("api/chat/send")]
        public IHttpActionResult sendmsg([FromBody]Message message)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;
            ApplicationUser reciever = new ApplicationUser();
           // Debug.Print("--------"+chatViewModel.Reciever+"------");

            Debug.Print(message.ChatId+"--------chat id by mudassir-----"+message.Text);
            if (message.ChatId == 0)
            {
                Debug.Print("Chat Id is null");
                reciever = userService.GetAllAvailableUsers(Constants.ROLE_HR).FirstOrDefault();
                if (reciever != null)
                {
                    reciever.available = false;
                    userService.Update(reciever);
                }
                else
                {
                    ChatRequest chatRequest = new ChatRequest();
                    chatRequest.UserId = sender.Id;
                    chatRequestService.Save(chatRequest);
                    return BadRequest();
                }

                Debug.Print("So reciever is+++"+reciever.UserName);
            }
              else
            {
                reciever = userService.findUserOnId(message.RecieverId);
            }


            RabbitMQBll obj = new RabbitMQBll();
            IConnection con = obj.GetConnection();
            int? chatId2 = chatService.getChatIdOfUsers(sender.Id, reciever.Id);
            Debug.Print("Chat id is  "+chatId2);
            if (chatId2 == null)
            {
                Debug.Print("Chat id 2 is null but how--"+chatId2);
                Chat chat = new Chat();
                chat.UserOneId = sender.Id;
                chat.UserTwoId = reciever.Id;
                chatId2 = chatService.Save(chat);
            }

            
            
            message.SenderId = sender.Id;
            if (message.RecieverId.IsNullOrWhiteSpace())
            {
                message.RecieverId = reciever.Id;
            }

            message.SentTime = DateTime.Now;
            message.ChatId = chatId2;
            Message msgWithId = messageService.Save(message);
            Debug.Print((msgWithId) + "----msgwitrhid");
            Debug.Print(msgWithId.Id + "----mdgid>><<<<<" + msgWithId.Reciever + "-----reciever");
            //Debug.Print("---here--------"+chatViewModel.ToString());
            bool flag = obj.send(con, msgWithId);

            return Ok(msgWithId);
        }

        [HttpGet]
        [Route("api/chat/recieve")]
        public IHttpActionResult recievemsg()
        {
            try
            {
                var Name1 = User.Identity.Name;
                Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
                ApplicationUser reciever = user.Result;
                Debug.Print("-----------recieber-----");
                RabbitMQBll obj = new RabbitMQBll();
                IConnection con = obj.GetConnection();
                Message message = obj.receive(con, reciever.UserName);
 


                return Ok(message);
            }
            catch (Exception e)
            {
                Debug.Print("======excepiton");
                Debug.Print(e.StackTrace);
                return null;
            }

        }

        [HttpGet]
        [Route("api/chat/hr/available")]
        public string checkHrAvailability()
        {
            ApplicationUser reciever = userService.GetAllAvailableUsers(Constants.ROLE_HR).FirstOrDefault();
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;
            int chatRequestCount = chatRequestService.ChatRequestsSize();
            if (reciever == null)
            {
                if (!chatRequestService.CheckRequestExistsOfUser(sender.Id))
                {
                    ChatRequest chatRequest = new ChatRequest();
                    chatRequest.UserId = sender.Id;
                    chatRequestService.Save(chatRequest);
                }

                return "false";
            }

            return "true";
        }

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

    }
}
