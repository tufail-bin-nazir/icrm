using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using icrm.Models;
using icrm.RepositoryInterface;
using PagedList;

namespace icrm.RepositoryImpl
{
    using System.Data.Entity;

    public class MessageRepository : MessageInterface
    {
        ApplicationDbContext db = new ApplicationDbContext();
         
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
            this.db.Entry(message).State = EntityState.Modified;
            this.db.SaveChanges();
            return message;
        }

        public Message UpdateRecieveTimeOfMessage(int id)
        {
            Message message = db.Message.FirstOrDefault(m => m.Id == id);
            message.RecieveTime = DateTime.Now;
            this.db.Entry(message).State = EntityState.Modified;
            db.SaveChanges();
            return message;
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

        public List<Message> GetMessagesOfChatRequestUser(string userId)
        {
            Debug.Print(userId+"-------------userID");
            return this.db.Message.Where(m => m.ChatId == null && m.SenderId == userId ).ToList();
        }
    }
}
