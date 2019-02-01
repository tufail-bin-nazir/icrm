using System.Collections.Generic;
using icrm.Models;
using PagedList;

namespace icrm.RepositoryInterface
{
    public interface ChatInterface
    {
        int Save(Chat chat);
        int? getChatIdOfUsers(string userOne, string userTwo);
        List<Chat> getChatsOfHr(string id);
        ApplicationUser GetUserFromChatIdOtherThanPassedUser(int? chatId, string userId);
    }
}