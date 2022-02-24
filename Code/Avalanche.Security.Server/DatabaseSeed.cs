using System.Collections.Generic;
using System.Linq;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Shared.Infrastructure.Security.Hashing;

namespace Avalanche.Security.Server
{
    public class DatabaseSeed
    {
        // TODO: Move to dbup migration so plaintext passwords are not included in source
        public static void Seed(SecurityDbContext context, IPasswordHasher passwordHasher)
        {
            if (!context.Users.Any())
            {
                var users = new List<UserEntity>
                {
                    new UserEntity { FirstName = "Main", LastName ="ism_user", UserName = "ism_user", PasswordHash = passwordHasher.HashPassword("1234") },
                    new UserEntity { FirstName = "Main", LastName ="ism_admin", UserName = "ism_admin", PasswordHash = passwordHasher.HashPassword("admin") },
                    new UserEntity { FirstName = "Main", LastName ="Administrator", UserName = "Administrator", PasswordHash = passwordHasher.HashPassword("Groove@net.com") }
                };

                context.Users.AddRange(users);
                _ = context.SaveChanges();
            }
        }
    }
}