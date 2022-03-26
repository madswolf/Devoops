using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;
using Minitwit.Repositories;
using Minitwit.Test.Context;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Minitwit.Test.Repositories
{
    public class UserRepositoryTest : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly MinitwitContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var builder = new DbContextOptionsBuilder<MinitwitContext>().UseSqlite(_connection);
            _context = new MinitwitTestContext(builder.Options);
            _context.Database.EnsureCreated();
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task Test_get_user_by_id()
        {
            var user = await _repository.GetUserById(1);
            Assert.Equal("user1", user.UserName);
        }

        [Fact]
        public async Task Test_get_user_by_username()
        {
            var user = await _repository.GetUserByUsername("user1");
            Assert.Equal(1, user.Id);
        }

        [Fact]
        public async Task Test_insert_user()
        {
            _repository.InsertUser(new User { Id = 5, UserName = "user5", Email = "user5@test.com" });
            var user = await _repository.GetUserById(5);
            Assert.Equal("user5", user.UserName);
        }

        [Fact]
        public async Task Test_get_all_users()
        {
            var userlist1 = await _repository.GetUsers();
            _repository.InsertUser(new User { Id = 6, UserName = "user6", Email = "user6@test.com" });
            var userlist2 = await _repository.GetUsers();
            Assert.True(userlist1.Count < userlist2.Count);
        }

        [Fact]
        public async Task Test_get_user_follows()
        {
            var follows = await _repository.GetUserFollows(1);
            Assert.Equal(3, follows.Count);
        }

        [Fact]
        public async Task Test_get_user_filtered_follows()
        {
            var follows = await _repository.GetFilteredFollows("user1");
            Assert.Equal(3, follows.follows.Count());
        }

        [Fact]
        public async Task Test_get_user_followed_by()
        {
            var follows = await _repository.GetUserFollowedBy(4);
            Assert.Equal(2, follows.Count);
        }

        [Fact]
        public async Task Test_get_follow()
        {
            var follow = await _repository.GetFollow(1,2);
            Assert.NotNull(follow);
        }

        [Fact]
        public async Task Test_add_follow()
        {
            await _repository.Follow(2, 1);
            var follow = await _repository.GetFollow(2, 1);
            Assert.NotNull(follow);
        }

        [Fact]
        public async Task Test_unfollow()
        {
            await _repository.Follow(4, 1);
            var follow = await _repository.GetFollow(4, 1);
            await _repository.Unfollow(follow);
            follow = await _repository.GetFollow(4, 1);
            Assert.Null(follow);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
