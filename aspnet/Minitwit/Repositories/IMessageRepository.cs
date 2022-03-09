using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Repositories
{
    public interface IMessageRepository
    {
        public Task<Message?> GetMessage(int id);
        public Task<List<Message>> GetPrivateTimeline(int id, List<int> follows, int limit = 30);
        public Task<List<Message>> GetMessagesByAuthorId(int id, int limit = 30);
        public Task<List<Message>> GetMessages(int limit = 30);

        public Task<List<Message>> GetFilteredMessages(int limit = 100);
        public Task InsertMessage(Message message);
        public Task FlagMessage(int messageId, bool flagged);
    }
}
