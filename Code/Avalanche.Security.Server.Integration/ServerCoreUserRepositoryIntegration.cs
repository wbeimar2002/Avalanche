using System;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Test;
using Avalanche.Security.Server.Test.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ism.Storage.Core.Infrastructure;
using Xunit;
using Xunit.Abstractions;

[assembly: AssemblyTrait("Category", "SkipWhenLiveUnitTesting")]
namespace Avalanche.Security.Server.Integration
{
    public class ServerCoreUserRepositoryIntegration : IUserRepositoryTest
    {
        private readonly UserRepositoryTest _tests;

        public ServerCoreUserRepositoryIntegration(ITestOutputHelper output) =>
            _tests = new UserRepositoryTest(output, GetInMemoryDbContextOptions(output));

        [Fact]
        public Task AddUser_UnexpectedError_LogsExceptionAndThrows() => _tests.AddUser_UnexpectedError_LogsExceptionAndThrows();

        [Trait("Category", "SkipWhenLiveUnitTesting")]
        [Theory]
        [InlineData]
        public Task AddUser_WriteSucceeds() =>
            _tests.AddUser_WriteSucceeds();

        [Fact]
        public Task AddUser_Duplicate_Throws() =>
            _tests.AddUser_Duplicate_Throws();

        [Fact]
        public Task AddUser_MultithreadedWritesSucceed() =>
            _tests.AddUser_MultithreadedWritesSucceed();

        [Fact]
        public Task AddUser_NameNull_ThrowsValidationException() =>
            _tests.AddUser_NameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_NameTooLong_ThrowsValidationException() =>
            _tests.AddUser_NameTooLong_ThrowsValidationException();

        [Fact]
        public Task DeleteUser_DeleteSucceeds() =>
            _tests.DeleteUser_DeleteSucceeds();

        [Fact]
        public Task DeleteUser_LogsExceptionAndThrows() =>
            _tests.DeleteUser_LogsExceptionAndThrows();

        [Fact]
        public Task GetUsers_ReadSucceeds() =>
            _tests.GetUsers_ReadSucceeds();

        [Fact]
        public Task GetUser_MultithreadedReadsSucceed() =>
            _tests.GetUser_MultithreadedReadsSucceed();

        [Fact]
        public Task Repository_AddUser_NameEmpty_ThrowsValidationException() =>
            _tests.Repository_AddUser_NameEmpty_ThrowsValidationException();

        [Fact]
        public Task Repository_GetAllUsers_ReadSucceeds() => 
            _tests.Repository_GetAllUsers_ReadSucceeds();

        [Fact]
        public Task UpdateUser_When_UserNotExist() => 
            _tests.UpdateUser_When_UserNotExist();

        [Fact]
        public Task UpdateUser_When_UserIsNull() => 
            _tests.UpdateUser_When_UserIsNull();

        [Fact]
        public Task UpdateUser_WhenUserNameIsNull() => 
            _tests.UpdateUser_WhenUserNameIsNull();

        [Fact]
        public Task UpdateUser_Success() =>
            _tests.UpdateUser_Success();

        #region private       

        private static DbContextOptions<SecurityDbContext> GetInMemoryDbContextOptions(ITestOutputHelper output)
        {
            var databasePath = $"users_storagetests_{ DateTime.Now.Ticks}.db";

            // Create database from migrations so we don't just get the EF default schema for our dbcontext
            var databaseManager = Utilities.GetDatabaseManager(output);
            _ = databaseManager.UpgradeDatabase(databasePath, typeof(SecurityDbContext).Assembly);

            var connectionString = new SqliteConnection(DatabaseMigrationManager.MakeConnectionString(databasePath));
            return Utilities.GetDbContextOptions<SecurityDbContext>(connectionString);
        }

        #endregion



    }
}
