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
            if (!context.Users.Any())
            {
                var users = new List<UserEntity>
                {
                    new UserEntity { FirstName = "Main", LastName ="ism_user", UserName = "ism_user", Password = passwordHasher.HashPassword("1234") },
                    new UserEntity { FirstName = "Main", LastName ="ism_admin", UserName = "ism_admin", Password = passwordHasher.HashPassword("admin") },
                    new UserEntity { FirstName = "Main", LastName ="Administrator", UserName = "Administrator", Password = passwordHasher.HashPassword("Groove@net.com") }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}
