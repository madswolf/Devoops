using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.DatabaseUtil
{
    public interface IMessageRepository
    {
        public Task<Message?> GetMessage(int id);
        public Task<List<Message>> GetPrivateTimeline(int id, List<int> follows, int limit = 30);
        public Task<List<Message>> GetMessagesByAuthorId(int id, int limit = 30);
        public Task<List<Message>> GetMessages(int limit = 30);
        public Task InsertMessage(Message message);
        public Task FlagMessage(int messageId, bool flagged);
    }
}
