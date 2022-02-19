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
        public DbSet<User> Users { get; set; }
        public DbSet<Latest> Latest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

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