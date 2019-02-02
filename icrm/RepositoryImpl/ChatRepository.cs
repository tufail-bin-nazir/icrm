using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using icrm.Models;
using icrm.RepositoryInterface;

namespace icrm.RepositoryImpl
{
    public class ChatRepository : ChatInterface          
    {

        ApplicationDbContext db = new ApplicationDbContext();

        public int? getChatIdOfUsers(string userOne, string userTwo)
        {
             Chat chat = db.Chat.Where(c => (c.UserOneId == userOne && c.UserTwoId == userTwo) || (c.UserOneId == userTwo && c.UserTwoId == userOne)).FirstOrDefault();
            if (chat != null)
            {
                return chat.Id;
            }

            return null;
        }

        public List<Chat> getChatsOfHr(string id)
        {
            return db.Chat.Include("UserOne").Include("UserTwo").Where(c => c.UserOneId == id || c.UserTwoId == id).ToList();
        }

        public ApplicationUser GetUserFromChatIdOtherThanPassedUser(int? chatId, string userId)
        {
            Chat chat = db.Chat.Include("UserOne").Include("UserTwo").SingleOrDefault(c=>c.Id ==chatId);
            if (chat.UserOneId == userId)
                return chat.UserTwo;

            return chat.UserOne;
        }

        public int Save(Chat chat)
        {
            db.Chat.Add(chat);
            db.SaveChanges();
            return chat.Id;
        }

        public bool IsActive(int? chatId)
        {
            if(chatId>0)
            return this.db.Chat.FirstOrDefault(c => c.Id == chatId).active;

            return false;
        }

        public void changeActiveStatus(int? chatId,bool value)
        {
            Chat chat = this.db.Chat.FirstOrDefault(c => c.Id == chatId);
            chat.active = value;
            this.db.SaveChanges();
        }
    }
}