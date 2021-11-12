using System;
using System.Threading.Tasks;
using Avalanche.Security.Server.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Avalanche.Security.Server.Test.Utilities;

namespace Avalanche.Security.Server.Test
{
#pragma warning disable S2699 // Tests should include assertions
    public class UserRepositoryIntegrationTests : IUserRepositoryTests
    {
        private readonly UserRepositoryTests _tests;

        public UserRepositoryIntegrationTests() =>
            _tests = new UserRepositoryTests(GetSqliteDbContextOptions());

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

        private static DbContextOptions<SecurityDbContext> GetSqliteDbContextOptions()
        {
            var databasePath = $"users_storagetests_{ DateTime.Now.Ticks}.db";
            var connectionString = new SqliteConnection(MakeConnectionString(databasePath));

            return GetDbContextOptions<SecurityDbContext>(connectionString);
        }

        public static string MakeConnectionString(string databasePath) => $"Data Source={databasePath};";
    }

#pragma warning restore S2699 // Tests should include assertions

}
