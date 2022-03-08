using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;


namespace Minitwit.DatabaseUtil
{
    public class UserRepository : IUserRepository
    {
        private readonly MinitwitContext _context;

        public UserRepository(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<List<User>?> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async void InsertUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Follow>?> GetUserFollows(int id)
        {
            return await _context.Users
                .Include(u => u.Follows)
                .Where(u => u.Id == id)
                .SelectMany(u => u.Follows)
                .ToListAsync();
        }

        public async Task<List<Follow>?> GetUserFollowedBy(int id)
        {
            return await _context.Users
                .Include(u => u.FollowedBy)
                .Where(u => u.Id == id)
                .SelectMany(u => u.FollowedBy)
                .ToListAsync();
        }
        
        public async Task<Follow?> GetFollow(int followerId, int followeeId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }

        public async Task Follow(int followerId, int followeeId)
        {
            _context.Follows.Add(new Follow()
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
            });
            await _context.SaveChangesAsync();
        }

        public async Task Unfollow(Follow follow)
        {
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
        }
    }
}
