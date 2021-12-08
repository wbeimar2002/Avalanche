using System.Collections.Generic;
using System.Linq;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.Security.Hashing;

namespace Avalanche.Security.Server.Core
{
    public class DatabaseSeed
    {
        public static void Seed(SecurityDbContext context, IPasswordHasher passwordHasher)
        {
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                var users = new List<UserEntity>
                {
                    new UserEntity { FirstName = "Main", LastName ="Administrator", UserName = "admin@admin.com", Password = passwordHasher.HashPassword("12345678") },
                    new UserEntity { FirstName = "Common", LastName ="User", UserName = "common@common.com", Password = passwordHasher.HashPassword("12345678") },
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}
