using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test.Managers
{
    public class SecurityInMemoryTest : IUserManagerTest
    {
        private readonly UserManagerTest _tests;
        public SecurityInMemoryTest(ITestOutputHelper output) =>
            _tests = new UserManagerTest(output, GetInMemoryDbContextOptions());

        [Fact]
        public Task AddUser_Duplicate_Throws() => _tests.AddUser_Duplicate_Throws();

        [Fact]
        public Task AddUser_MultithreadedWritesSucceed() => _tests.AddUser_MultithreadedWritesSucceed();

        [Fact]
        public Task AddUser_NameNull_ThrowsValidationException() => _tests.AddUser_NameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_NameTooLong_ThrowsValidationException() => _tests.AddUser_NameTooLong_ThrowsValidationException();

        [Fact]
        public Task AddUser_UnexpectedError_LogsExceptionAndThrows() => _tests.AddUser_UnexpectedError_LogsExceptionAndThrows();

        [Fact]
        public Task AddUser_WriteSucceeds() => _tests.AddUser_WriteSucceeds();

        [Fact]
        public Task DeleteUser_DeleteSucceeds() => _tests.DeleteUser_DeleteSucceeds();

        [Fact]
        public Task DeleteUser_LogsExceptionAndThrows() => _tests.DeleteUser_LogsExceptionAndThrows();

        [Fact]
        public Task GetUsers_ReadSucceeds() => _tests.GetUsers_ReadSucceeds();

        [Fact]
        public Task GetUser_MultithreadedReadsSucceed() => _tests.GetUser_MultithreadedReadsSucceed();

        [Fact]
        public Task Repository_AddUser_NameEmpty_ThrowsValidationException() => _tests.Repository_AddUser_NameEmpty_ThrowsValidationException();

        [Fact]
        public Task Repository_GetAllUsers_ReadSucceeds() => _tests.Repository_GetAllUsers_ReadSucceeds();

        private static DbContextOptions<SecurityDbContext> GetInMemoryDbContextOptions()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            return Utilities.GetDbContextOptions<SecurityDbContext>(connection);
        }
    }
}
