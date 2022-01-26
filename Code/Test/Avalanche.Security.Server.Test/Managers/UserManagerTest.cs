using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Managers;
using Avalanche.Security.Server.Core.Models;
using Ism.Storage.Core.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test.Managers
{
    public class UsersManagerTest : IUsersManagerTest
    {
        private readonly DbContextOptions<SecurityDbContext> _options;
        private readonly ITestOutputHelper _output;
        private UsersManager _usersManager;

        public UsersManagerTest(ITestOutputHelper output, DbContextOptions<SecurityDbContext> options)
        {
            _options = options;
            _output = output;
        }

        #region Users

        public async Task AddUserManagerTest()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var addedUser = await _usersManager.AddUser(user).ConfigureAwait(false);

            // Assert
            Assert.Equal(user.UserName, addedUser.UserName);
        }

        public async Task AddUser_MultithreadedWritesSucceed()
        {
            const int quantity = 50;
            const int threads = 4;
            var databaseWriter = Utilities.GetDatabaseWriter(_options, _output);
            var timings = new ConcurrentBag<long>();

            // Act
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(
                     Task.Run(() => WriteUserss(databaseWriter))
                );
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Assert
            Assert.True(timings.Count == quantity * threads);

            async Task WriteUserss(DatabaseWriter<SecurityDbContext> databaseWriter)
            {
                try
                {
                    // Make a new SqliteConnection specific to this thread
                    var connectionString = Utilities.GetConnectionStringFromSqliteOptions(_options);
                    var options = Utilities.GetDbContextOptions<SecurityDbContext>(
                        new SqliteConnection(connectionString)
                    );
                    using var repo = Utilities.GetUserRepository(options, _output, databaseWriter, out var _);
                    var users = Fakers.GetUserFaker().Generate(quantity);
                    var i = 1;
                    foreach (var user in users)
                    {
                        var sw = Stopwatch.StartNew();
                        _output.WriteLine($"Inserting record {i} on Thread {Environment.CurrentManagedThreadId}");
                        _ = await repo.AddUser(user).ConfigureAwait(false);
                        sw.Stop();
                        timings?.Add(sw.ElapsedMilliseconds);
                        i++;
                    }
                }
                finally
                {
                    _output.WriteLine($"Done inserting records on Thread {Environment.CurrentManagedThreadId}");
                }
            }
        }

        public async Task Repository_AddUser_NameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Empty out required property
            user.UserName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await _usersManager.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_NameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Null out required property
            user.UserName = null;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await _usersManager.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_NameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Empty out required property
            user.UserName = Utilities.CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await _usersManager.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_UnexpectedError_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = Utilities.GetBuggyUserRepository(_options, _output, out var logger);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await _usersManager.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled(Microsoft.Extensions.Logging.LogLevel.Error, Times.Once());
        }

        public async Task UpdateUser_When_UserIsNull()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            UserModel user = null;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await _usersManager.UpdateUser(user).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<InvalidCastException>(exception);
        }

        public async Task UpdateUser_When_UserNotExist()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var users = GetUsers_ReadSucceeds();
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await _usersManager.UpdateUser(user).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<InvalidCastException>(exception);
        }

        public async Task UpdateUser_WhenUserNameIsNull()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();
            user.UserName = null;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await _usersManager.UpdateUser(user).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<InvalidCastException>(exception);
        }

        public async Task UpdateUser_Success()
        {
            // Arrange
            const int quantity = 10;
            var repository = Utilities.GetUserRepository(_options, _output, out _);
            _usersManager = new UsersManager(repository);
            var users = Fakers.GetUserFaker().Generate(quantity);

            foreach (var saveUser in users)
            {
                _ = await _usersManager.AddUser(saveUser).ConfigureAwait(false);
            }

            var user = users[0];

            // Act
            await _usersManager.UpdateUser(user).ConfigureAwait(false);

            // Assert
            using var context = new SecurityDbContext(_options);
            var readEntity = await context.Users
                .FirstAsync(x => x.UserName == user.UserName)
                .ConfigureAwait(false);

            var mapper = Utilities.GetMapper(typeof(SecurityDbContext));
            var actual = mapper.Map<UserModel>(readEntity);

            Assert.Equal(user.UserName, actual.UserName);
        }

        public async Task DeleteUser_DeleteSucceeds()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();
            var written = await repository.AddUser(user).ConfigureAwait(false);

            // Act
            var count = await _usersManager.DeleteUser(written.Id).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, count);
            var result = await _usersManager.FindByUserNameAsync(written.UserName!).ConfigureAwait(false);
            Assert.Null(result);
        }

        public async Task DeleteUser_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = Utilities.GetBuggyUserRepository(_options, _output, out var logger);
            _usersManager = new UsersManager(repository);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await _usersManager.DeleteUser(user.Id).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            //_ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled(Microsoft.Extensions.Logging.LogLevel.Error, Times.Once());
        }

        public async Task Repository_GetAllUsers_ReadSucceeds()
        {
            // Arrange
            const int quantity = 100;
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            _usersManager = new UsersManager(repository);
            foreach (var user in Fakers.GetUserFaker().Generate(quantity))
            {
                _ = await _usersManager.AddUser(user).ConfigureAwait(false);
            }

            // Act
            var readUsers = await _usersManager.GetAllUsers().ConfigureAwait(false);

            Assert.True(readUsers.Count() == quantity);
        }

        public async Task GetUser_MultithreadedReadsSucceed()
        {
            // Arrange
            const int quantity = 100;
            const int threads = 4;
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            var users = Fakers.GetUserFaker().Generate(quantity);
            foreach (var user in users)
            {
                _ = await repository.AddUser(user).ConfigureAwait(false);
            }

            var timings = new ConcurrentBag<long>();

            // Act
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(
                     Task.Run(() => ReadUsers(users))
                );
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Assert
            Assert.True(timings.Count == quantity * threads);

            async Task ReadUsers(List<UserModel> users)
            {
                try
                {
                    // Make a new SqliteConnection specific to this thread
                    var connectionString = Utilities.GetConnectionStringFromSqliteOptions(_options);
                    var options = Utilities.GetDbContextOptions<SecurityDbContext>(
                        new SqliteConnection(connectionString)
                    );
                    using var repo = Utilities.GetUserRepository(options, _output, out var _);
                    var i = 1;
                    foreach (var user in users)
                    {
                        var sw = Stopwatch.StartNew();
                        _output.WriteLine($"Reading record {i} on Thread {Environment.CurrentManagedThreadId}");
                        _ = await repo.GetUser(user.UserName!).ConfigureAwait(false);
                        sw.Stop();
                        timings?.Add(sw.ElapsedMilliseconds);
                        i++;
                    }
                }
                finally
                {
                    _output.WriteLine($"Done reading records on Thread {Environment.CurrentManagedThreadId}");
                }
            }
        }

        public async Task GetUsers_ReadSucceeds()
        {
            // Arrange
            const int quantity = 100;
            var repository = Utilities.GetUserRepository(_options, _output, out _);
            _usersManager = new UsersManager(repository);
            var users = Fakers.GetUserFaker().Generate(quantity);
            foreach (var user in users)
            {
                _ = await _usersManager.AddUser(user).ConfigureAwait(false);
            }

            var findMe = Utilities.PickRandom(users);

            // Act
            var found = await _usersManager.FindByUserNameAsync(findMe.UserName!).ConfigureAwait(false);

            // Assert
            var mapper = Utilities.GetMapper(typeof(SecurityDbContext));
            var actual = mapper.Map<UserModel>(found);

            Assert.NotNull(actual);
            Assert.NotNull(actual.UserName);

            Assert.Equal(findMe.UserName, actual.UserName);
        }

        #endregion
    }
}