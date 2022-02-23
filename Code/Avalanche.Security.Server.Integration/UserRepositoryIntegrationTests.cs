using System;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Test;
using Avalanche.Security.Server.Test.Repositories;
using Ism.Storage.Core.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

[assembly: AssemblyTrait("Category", "SkipWhenLiveUnitTesting")]
namespace Avalanche.Security.Server.Integration
{
#pragma warning disable S2699 // Tests should include assertions
    public class UserRepositoryIntegrationTests : IUserRepositoryTests
    {
        private readonly UserRepositoryTests _tests;

        public UserRepositoryIntegrationTests(ITestOutputHelper output) =>
            _tests = new UserRepositoryTests(output, GetInMemoryDbContextOptions(output));

        [Fact]
        public Task AddUser_Duplicate_Throws() => _tests.AddUser_Duplicate_Throws();

        [Fact]
        public Task AddUser_FirstNameEmpty_ThrowsValidationException() => _tests.AddUser_FirstNameEmpty_ThrowsValidationException();

        [Fact]
        public Task AddUser_FirstNameNull_ThrowsValidationException() => _tests.AddUser_FirstNameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_FirstNameTooLong_ThrowsValidationException() => _tests.AddUser_FirstNameTooLong_ThrowsValidationException();

        [Fact]
        public Task AddUser_LastNameEmpty_ThrowsValidationException() => _tests.AddUser_LastNameEmpty_ThrowsValidationException();

        [Fact]
        public Task AddUser_LastNameNull_ThrowsValidationException() => _tests.AddUser_LastNameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_LastNameTooLong_ThrowsValidationException() => _tests.AddUser_LastNameTooLong_ThrowsValidationException();

        [Fact]
        public Task AddUser_MultithreadedWrites_Succeeds() => _tests.AddUser_MultithreadedWrites_Succeeds();

        [Fact]
        public Task AddUser_PasswordEmpty_ThrowsValidationException() => _tests.AddUser_PasswordEmpty_ThrowsValidationException();

        [Fact]
        public Task AddUser_PasswordNull_ThrowsValidationException() => _tests.AddUser_PasswordNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_UnexpectedError_LogsExceptionAndThrows() => _tests.AddUser_UnexpectedError_LogsExceptionAndThrows();

        [Fact]
        public Task AddUser_UserNameEmpty_ThrowsValidationException() => _tests.AddUser_UserNameEmpty_ThrowsValidationException();

        [Fact]
        public Task AddUser_UserNameNull_ThrowsValidationException() => _tests.AddUser_UserNameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_UserNameTooLong_ThrowsValidationException() => _tests.AddUser_UserNameTooLong_ThrowsValidationException();

        public Task AddUser_WriteSucceeds() => _tests.AddUser_WriteSucceeds();

        [Fact]
        public Task DeleteUser_DefaultUserId_Throws() => _tests.DeleteUser_DefaultUserId_Throws();
        [Fact]
        public Task DeleteUser_DeleteSucceeds() => _tests.DeleteUser_DeleteSucceeds();

        [Fact]
        public Task DeleteUser_LogsExceptionAndThrows() => _tests.DeleteUser_LogsExceptionAndThrows();

        [Fact]
        public Task GetAllUsers_ReadSucceeds() => _tests.GetAllUsers_ReadSucceeds();

        [Fact]
        public Task GetUser_MultithreadedReadsSucceed() => _tests.GetUser_MultithreadedReadsSucceed();

        [Fact]
        public Task GetUsers_ReadSucceeds() => _tests.GetUsers_ReadSucceeds();
        [Fact]
        public Task UpdateUser_DefaultId_ThrowsValidationException() => _tests.UpdateUser_DefaultId_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_FirstNameEmpty_ThrowsValidationException() => _tests.UpdateUser_FirstNameEmpty_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_FirstNameNull_ThrowsValidationException() => _tests.UpdateUser_FirstNameNull_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_FirstNameTooLong_ThrowsValidationException() => _tests.UpdateUser_FirstNameTooLong_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_LastNameEmpty_ThrowsValidationException() => _tests.UpdateUser_LastNameEmpty_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_LastNameNull_ThrowsValidationException() => _tests.UpdateUser_LastNameNull_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_LastNameTooLong_ThrowsValidationException() => _tests.UpdateUser_LastNameTooLong_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_MultithreadedUpdates_Succeeds() => _tests.UpdateUser_MultithreadedUpdates_Succeeds();

        [Fact]
        public Task UpdateUser_PasswordEmpty_ThrowsValidationException() => _tests.UpdateUser_PasswordEmpty_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_PasswordNull_ThrowsValidationException() => _tests.UpdateUser_PasswordNull_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_Succeeds() => _tests.UpdateUser_Succeeds();

        [Fact]
        public Task UpdateUser_UserNameEmpty_ThrowsValidationException() => _tests.UpdateUser_UserNameEmpty_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_UserNameNull_ThrowsValidationException() => _tests.UpdateUser_UserNameNull_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_UserNameTooLong_ThrowsValidationException() => _tests.UpdateUser_UserNameTooLong_ThrowsValidationException();

        [Fact]
        public Task UpdateUser_UserNotExist_Throws() => _tests.UpdateUser_UserNotExist_Throws();

        [Fact]
        public Task UpdateUser_UserNull_Throws() => _tests.UpdateUser_UserNull_Throws();
        private static DbContextOptions<SecurityDbContext> GetInMemoryDbContextOptions(ITestOutputHelper output)
        {
            var databasePath = $"users_storagetests_{ DateTime.Now.Ticks}.db";

            // Create database from migrations so we don't just get the EF default schema for our dbcontext
            var databaseManager = Utilities.GetDatabaseManager(output);
            _ = databaseManager.UpgradeDatabase(databasePath, typeof(SecurityDbContext).Assembly);

            var connectionString = new SqliteConnection(DatabaseMigrationManager.MakeConnectionString(databasePath));
            return Utilities.GetDbContextOptions<SecurityDbContext>(connectionString);
        }
    }
#pragma warning restore S2699 // Tests should include assertions
}
