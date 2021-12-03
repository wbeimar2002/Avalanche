using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace Avalanche.Security.Server.Core
{
    public class SecurityManagementContext : DbContext
    {
        public SecurityManagementContext(DbContextOptions<SecurityManagementContext> options)
            : base(options)
        {
        }

        public SecurityManagementContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Log EF Queries if built in debug
#if DEBUG
            optionsBuilder
                .UseLoggerFactory(LoggerFactory.Create(builder =>
                    builder
                        //.AddConsole() //Temporary disabled
                        //.AddDebug()
                        .AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Debug)
                ));
#endif

            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<UserEntity> Users { get; set; } = default!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        }
    }
}
