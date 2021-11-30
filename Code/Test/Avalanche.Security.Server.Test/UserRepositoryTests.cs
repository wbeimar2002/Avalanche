using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Avalanche.Security.Server.Test.Utilities;

namespace Avalanche.Security.Server.Test
{
    public class UserRepositoryTests : IUserRepositoryTests
    {
        private readonly DbContextOptions<SecurityDbContext> _options;

        public UserRepositoryTests(DbContextOptions<SecurityDbContext> options) => _options = options;

        public async Task Repository_AddUser_Succeeds()
        {
            // Arrange
            var repository = GetRepository(_options);

            // Act
            await repository.AddAsync(new UserEntity
            {
                LoginName = "test@test.com",
                Password = "test123",
                FirstName = "test",
                LastName = "test"
            },
            System.Array.Empty<Core.Models.ERole>()).ConfigureAwait(false);


            // Assert
            using var context = new SecurityDbContext(_options);
            var readUserList = await context.Users
                .ToListAsync()
                .ConfigureAwait(false);

            Assert.Single(readUserList);
        }

        public async Task Repository_AddUser_PasswordMissing_Throws()
        {
            // Arrange
            var repository = GetRepository(_options);

            // Act
            var exception = await Record.ExceptionAsync(async () => await repository.AddAsync(new UserEntity
            {
                LoginName = "test@test.com",
                FirstName = "test",
                LastName = "test"
            },
            System.Array.Empty<Core.Models.ERole>()).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
        }

        public async Task Repository_AddUser_LoginNameMissing_Throws()
        {
            // Arrange
            var repository = GetRepository(_options);

            // Act
            var exception = await Record.ExceptionAsync(async () => await repository.AddAsync(new UserEntity
            {
                Password = "test123",
                FirstName = "test",
                LastName = "test"
            },
            System.Array.Empty<Core.Models.ERole>()).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
        }

        public async Task Repository_FindUser_Fails()
        {
            // Arrange
            var repository = GetRepository(_options);

            // Act
            var user = await repository.FindByLoginAsync("test1").ConfigureAwait(false);

            // Assert
            Assert.Null(user);
        }

        public async Task Repository_FindUser_Succeeds()
        {
            // Arrange
            var repository = GetRepository(_options);

            // Act
            await repository.AddAsync(new UserEntity { LoginName = "test@test.com", Password = "test123", FirstName = "test", LastName = "test" }, System.Array.Empty<Core.Models.ERole>()).ConfigureAwait(false);
            var user = await repository.FindByLoginAsync("test@test.com").ConfigureAwait(false);

            // Assert
            Assert.NotNull(user);
        }
    }
}
