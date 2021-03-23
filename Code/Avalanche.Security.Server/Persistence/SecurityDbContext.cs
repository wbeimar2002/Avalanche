using JetBrains.Annotations;
using Avalanche.Security.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Avalanche.Security.Server.Persistence
{
    public class SecurityDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}