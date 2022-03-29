using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;
using Minitwit.Repositories;
using Minitwit.Test.Context;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Minitwit.Test.Repositories
{
    public class LatestRepositoryTest : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly MinitwitContext _context;
        private readonly LatestRepository _repository;

        public LatestRepositoryTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var builder = new DbContextOptionsBuilder<MinitwitContext>().UseSqlite(_connection);
            _context = new MinitwitTestContext(builder.Options);
            _context.Database.EnsureCreated();
            _repository = new LatestRepository(_context);
        }

        [Fact]
        public async Task GIVEN_LatestExists_WHEN_GettingLatest_THEN_ReturnLatest()
        {
            var latest = await _repository.GetLatest();
            latest.Value.Should().Be(1101);
        }

        [Fact]
        public async Task GIVEN_LatestInfo_WHEN_CreatingLatest_THEN_LatestIsCreated()
        {
            await _repository.InsertLatest(new Latest { Id = 2, Value = 1102, CreationTime = System.DateTime.Now });
            var latest = await _repository.GetLatest();
            latest.Value.Should().Be(1102);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
