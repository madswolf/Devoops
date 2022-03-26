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
        public async Task Test_get_private_timeline()
        {
            var follows = (await _userRepository.GetUserFollows(2)).Select(f => f.FolloweeId).ToList();
            var timeline = await _repository.GetPrivateTimeline(2, follows);
            Assert.Equal(2, timeline.Count);
        }

        [Fact]
        public async Task Test_get_messages_by_author_id()
        {
            var messages = await _repository.GetMessagesByAuthorId(3);
            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async Task Test_get_filtered_messages_by_author_id()
        {
            var messages = await _repository.GetFilteredMessagesByAuthorId(3);
            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async Task Test_get_messages()
        {
            var messages = await _repository.GetMessages();
            Assert.Equal(5, messages.Count);
        }

        [Fact]
        public async Task Test_get_filtered_messages()
        {
            var messages = await _repository.GetFilteredMessages();
            Assert.Equal(5, messages.Count);
        }

        [Fact]
        public async Task Test_insert_messages()
        {
            await _repository.InsertMessage(new Message { Id = 7, AuthorId = 1,
                PublishDate = System.DateTime.Now, Text = "test post", Flagged = false });
            var messages = await _repository.GetMessages();
            Assert.Equal(6, messages.Count);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
