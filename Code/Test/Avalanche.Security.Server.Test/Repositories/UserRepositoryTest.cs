using System;
using System.Collections.Generic;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Ism.Storage.Core.Infrastructure;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Grpc.Core.Logging;
using System.Linq;

namespace Avalanche.Security.Server.Test.Repositories
{
    public class UserRepositoryTest : IUserRepositoryTest
    {
        private readonly DbContextOptions<SecurityDbContext> _options;
        private readonly ITestOutputHelper _output;

        public UserRepositoryTest(ITestOutputHelper output, DbContextOptions<SecurityDbContext> options)
        {
            _options = options;
            _output = output;
        }

        #region Users

        public async Task AddUser_Duplicate_Throws()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out _);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            // Add it once
            _ = await repository.AddUser(user).ConfigureAwait(false);
            // Add it again...
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<DuplicateEntityException>(exception);
            var dupException = (DuplicateEntityException)exception;
            Assert.Equal(typeof(UserModel), dupException.Entity);
            Assert.Equal(nameof(user.UserName), dupException.ConstraintName);
            Assert.Equal(user.UserName, dupException.DuplicateValue);
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
            var user = Fakers.GetUserFaker().Generate();

            // Empty out required property
            user.UserName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_NameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUserFaker().Generate();

            // Null out required property
            user.UserName = null;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_NameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUserFaker().Generate();

            // Empty out required property
            user.UserName = Utilities.CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<FluentValidation.ValidationException>(exception);
        }

        public async Task AddUser_UnexpectedError_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = Utilities.GetBuggyUserRepository(_options, _output, out var logger);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled((Microsoft.Extensions.Logging.LogLevel)LogLevel.Error, Times.Once());
        }

        public async Task AddUser_WriteSucceeds()
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

        public async Task DeleteUser_DeleteSucceeds()
        {
            // Arrange
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUserFaker().Generate();
            var written = await repository.AddUser(user).ConfigureAwait(false);

            // Act
            var count = await repository.DeleteUser(written.Id).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, count);
            var result = await repository.GetUser(written.UserName!).ConfigureAwait(false);
            Assert.Null(result);
        }

        public async Task DeleteUser_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = Utilities.GetBuggyUserRepository(_options, _output, out var logger);
            var user = Fakers.GetUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.DeleteUser(user.Id).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled((Microsoft.Extensions.Logging.LogLevel)LogLevel.Error, Times.Once());
        }

        public async Task Repository_GetAllUsers_ReadSucceeds()
        {
            // Arrange
            const int quantity = 10;
            var repository = Utilities.GetUserRepository(_options, _output, out var _);
            foreach (var user in Fakers.GetUserFaker().Generate(quantity))
            {
                _ = await repository.AddUser(user).ConfigureAwait(false);
            }

            // Act
            var readDepartments = await repository.GetAllUsers().ConfigureAwait(false);

            Assert.True(readDepartments.Count() == quantity);
        }

        public async Task GetUser_MultithreadedReadsSucceed()
        {
            // Arrange
            const int quantity = 500;
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
            const int quantity = 10;
            var repository = Utilities.GetUserRepository(_options, _output, out _);
            var users = Fakers.GetUserFaker().Generate(quantity);
            foreach (var user in users)
            {
                _ = await repository.AddUser(user).ConfigureAwait(false);
            }

            var findMe = Utilities.PickRandom(users);

            // Act
            var found = await repository.GetUser(findMe.UserName!).ConfigureAwait(false);

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
