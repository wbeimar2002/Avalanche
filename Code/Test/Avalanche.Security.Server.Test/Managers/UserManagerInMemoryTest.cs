using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Managers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test.Managers
{
    public class UserManagerInMemoryTest
    {
        private readonly IUsersManagerTest _tests;

        public UserManagerInMemoryTest(ITestOutputHelper output) =>
            _tests = new UsersManagerTest(output, GetInMemoryDbContextOptions());

        [Fact]
        public Task AddUserManagerTest() => _tests.AddUserManagerTest();

        //[Fact]
        //public Task AddUser_MultithreadedWritesSucceed() => _tests.AddUser_MultithreadedWritesSucceed();

        [Fact]
        public Task AddUser_NameNull_ThrowsValidationException() => _tests.AddUser_NameNull_ThrowsValidationException();

        [Fact]
        public Task AddUser_NameTooLong_ThrowsValidationException() => _tests.AddUser_NameTooLong_ThrowsValidationException();

        [Fact]
        public Task AddUser_UnexpectedError_LogsExceptionAndThrows() => _tests.AddUser_UnexpectedError_LogsExceptionAndThrows();

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

        [Fact]
        public Task UpdateUser_When_UserNotExist() => _tests.UpdateUser_When_UserNotExist();

        [Fact]
        public Task UpdateUser_When_UserIsNull() => _tests.UpdateUser_When_UserIsNull();

        [Fact]
        public Task UpdateUser_WhenUserNameIsNull() => _tests.UpdateUser_WhenUserNameIsNull();

        [Fact]
        public Task UpdateUser_Success() => _tests.UpdateUser_Success();

        private static DbContextOptions<SecurityDbContext> GetInMemoryDbContextOptions()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            return Utilities.GetDbContextOptions<SecurityDbContext>(connection);
        }
    }
}
