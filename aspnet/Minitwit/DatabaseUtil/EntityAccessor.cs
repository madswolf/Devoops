using Minitwit.Models.Context;
using Minitwit.Models.Entity;

namespace Minitwit.DatabaseUtil
{
    public class EntityAccessor
    {
        private readonly MinitwitContext _context;

        public EntityAccessor(MinitwitContext context)
        {
            _context = context;
        }

        public List<Message> GetMessagesByAuthorId(int id)
        {
            return _context.Posts.Where(m => m.Author.Id == id && !m.Flagged).ToList();
        }

        public User GetUserById(int id)
        {
            return _context.Users.Where(u => u.Id == id).First();
        }

        public List<Message> GetMessages()
        {
            return _context.Posts.Where(m => !m.Flagged).ToList();
        }

        public List<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        public void InsertUser(User user)
        {
            _context.Users.Add(user);
        }

        public void InsertMessage(Message message)
        {
            _context.Posts.Add(message);
        }

        // Might not be a necessary method, since GetUserById already exists.
        public void AFollowB(int idA, int idB)
        {
            var uA = GetUserById(idA);
            var uB = GetUserById(idB);

            uA.Follows.Add(uB);
            uB.Follows.Add(uA);
        }


    }
}
