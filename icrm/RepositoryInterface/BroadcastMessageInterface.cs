using System.Collections.Generic;
using icrm.Models;

namespace icrm.RepositoryInterface
{
    public interface BroadcastMessageInterface
    {
        bool Save(BroadcastMessage broadcastMessage);
        List<BroadcastMessage> FindAll();
        BroadcastMessage FindOne(int? id);
    }
}