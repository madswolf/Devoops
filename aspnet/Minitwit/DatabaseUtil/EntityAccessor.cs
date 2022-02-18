using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.DatabaseUtil
{
    public class EntityAccessor : IEntityAccessor
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

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
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
    }
}
