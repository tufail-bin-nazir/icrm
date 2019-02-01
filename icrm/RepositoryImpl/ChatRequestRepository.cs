using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using icrm.Models;
using icrm.RepositoryInterface;

namespace icrm.RepositoryImpl
{
    public class ChatRequestRepository:ChatRequestInterface
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public int ChatRequestsSize()
        {
          return  db.ChatRequest.ToList().Count();
        }

        public void Save(ChatRequest chatRequest)
        {
            db.ChatRequest.Add(chatRequest);
            db.SaveChanges();
        }

        public bool CheckRequestExistsOfUser(string userId)
        {
          ChatRequest chatRequest =  db.ChatRequest.Where(c => c.UserId == userId).SingleOrDefault();
            if (chatRequest == null)
            {
                return false;
            }

            return true;
        }
    }
}