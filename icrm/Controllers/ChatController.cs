using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RabbitMQ.Client;
using RabbitMQ.Util;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using Constants = icrm.Models.Constants;

namespace icrm.Controllers
{
    public class ChatController : Controller
    {
        private ApplicationUserManager _userManager;
        private UserInterface userService;
        private ChatInterface chatService;
        private MessageInterface messageService;

        public ChatController()
        {
          
            userService = new UserRepository();
            chatService = new ChatRepository();
            messageService = new MessageRepository();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Chat
        public ActionResult Index()
        {
     
            
            return View();
        }

        public ActionResult HRChat()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            List<Message> messages = messageService.getChatListOfHrWithLastMessage(user.Id);
            Debug.Print("---heklololddksnk-------------");
            Debug.Print(messages.Count + "-----COunt--++---");
            foreach (var message in messages)
            {
               Debug.Print(message.Chat.UserOne.UserName + "------qwuwe------" + message.Chat.UserTwo.UserName + "---userone----" + message.Text + "----chat2---"+message.RecieveTime);
            }
            ViewBag.Messages = messages;
            return View();
        }


        [HttpPost]
        public JsonResult sendmsg(string text,int? chatId)
        {
            ApplicationUser sender = UserManager.FindById(User.Identity.GetUserId());
            ApplicationUser reciever = chatService.GetUserFromChatIdOtherThanPassedUser(chatId, sender.Id);
            /*            if (chatId == null)
                        {
                             reciever = userService.GetAllAvailableUsers(Constants.ROLE_HR).FirstOrDefault();

                        }
                        else 
                        {
                            reciever = chatService.GetUserFromChatIdOtherThanPassedUser(chatId, sender.Id);
                        }*/
            //ChatViewModel chatViewModel = new ChatViewModel(){Text = text,Sender = sender.UserName,Reciever = reciever};

            RabbitMQBll obj = new RabbitMQBll();
            IConnection con = obj.GetConnection();
            Debug.Print(con + "-----rec-----"+ reciever.UserName +"-----sen-----"+sender.UserName+"---msg------"+text+"----");
            /*int? chatId2 = chatService.getChatIdOfUsers(sender, reciever);
            if (chatId2 == null)
            {
                Chat chat = new Chat();
                chat.UserOneId = sender.Id;
                chat.UserTwoId = reciever.Id;
                chatId2 = chatService.Save(chat);
            }*/

            Message message = new Message();
            message.Text = text;
            message.SenderId = sender.Id;
            message.RecieverId = reciever.Id;
            message.SentTime = DateTime.Now;
            message.ChatId = chatId;
            Message msgWithId = messageService.Save(message);
            Debug.Print((msgWithId)+"----msgwitrhid");
            Debug.Print(msgWithId.Id+"----mdgid>><<<<<"+msgWithId.Reciever+"-----reciever");
            bool flag = obj.send(con,msgWithId);
            
            return Json(msgWithId);
        }
        [HttpGet]
        public JsonResult receive()
        {
            try
            {

                RabbitMQBll obj = new RabbitMQBll();
                IConnection con = obj.GetConnection();
                ApplicationUser userqueue = (ApplicationUser) Session["user"];
                Message message= obj.receive(con, userqueue.UserName);
                /*if (message != null)
                {
                    Debug.Print("returned message----=" + message.Text);

                }*/

            

            return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.Print("======excepiton");
                Debug.Print(e.StackTrace);
                return null;
            }



        }

        [HttpGet]
        [Route("chat/{chatId}/messages/{page}")]
        public JsonResult getPagedMessages(int chatId,int Page)
        {
            return Json(messageService.getPagedMessages(chatId, Page),JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CloseChat()
        {
            ApplicationUser user = (ApplicationUser) Session["user"];
            Debug.Print(user.UserName+"--------here is user");
            user.available = true;
            if (userService.Update(user))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}