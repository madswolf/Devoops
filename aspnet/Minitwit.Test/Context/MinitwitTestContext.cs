using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;

namespace Minitwit.Test.Context
{
    public class MinitwitTestContext : MinitwitContext
    {
        public MinitwitTestContext(DbContextOptions<MinitwitContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "user1", Email = "user1@test.com"},
                new User { Id = 2, UserName = "user2", Email = "user2@test.com" },
                new User { Id = 3, UserName = "user3", Email = "user3@test.com" },
                new User { Id = 4, UserName = "user4", Email = "user4@test.com"}
            );
            
            modelBuilder.Entity<Message>().HasData(
                new Message { Id = 1, AuthorId = 1, PublishDate = System.DateTime.Now, Text = "Shitpost", Flagged = false },
                new Message { Id = 2, AuthorId = 2, PublishDate = System.DateTime.Now, Text = "Dead meme", Flagged = false },
                new Message { Id = 3, AuthorId = 3, PublishDate = System.DateTime.Now.AddDays(-1), Text = "Repost", Flagged = false },
                new Message { Id = 4, AuthorId = 4, PublishDate = System.DateTime.Now.AddHours(-1), Text = "Actual sensible post", Flagged = false },
                new Message { Id = 5, AuthorId = 1, PublishDate = System.DateTime.Now, Text = "L + ratio", Flagged = true },
                new Message { Id = 6, AuthorId = 3, PublishDate = System.DateTime.Now, Text = "Ingefær", Flagged = false }
            );

            
            modelBuilder.Entity<Latest>().HasData(
                new Latest { Id = 1, Value = 1101, CreationTime = System.DateTime.Now}
            );
            
            modelBuilder.Entity<Follow>().HasData(
                new Follow { FollowerId = 1, FolloweeId = 2 },
                new Follow { FollowerId = 1, FolloweeId = 3 },
                new Follow { FollowerId = 1, FolloweeId = 4 },
                new Follow { FollowerId = 2, FolloweeId = 4 },
                new Follow { FollowerId = 3, FolloweeId = 1 },
                new Follow { FollowerId = 3, FolloweeId = 2 },
                new Follow { FollowerId = 4, FolloweeId = 3 }
            );
        }
    }
}
