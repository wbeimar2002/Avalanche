using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Avalanche.Security.Server.Persistence
{
    public class SecurityDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserFtsEntity> UserFts { get; set; }

        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _ = modelBuilder.ApplyConfiguration(new UserFtsEntityConfiguration());

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}
