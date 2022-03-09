using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserById(int id);

        public Task<User?> GetUserByUsername(string username);

        public Task<List<User>?> GetUsers();

        public void InsertUser(User user);

        public Task<List<Follow>?> GetUserFollows(int id);

        public Task<FilteredFollowDTO?> GetFilteredFollows(string username, int limit = 100);

        public Task<List<Follow>?> GetUserFollowedBy(int id);

        public Task<Follow?> GetFollow(int followerId, int followeeId);

        public Task Follow(int followerId, int followeeId);

        public Task Unfollow(Follow follow);
    }
}
