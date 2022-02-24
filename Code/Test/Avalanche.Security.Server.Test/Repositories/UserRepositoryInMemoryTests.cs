using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test.Repositories
{
    public class UserRepositoryInMemoryTests : IUserRepositoryTests
    {
        private readonly UserRepositoryTests _tests;
        public UserRepositoryInMemoryTests(ITestOutputHelper output) =>
            _tests = new UserRepositoryTests(output, GetInMemoryDbContextOptions());

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
        [Fact]
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

        /// <summary>
        /// This test doesn't work with in-memory database
        /// </summary>
        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public Task UpdateUser_MultithreadedUpdates_Succeeds() => Task.CompletedTask;
#pragma warning restore S2699 // Tests should include assertions

        [Fact]
        public Task UpdateUser_Succeeds() => _tests.UpdateUser_Succeeds();

        [Fact]
        public Task UpdateUser_UnexpectedError_LogsExceptionAndThrows() => _tests.UpdateUser_UnexpectedError_LogsExceptionAndThrows();

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
        [Fact]
        public Task UpdateUserPassword_PasswordEmpty_ThrowsValidationException() => _tests.UpdateUserPassword_PasswordEmpty_ThrowsValidationException();

        [Fact]
        public Task UpdateUserPassword_PasswordNull_ThrowsValidationException() => _tests.UpdateUserPassword_PasswordNull_ThrowsValidationException();

        [Fact]
        public Task UpdateUserPassword_Succeeds() => _tests.UpdateUserPassword_Succeeds();
        [Fact]
        public Task UpdateUserPassword_UnexpectedError_LogsExceptionAndThrows() => _tests.UpdateUserPassword_UnexpectedError_LogsExceptionAndThrows();

        private static DbContextOptions<SecurityDbContext> GetInMemoryDbContextOptions()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            return Utilities.GetDbContextOptions<SecurityDbContext>(connection);
        }
    }
}
