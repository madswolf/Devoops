using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using Prometheus;


namespace Minitwit.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MinitwitContext _context;
        private readonly ILogger<UserRepository> _logger;


        private static readonly Gauge getUserByIdTime = Metrics.CreateGauge("getuserbyid_time_s", "Time of GetUserById()");
        private static readonly Gauge getUserByUsernameTime = Metrics.CreateGauge("getuserbyusername_time_s", "Time of GetUserByUsername()");
        private static readonly Gauge getUsersTime = Metrics.CreateGauge("getusers_time_s", "Time of GetUsers()");
        private static readonly Gauge insertUserTime = Metrics.CreateGauge("insertuser_time_s", "Time of InsertUser()");
        private static readonly Gauge getUserFollowsTime = Metrics.CreateGauge("getuserfollows_time_s", "Time of GetUserFollows()");
        private static readonly Gauge getFilteredUserFollowsTime = Metrics.CreateGauge("getfiltereduserfollows_time_s", "Time of GetFilteredUserFollows()");
        private static readonly Gauge getUserFollowedByTime = Metrics.CreateGauge("getuserfollowedby_time_s", "Time of GetUserFollowedBy()");
        private static readonly Gauge getFollowTime = Metrics.CreateGauge("getfollow_time_s", "Time of GetFollow()");
        private static readonly Gauge followTime = Metrics.CreateGauge("follow_time_s", "Time of Follow()");
        private static readonly Gauge unfollowTime = Metrics.CreateGauge("unfollow_time_s", "Time of Unfollow()");


        public UserRepository(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserById(int id)
        {
            _logger.LogDebug($"Called GetUserById() with arguments {id}");

            using (getUserByIdTime.NewTimer())
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            _logger.LogDebug($"Called GetUserById() with arguments {username}");

            using (getUserByUsernameTime.NewTimer())
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == username);
            }
        }

        public async Task<List<User>?> GetUsers()
        {
            _logger.LogDebug($"Called GetUsers()");

            using (getUsersTime.NewTimer())
            {
                return await _context.Users.ToListAsync();
            }
        }

        public async void InsertUser(User user)
        {
            _logger.LogDebug($"Called InsertUser() {user}");

            using (insertUserTime.NewTimer())
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Inserted new user {user}");
            }
        }

        public async Task<List<Follow>?> GetUserFollows(int id)
        {
            _logger.LogDebug($"Called GetUserFollows() {id}");

            using (getUserFollowsTime.NewTimer())
            {
                return await _context.Users
                    .Include(u => u.Follows)
                    .Where(u => u.Id == id)
                    .SelectMany(u => u.Follows)
                    .ToListAsync();
            }
        }

        public async Task<FilteredFollowDTO?> GetFilteredFollows(string username, int limit = 100)
        {
            _logger.LogDebug($"Called GetFilteredFollows() {username}, {limit}");

            using (getFilteredUserFollowsTime.NewTimer())
            {
                return await _context.Users
                    .Include(u => u.Follows)
                    .Where(u => u.UserName == username)
                    .Select(u => new FilteredFollowDTO
                    {
                        follows = u.Follows
                            .Join(
                                _context.Users,
                                f => f.FolloweeId,
                                u => u.Id,
                                (f, u) => u
                            )
                            .Select(u2 => u2.UserName)
                            .Take(limit)
                    })
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<List<Follow>?> GetUserFollowedBy(int id)
        {
            _logger.LogDebug($"Called GetUserFollowedBy() {id}");

            using (getUserFollowedByTime.NewTimer())
            {
                return await _context.Users
                    .Include(u => u.FollowedBy)
                    .Where(u => u.Id == id)
                    .SelectMany(u => u.FollowedBy)
                    .ToListAsync();
            }
        }
        
        public async Task<Follow?> GetFollow(int followerId, int followeeId)
        {
            _logger.LogDebug($"Called GetFollow() {followerId}, {followeeId}");

            using (getFollowTime.NewTimer())
            {
                return await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
            }
        }

        public async Task Follow(int followerId, int followeeId)
        {
            _logger.LogDebug($"Called Follow() {followerId}, {followeeId}");

            using (followTime.NewTimer())
            {
                _context.Follows.Add(new Follow()
                {
                    FollowerId = followerId,
                    FolloweeId = followeeId,
                });
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added {followerId} as follower of {followeeId}");
            }
        }

        public async Task Unfollow(Follow follow)
        {
            _logger.LogDebug($"Called UnFollow() {follow}");


            using (unfollowTime.NewTimer())
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Removed {follow.Follower} as follower of {follow.Followee}");
            }
        }
    }
}
