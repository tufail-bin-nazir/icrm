using icrm.Models;

namespace icrm.RepositoryInterface
{
    public interface ChatRequestInterface
    {
        void Save(ChatRequest chatRequest);
        int ChatRequestsSize();
        bool CheckRequestExistsOfUser(string userId);
        ChatRequest NextChatRequestInQueue();
        void delete(ChatRequest chatRequest);
    }
} 