using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Entity;

namespace Minitwit.Models.Context
{
    public class MinitwitContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public MinitwitContext(DbContextOptions<MinitwitContext> options) : base(options)
        {
        }

        public DbSet<Message> Posts { get; set; }
        public DbSet<Latest> Latest { get; set; }
        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

            builder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Author)
                .HasForeignKey(m => m.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Follow>().HasKey(f => new { f.FollowerId, f.FolloweeId });

            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Follows)
                .HasForeignKey(f => f.FollowerId);

            builder.Entity<Follow>()
                .HasOne(f => f.Followee)
                .WithMany(u => u.FollowedBy)
                .HasForeignKey(f => f.FolloweeId);

            builder.Entity<Follow>().HasIndex(f => f.FolloweeId);
            builder.Entity<Follow>().HasIndex(f => new { f.FolloweeId, f.FollowerId });
            builder.Entity<Follow>().HasIndex(f => new { f.FollowerId, f.FolloweeId });

            builder.Entity<Message>().HasIndex(m => new{m.PublishDate});
            builder.Entity<Message>().HasIndex(m => new{m.Flagged,m.PublishDate});
            builder.Entity<Message>().HasIndex(m => new{m.AuthorId,m.PublishDate});
            builder.Entity<Message>().HasIndex(m => new{m.Flagged,m.AuthorId,m.PublishDate});
            builder.Entity<Latest>().HasIndex(l => new{l.CreationTime,l.Value});
        }
    }
}