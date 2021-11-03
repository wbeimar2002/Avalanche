using System.Threading.Tasks;
using Avalanche.Security.Server.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Avalanche.Security.Server.Test.Utilities;

namespace Avalanche.Security.Server.Test
{
#pragma warning disable S2699 // Tests should include assertions
    public class UserRepositoryInMemoryTests : IUserRepositoryTests
    {
        private readonly UserRepositoryTests _tests;

        public UserRepositoryInMemoryTests() =>
            _tests = new UserRepositoryTests(GetSqliteInMemoryDbContextOptions());

        [Fact]
        public async Task Repository_AddUser_Succeeds() =>
            await _tests.Repository_AddUser_Succeeds().ConfigureAwait(false);

        [Fact]
        public async Task Repository_FindUser_Fails() =>
            await _tests.Repository_FindUser_Fails().ConfigureAwait(false);

        [Fact]
        public async Task Repository_FindUser_Succeeds() =>
            await _tests.Repository_FindUser_Succeeds().ConfigureAwait(false);

        [Fact]
        public async Task Repository_AddUser_PasswordMissing_Throws() =>
            await _tests.Repository_AddUser_PasswordMissing_Throws().ConfigureAwait(false);

        [Fact]
        public async Task Repository_AddUser_LoginNameMissing_Throws() =>
            await _tests.Repository_AddUser_LoginNameMissing_Throws().ConfigureAwait(false);

        private static DbContextOptions<SecurityDbContext> GetSqliteInMemoryDbContextOptions()
        {
            var connectionString = new SqliteConnection("DataSource=:memory:");

            return GetDbContextOptions<SecurityDbContext>(connectionString);
        }
    }

#pragma warning restore S2699 // Tests should include assertions

}
