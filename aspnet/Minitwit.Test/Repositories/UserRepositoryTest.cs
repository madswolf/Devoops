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
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

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
            _repository = new UserRepository(_context, new NullLogger<UserRepository>());
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GetUserById_THEN_ReturnUser()
        {
            var user = await _repository.GetUserById(1);
            user.UserName.Should().Be("user1");
        }

        [Fact]
        public async Task GIVEN_Username_WHEN_GetUserByUsername_THEN_ReturnUser()
        {
            var user = await _repository.GetUserByUsername("user1");
            user.Id.Should().Be(1);
        }

        [Fact]
        public async Task GIVEN_UserInfo_WHEN_CreatingUser_THEN_UserIsCreated()
        {
            _repository.InsertUser(new User { Id = 5, UserName = "user5", Email = "user5@test.com" });
            var user = await _repository.GetUserById(5);
            user.UserName.Should().Be("user5");
        }

        [Fact]
        public async Task GIVEN_UsersExist_WHEN_GettingAllUsers_THEN_ReturnAllUsers()
        {
            var userlist1 = await _repository.GetUsers();
            _repository.InsertUser(new User { Id = 6, UserName = "user6", Email = "user6@test.com" });
            var userlist2 = await _repository.GetUsers();
            userlist1.Count.Should().BeLessThan(userlist2.Count);
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GettingUserFollows_THEN_ReturnUserFollows()
        {
            var follows = await _repository.GetUserFollows(1);
            follows.Count.Should().Be(3);
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GettingUserFilteredFollows_THEN_ReturnUserFilteredFollows()
        {
            var follows = await _repository.GetFilteredFollows("user1");
            follows.follows.Count().Should().Be(3);
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GettingUserFollowedBy_THEN_ReturnUserFollowedBy()
        {
            var follows = await _repository.GetUserFollowedBy(4);
            follows.Count.Should().Be(2);
        }

        [Fact]
        public async Task TGIVEN_FollowerAndFollowee_WHEN_GetFollow_THEN_ReturnMatchingFollow()
        {
            var follow = await _repository.GetFollow(1,2);
            follow.Should().NotBeNull();
        }

        [Fact]
        public async Task GIVEN_FollowerAndFollowee_WHEN_CreatingFollow_THEN_FollowIsCreated()
        {
            await _repository.Follow(2, 1);
            var follow = await _repository.GetFollow(2, 1);
            follow.Should().NotBeNull();
        }

        [Fact]
        public async Task GIVEN_FollowerAndFollowee_WHEN_Unfollowing_THEN_FollowIsDeleted()
        {
            await _repository.Follow(4, 1);
            var follow = await _repository.GetFollow(4, 1);
            await _repository.Unfollow(follow);
            follow = await _repository.GetFollow(4, 1);
            follow.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
