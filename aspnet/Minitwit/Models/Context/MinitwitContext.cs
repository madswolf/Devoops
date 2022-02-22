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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Author)
                .HasForeignKey(m => m.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follow>().HasKey(f => new { f.FollowerId, f.FolloweeId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Follows)
                .HasForeignKey(f => f.FollowerId);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Followee)
                .WithMany(u => u.FollowedBy)
                .HasForeignKey(f => f.FolloweeId);

            modelBuilder.Entity<Follow>().HasIndex(f => new { f.FolloweeId, f.FollowerId });
            modelBuilder.Entity<Follow>().HasIndex(f => new { f.FollowerId, f.FolloweeId });

            modelBuilder.Entity<Message>().HasIndex(m => new{m.PublishDate});
            modelBuilder.Entity<Message>().HasIndex(m => new{m.Flagged,m.PublishDate});
            modelBuilder.Entity<Message>().HasIndex(m => new{m.AuthorId,m.PublishDate});
            modelBuilder.Entity<Message>().HasIndex(m => new{m.Flagged,m.AuthorId,m.PublishDate});
            modelBuilder.Entity<Latest>().HasIndex(l => new{l.CreationTime,l.Value});
        }
    }
}