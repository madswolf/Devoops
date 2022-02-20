using Minitwit.Models.Entity;

namespace Minitwit.DatabaseUtil
{
    public interface IEntityAccessor
    {
        public List<Message> GetMessagesByAuthorId(int id);
        public Task<User?> GetUserByUsername(string username);
        public Task<User?> GetUserById(int id);
        public List<Message> GetMessages();
        public List<User> GetUsers();
        public void InsertUser(User user);
        public void InsertMessage(Message message);
    }
}
