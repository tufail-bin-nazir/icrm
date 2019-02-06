using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using icrm.Events;
using icrm.Models;
using icrm.RepositoryInterface;
using PagedList;

namespace icrm.RepositoryImpl
{
    using System.Data.Entity;

    public class MessageRepository : MessageInterface
    {
        ApplicationDbContext db = new ApplicationDbContext();
         EventService eventService = new EventService();
        public Message Save(Message message)
        {
            Debug.Print(message.SentTime+"---------sent time");
            db.Message.Add(message);
            db.SaveChanges();
           return db.Message.Include("Sender").Include("Reciever").Where(m => m.Id == message.Id)
                .FirstOrDefault();
            
        }

        public List<Message> GetMessagesOnIds(ArrayList idList)
        {
            List<Message> messages = new List<Message>();
            foreach (var i in idList)
            {
               Message message =  db.Message.SingleOrDefault(m => m.Id == (int)i);
                message.RecieveTime = DateTime.Now;
                db.Message.Add(message);
               messages.Add(message); 
            }

            return messages;
        }

        public Message updateMessage(Message message)
        {
            using (var context = new ApplicationDbContext() )
            {
                Message message2 = context.Message.Find(message.Id);
                message2.RecieverId = message.RecieverId;
                message2.ChatId = message.ChatId;
                context.Entry(message2).State = EntityState.Modified;
                context.SaveChanges();
                return context.Message.Include("Chat").Include("Reciever").Include("Chat").Where(m => m.Id == message.Id)
                    .FirstOrDefault();
            }
            
        }

        public Message UpdateRecieveTimeOfMessage(int id)
        {
            using (var context = new ApplicationDbContext() )
            {
                Message message = context.Message.Include("Chat").FirstOrDefault(m => m.Id == id);

                message.RecieveTime = DateTime.Now;
                context.Entry(message).State = EntityState.Modified;
                context.SaveChanges();

                ApplicationUser msgsender = new ApplicationUser();
                ApplicationUser msgreciever = new ApplicationUser();

                ApplicationUser sender = context.Users.Find(message.SenderId);
                msgsender.Id = sender.Id;
                msgsender.FirstName = sender.FirstName;
                msgsender.LastName = sender.LastName;
                msgsender.UserName = sender.UserName;


                ApplicationUser reciever = context.Users.Find(message.RecieverId);
                msgreciever.Id = reciever.Id;
                msgreciever.FirstName = reciever.FirstName;
                msgreciever.LastName = reciever.LastName;
                msgreciever.UserName = reciever.UserName;
                msgreciever.Roles.Add(reciever.Roles.FirstOrDefault());

                message.Sender = msgsender;
                message.Reciever = msgreciever;

                eventService.pushMessage(message);
                return message;
            }
        }

        public List<Message> getChatListOfHrWithLastMessage(string id)
        {
            List<Message> messages = db.Message.Include("Chat").GroupBy(m => m.ChatId).Select(m=>m.Where(x => (x.Chat.UserOneId == id || x.Chat.UserTwoId == id)&& x.RecieveTime != null).OrderByDescending(x=>x.Id).FirstOrDefault()).ToList();
            

            return messages;
        }

        public IPagedList<Message> getPagedMessages(int chatId, int Page)
        {
            List<Message> messages = db.Message.Where(m => m.ChatId == chatId).OrderByDescending(m=>m.RecieveTime).ToList();
            return messages.ToPagedList(Page, 10);
        }

        public int GetMessageSizeOfChatRequestUser(string userId)
        {
            Debug.Print(userId+"-------------userID");
            return this.db.Message.Where(m => m.ChatId == null && m.SenderId == userId ).ToList().Count;
        }

        public List<Message> GetMessagesOfChatRequestUser(string userId, string recieverId, int? chatId)
        {
            var messages = this.db.Message.Include("Chat").Where(m => m.ChatId == null && m.SenderId == userId).ToList();
            messages.ForEach(m =>
            {
                m.ChatId = chatId;
                m.RecieverId = recieverId;
            });
            db.SaveChanges();
            return messages;
        }
        public dynamic GetMessagesOfUser(string id)
        {
            var messages = this.db.Message.Where(m => m.SenderId == id || m.RecieverId == id).Select(m=>new{m.Text,m.Sender.EmployeeId}).ToList();
                        return messages;
        }
    }
}
