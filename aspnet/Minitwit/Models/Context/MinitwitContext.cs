using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Entity;

namespace Minitwit.Models.Context
{
    public class MinitwitContext : DbContext
    {
        public MinitwitContext(DbContextOptions<MinitwitContext> options) : base(options)
        {
        }

        public DbSet<Message> Posts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasMany(u => u.FollowedBy);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Author)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.Follows);
        }
    }
}