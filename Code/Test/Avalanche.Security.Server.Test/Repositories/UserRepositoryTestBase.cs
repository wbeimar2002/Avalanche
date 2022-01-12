using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test.Repositories
{
    public class UserRepositoryTestBase
    {
        private readonly DbContextOptions<SecurityDbContext> _options;
        private readonly ITestOutputHelper _output;

        public UserRepositoryTestBase(ITestOutputHelper output, DbContextOptions<SecurityDbContext> options)
        {
            _options = options;
            _output = output;
        }

        public async Task AddUserWriteSucceeds()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            user = await repository.AddUser(user).ConfigureAwait(false);

            // Assert
            using var context = new SecurityDbContext(_options);
            var readEntity = await context.Users
                .FirstAsync(x => x.UserName == user.UserName)
                .ConfigureAwait(false);

            var mapper = Utilities.GetMapper(typeof(SecurityDbContext));
            var readModel = mapper.Map<UserModel>(readEntity);

            Assert.NotNull(readModel);
            Assert.NotNull(readModel.UserName);
            Assert.Equal(user.UserName, readModel.UserName);
        }
    }
}
