using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RabbitMQ.Client;
using RabbitMQ.Util;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using Constants = icrm.Models.Constants;

namespace icrm.Controllers
{
    using icrm.Events;
    using System.Web.Routing;

    [Authorize]
    public class ChatController : Controller
    {
        private ApplicationUserManager _userManager;
        private UserInterface userService;
        private ChatInterface chatService;
        private MessageInterface messageService;
        private ChatRequestInterface chatRequestService;
        private EventService eventService;
        public ChatController()
        {
            chatRequestService = new ChatRequestRepository();
            userService = new UserRepository();
            chatService = new ChatRepository();
            messageService = new MessageRepository();
            this.eventService = new EventService();
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
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (Session["user"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
            }
            base.OnActionExecuting(filterContext);
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

            ViewBag.Messages = messages;
            return View();
        }


        [HttpPost]
        public JsonResult sendmsg(string text, int? chatId)
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

            //Producer producer = new Producer("messageexchange",ExchangeType.Direct); 
            // Debug.Print(con + "-----rec-----"+ reciever.UserName +"-----sen-----"+sender.UserName+"---msg------"+text+"----");
            /*int? chatId2 = chatService.getChatIdOfUsers(sender, reciever);
            if (chatId2 == null)
            {
                Chat chat = new Chat();
                chat.UserOneId = sender.Id;
                chat.UserTwoId = reciever.Id;
                chatId2 = chatService.Save(chat);
            }*/

            /*Message message = new Message();
            message.Text = text;
            message.SenderId = sender.Id;
            message.RecieverId = reciever.Id;
            message.SentTime = DateTime.Now;
            message.ChatId = chatId;
            Message msgWithId = messageService.Save(message);
            Debug.Print((msgWithId)+"----msgwitrhid");
            Debug.Print(msgWithId.Id+"----mdgid>><<<<<"+msgWithId.Reciever+"-----reciever");
            bool flag = producer.send(msgWithId);*/
            Message message = SendMessage(text, chatId, sender, reciever);
            return Json(message);
        }
        [HttpGet]
        public JsonResult receive()
        {
            try
            {

                /*RabbitMQBll obj = new RabbitMQBll();
                IConnection con = obj.GetConnection();
                ApplicationUser userqueue = (ApplicationUser) Session["user"];
                Message message= obj.receive(con, userqueue.UserName);*/
                /*if (message != null)
                {
                    Debug.Print("returned message----=" + message.Text);

                }*/



                return Json("", JsonRequestBehavior.AllowGet);
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
        public JsonResult getPagedMessages(int chatId, int Page)
        {
            return Json(messageService.getPagedMessages(chatId, Page), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("chat/close/{activeUser}")]
        public JsonResult CloseChat(string activeUser)
        {
            Debug.Print(activeUser + "-----------active user");
            ApplicationUser user1 = (ApplicationUser)Session["user"];
            ApplicationUser user2 = userService.findUserOnId(user1.Id);
            if (!activeUser.IsNullOrWhiteSpace())
            {
                Debug.Print(user2.Id+"----active user not null chat "+activeUser);
                int? chatId = chatService.getChatIdOfUsers(activeUser, user2.Id);
                Debug.Print("chat id is----"+chatId);
                if (chatId != null)
                {

                    chatService.changeActiveStatus(chatId, false);
                }

                eventService.hrClosedChat(userService.findUserOnId(activeUser).UserName);
            }

            if (this.chatRequestService.ChatRequestsSize() > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                user2.available = true;
                this.userService.Update(user2);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        [Route("chat/nextrequest")]
        public void getNextChatRequestForHR()
        {
            Thread.Sleep(2000);
            this.AssignNextRequestInQueueToHr();
        }

        [HttpGet]
        [Route("chat/window/close")]
        public void HrLeftChatWindow()
        {
            Debug.Print("disposing consumer----------here");
            Consumer consumer = (Consumer)Session["consumer"];
            consumer.Dispose();
        }

        [HttpGet]
        [Route("chat/hr/available/{value}")]
        public void changeHrAvailabilityStatus(bool value)
        {
            ApplicationUser hr = this.userService.findUserOnId(User.Identity.GetUserId());
            Debug.Print(value + "---------value");
            hr.available = value;
            this.userService.Update(hr);
            if (value)
            {
                if (this.chatRequestService.ChatRequestsSize() > 0)
                    this.AssignNextRequestInQueueToHr();
            }
        }

        [HttpGet]
        [Route("chat/hr/checkavailable")]
        public bool checkHrAvailabilityStatus()
        {
            ApplicationUser hr = this.userService.findUserOnId(User.Identity.GetUserId());
            return (bool)hr.available;
        }

        [HttpGet]
        [Route("chat/startconsumer")]
        public void startConsumer()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            Debug.Print("starting consumer");
            Consumer consumer;
            if (Session["consumer"] == null)
            {
                Debug.Print("new consumer");
                consumer = new Consumer("messageexchange", ExchangeType.Direct);
                Session["consumer"] = consumer;

            }
            else
            {
                Debug.Print("existing consumer");

                consumer = (Consumer)Session["consumer"];
            }
            if (consumer.ConnectToRabbitMQ())
                consumer.StartConsuming(user.UserName);
        }

        [HttpGet]
        [Route("test")]
        public void testapp()
        {
            HttpContext.Application["date"] = DateTime.Now;
            Debug.Print("sett the date in api---");
        }

        public Message SendMessage(string text, int? chatId, ApplicationUser sender, ApplicationUser reciever)
        {
            Debug.Print("sending message" + text + "---recirever---" + reciever.UserName + "---chatid------" + chatId);
            Producer producer = new Producer("messageexchange", ExchangeType.Direct);
            Message message = new Message();
            message.Text = text;
            message.SenderId = sender.Id;
            message.RecieverId = reciever.Id;
            message.SentTime = DateTime.Now;
            message.ChatId = chatId;
            Message msgWithId = messageService.Save(message);
            // Debug.Print((msgWithId) + "----msgwitrhid");
            // Debug.Print(msgWithId.Id + "----mdgid>><<<<<" + msgWithId.Reciever + "-----reciever");
            if (producer.ConnectToRabbitMQ())
                producer.send(msgWithId);
            return msgWithId;
        }


        public void AssignNextRequestInQueueToHr()
        {
            HttpContext.Application["Time"] = DateTime.Now;
            Debug.Print("in hr get next request---");
            ChatRequest chatRequest = this.chatRequestService.NextChatRequestInQueue();
            ApplicationUser reciever = UserManager.FindById(User.Identity.GetUserId());
            ApplicationUser sender = UserManager.FindById(chatRequest.UserId);
            Debug.Print(chatRequest + "------cgat rwq");
            Producer producer = new Producer("messageexchange", ExchangeType.Direct);

            if (chatRequest != null)
            {
                int messageSize = this.messageService.GetMessageSizeOfChatRequestUser(chatRequest.UserId);
                this.chatRequestService.delete(chatRequest);
                int? chatId2 = null;
                chatId2 = chatService.getChatIdOfUsers(sender.Id, reciever.Id);
                Debug.Print("Chat id is  " + chatId2);
                List<Message> messages = new List<Message>();
                if (chatId2 == null)
                {
                    Debug.Print("Chat id 2 is null but how--" + chatId2);
                    Chat chat = new Chat();
                    chat.UserOneId = sender.Id;
                    chat.UserTwoId = reciever.Id;
                    chat.active = true;
                    chatId2 = chatService.Save(chat);
                }
                else
                {
                    this.chatService.changeActiveStatus(chatId2, true);
                }
                if (messageSize > 0)
                {
                    messages = messageService.GetMessagesOfChatRequestUser(sender.Id, reciever.Id, chatId2);


                }
                else
                {
                    Message message = new Message();
                    message.Text = "Hello Agent,I want to chat with you";
                    message.RecieverId = reciever.Id;
                    message.SenderId = sender.Id;
                    message.SentTime = DateTime.Now;
                    message.ChatId = chatId2;
                    message = messageService.Save(message);
                    messages.Add(message);
                }
                foreach (Message msgWithId in messages)
                {
                    if (producer.ConnectToRabbitMQ())
                    {
                        producer.send(msgWithId);
                        this.eventService.NotifyHrAboutChat(msgWithId);
                    }
                }
                this.eventService.hrAvailableNotification(sender.DeviceCode);
            }
        }
    }

}

