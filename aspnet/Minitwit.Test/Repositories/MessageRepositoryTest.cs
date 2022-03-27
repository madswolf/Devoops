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

namespace Minitwit.Test.Repositories
{
    public class MessageRepositoryTest : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly MinitwitContext _context;
        private readonly MessageRepository _repository;
        private readonly UserRepository _userRepository;

        public MessageRepositoryTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var builder = new DbContextOptionsBuilder<MinitwitContext>().UseSqlite(_connection);
            _context = new MinitwitTestContext(builder.Options);
            _context.Database.EnsureCreated();
            _repository = new MessageRepository(_context);
            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public async Task GIVEN_UserAndFollows_WHEN_GettingTimeline_THEN_ReturnUserTimeline()
        {
            var follows = (await _userRepository.GetUserFollows(2)).Select(f => f.FolloweeId).ToList();
            var timeline = await _repository.GetPrivateTimeline(2, follows);
            timeline.Count.Should().Be(2);
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GettingAuthorMessages_THEN_ReturnUserMessages()
        {
            var messages = await _repository.GetMessagesByAuthorId(3);
            messages.Count.Should().Be(2);
        }

        [Fact]
        public async Task GIVEN_UserId_WHEN_GettingFilteredAuthorMessages_THEN_ReturnFilteredUserMessages()
        {
            var messages = await _repository.GetFilteredMessagesByAuthorId(3);
            messages.Count.Should().Be(2);
        }

        [Fact]
        public async Task GIVEN_MessagesExist_WHEN_GettingAllMessages_THEN_ReturnAllMessages()
        {
            var messages = await _repository.GetMessages();
            messages.Count.Should().Be(5);
        }

        [Fact]
        public async Task GIVEN_MessagesExist_WHEN_GettingAllFilteredMessages_THEN_ReturnAllFilteredMessages()
        {
            var messages = await _repository.GetFilteredMessages();
            messages.Count.Should().Be(5);
        }

        [Fact]
        public async Task GIVEN_MessageInfo_WHEN_CreatingMessage_THEN_MessageIsCreated()
        {
            await _repository.InsertMessage(new Message { Id = 7, AuthorId = 1,
                PublishDate = System.DateTime.Now, Text = "test post", Flagged = false });
            var messages = await _repository.GetMessages();
            messages.Count.Should().Be(6);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
