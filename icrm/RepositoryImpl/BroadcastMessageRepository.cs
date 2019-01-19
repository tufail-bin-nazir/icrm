using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using icrm.Models;
using icrm.RepositoryInterface;

namespace icrm.RepositoryImpl
{
    public class BroadcastMessageRepository:BroadcastMessageInterface
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public List<BroadcastMessage> FindAll()
        {
            return db.BroadcastMessage.OrderByDescending(b=>b.SentTime).ToList();
        }

        public BroadcastMessage FindOne(int? id)
        {
            return db.BroadcastMessage.Find(id);
        }

        public bool Save(BroadcastMessage broadcastMessage)
        {
            try
            {
                db.BroadcastMessage.Add(broadcastMessage);
                db.SaveChanges();
                return true;

            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                return false;
            }
        }

         
    }

}