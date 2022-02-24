using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.Exceptions;
using Avalanche.Security.Server.Core.Models;
using FluentValidation;
using Ism.Storage.Core.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;
using static Avalanche.Security.Server.Test.Utilities;

namespace Avalanche.Security.Server.Test.Repositories
{
    public class UserRepositoryTests : IUserRepositoryTests
    {
        private readonly DbContextOptions<SecurityDbContext> _options;
        private readonly ITestOutputHelper _output;

        public UserRepositoryTests(ITestOutputHelper output, DbContextOptions<SecurityDbContext> options)
        {
            _options = options;
            _output = output;
        }

        public async Task AddUser_Duplicate_Throws()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out _);
            var user = Fakers.GetNewUserFaker().Generate();

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
            Assert.Equal(typeof(UserEntity), dupException.Entity);
            Assert.Equal(nameof(user.UserName), dupException.ConstraintName);
            Assert.Equal(user.UserName, dupException.DuplicateValue);
        }

        public async Task AddUser_FirstNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Empty out required property
            user.FirstName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_FirstNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.FirstName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_FirstNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Set to a value with invalid length
            user.FirstName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_LastNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Empty out required property
            user.LastName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_LastNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.LastName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_LastNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Set to a value with invalid length
            user.LastName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_MultithreadedWrites_Succeeds()
        {
            const int quantity = 50;
            const int threads = 4;
            var databaseWriter = GetDatabaseWriter(_options, _output);
            var timings = new ConcurrentBag<long>();

            // Act
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(
                     Task.Run(() => WriteUsers(databaseWriter))
                );
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Assert
            Assert.True(timings.Count == quantity * threads);

            async Task WriteUsers(DatabaseWriter<SecurityDbContext> databaseWriter)
            {
                try
                {
                    // Make a new SqliteConnection specific to this thread
                    var connectionString = GetConnectionStringFromSqliteOptions(_options);
                    var options = GetDbContextOptions<SecurityDbContext>(
                        new SqliteConnection(connectionString)
                    );
                    using var repo = GetUserRepository(options, _output, databaseWriter, out var _);
                    var users = Fakers.GetNewUserFaker().Generate(quantity);
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

        public async Task AddUser_PasswordEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Empty out required property
            user.Password = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_PasswordNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.Password = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_UnexpectedError_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = GetBuggyUserRepository(_options, _output, out var logger);
            var user = Fakers.GetNewUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled(Microsoft.Extensions.Logging.LogLevel.Error, Times.Once());
        }

        public async Task AddUser_UserNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Empty out required property
            user.UserName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_UserNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.UserName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_UserNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Set to a value with invalid length
            user.UserName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.AddUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task AddUser_WriteSucceeds()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();

            // Act
            var added = await repository.AddUser(user).ConfigureAwait(false);

            // Assert
            using var context = new SecurityDbContext(_options);
            var readEntity = await context.Users
                .FirstAsync(x => x.UserName == added.UserName)
                .ConfigureAwait(false);

            var mapper = GetMapper(typeof(SecurityDbContext));
            var readModel = mapper.Map<UserModel>(readEntity);

            Assert.NotNull(readModel);
            Assert.NotNull(readModel.UserName);
            Assert.Equal(added.UserName, readModel.UserName);
            Assert.Equal(added.FirstName, readModel.FirstName);
            Assert.Equal(added.LastName, readModel.LastName);
            Assert.Equal(added.PasswordHash, readModel.PasswordHash);

            // Ensure that password was hashed on save
            Assert.NotEqual(user.Password, added.PasswordHash);
            Assert.Equal(MockHashedPassword, added.PasswordHash);
        }

        public async Task DeleteUser_DefaultUserId_Throws()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var logger);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.DeleteUser(0).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ArgumentException>(exception);
        }

        public async Task DeleteUser_DeleteSucceeds()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetNewUserFaker().Generate();
            var written = await repository.AddUser(user).ConfigureAwait(false);

            // Act
            var count = await repository.DeleteUser(written.Id).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, count);
            var result = await repository.GetUser(written.UserName).ConfigureAwait(false);
            Assert.Null(result);
        }

        public async Task DeleteUser_LogsExceptionAndThrows()
        {
            // Arrange
            var repository = GetBuggyUserRepository(_options, _output, out var logger);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.DeleteUser(1).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<InvalidOperationException>(exception);
            logger.AssertLoggerCalled(Microsoft.Extensions.Logging.LogLevel.Error, Times.Once());
        }

        public async Task GetAllUsers_ReadSucceeds()
        {
            // Arrange
            const int quantity = 100;
            var repository = GetUserRepository(_options, _output, out var _);
            foreach (var user in Fakers.GetNewUserFaker().Generate(quantity))
            {
                _ = await repository.AddUser(user).ConfigureAwait(false);
            }

            // Act
            var readUsers = await repository.GetUsers().ConfigureAwait(false);

            Assert.True(readUsers.Count() == quantity);
        }

        public async Task GetUser_MultithreadedReadsSucceed()
        {
            // Arrange
            const int quantity = 100;
            const int threads = 4;
            var repository = GetUserRepository(_options, _output, out var _);
            var users = Fakers.GetNewUserFaker().Generate(quantity);
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

            async Task ReadUsers(List<NewUserModel> users)
            {
                try
                {
                    // Make a new SqliteConnection specific to this thread
                    var connectionString = GetConnectionStringFromSqliteOptions(_options);
                    var options = GetDbContextOptions<SecurityDbContext>(
                        new SqliteConnection(connectionString)
                    );
                    using var repo = GetUserRepository(options, _output, out var _);
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
            var repository = GetUserRepository(_options, _output, out _);
            var users = Fakers.GetNewUserFaker().Generate(quantity);
            foreach (var user in users)
            {
                _ = await repository.AddUser(user).ConfigureAwait(false);
            }

            var findMe = PickRandom(users);

            // Act
            var found = await repository.GetUser(findMe.UserName!).ConfigureAwait(false);

            // Assert
            var mapper = GetMapper(typeof(SecurityDbContext));
            var actual = mapper.Map<UserModel>(found);

            Assert.NotNull(actual);
            Assert.NotNull(actual.UserName);

            Assert.Equal(findMe.UserName, actual.UserName);
            Assert.Equal(findMe.FirstName, actual.FirstName);
            Assert.Equal(findMe.LastName, actual.LastName);
            Assert.NotEqual(findMe.Password, actual.PasswordHash);
        }

        public async Task UpdateUser_DefaultId_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Set to default value
            user.Id = 0;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_FirstNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Empty out required property
            user.FirstName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_FirstNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.FirstName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_FirstNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Set to a value with invalid length
            user.FirstName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_LastNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Empty out required property
            user.LastName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_LastNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.LastName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_LastNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Set to a value with invalid length
            user.LastName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_MultithreadedUpdates_Succeeds()
        {
            const int quantity = 50;
            const int threads = 4;
            var databaseWriter = GetDatabaseWriter(_options, _output);
            var timings = new ConcurrentBag<long>();

            using var repo = GetUserRepository(_options, _output, databaseWriter, out var _);
            var users = Fakers.GetNewUserFaker().Generate(100);

            var added = new List<UserModel>();
            foreach (var user in users)
            {
                added.Add(await repo.AddUser(user).ConfigureAwait(false));
            }

            // Act
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(
                     Task.Run(() => UpdateUsers(databaseWriter, added))
                );
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Assert
            Assert.True(timings.Count == quantity * threads);

            async Task UpdateUsers(DatabaseWriter<SecurityDbContext> databaseWriter, List<UserModel> added)
            {
                try
                {
                    // Make a new SqliteConnection specific to this thread
                    var connectionString = GetConnectionStringFromSqliteOptions(_options);
                    var options = GetDbContextOptions<SecurityDbContext>(
                        new SqliteConnection(connectionString)
                    );
                    using var repo = GetUserRepository(options, _output, databaseWriter, out var _);

                    // For a set number of iterations pick a random user to update and reset their password
                    for (var i = 0; i < quantity; i++)
                    {
                        var sw = Stopwatch.StartNew();
                        var toUpdate = PickRandom(added);
                        _output.WriteLine($"Updating record with Id {toUpdate.Id} on Thread {Environment.CurrentManagedThreadId}");
                        var updateModel = new UpdateUserModel
                        {
                            Id = toUpdate.Id,
                            UserName = toUpdate.UserName,
                            FirstName = toUpdate.FirstName,
                            LastName = toUpdate.LastName,
                            Password = $"{DateTime.Now.Ticks}-{i}",
                        };

                        await repo.UpdateUser(updateModel).ConfigureAwait(false);
                        sw.Stop();
                        timings?.Add(sw.ElapsedMilliseconds);
                    }
                }
                finally
                {
                    _output.WriteLine($"Done inserting records on Thread {Environment.CurrentManagedThreadId}");
                }
            }
        }

        public async Task UpdateUser_PasswordEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Empty out required property
            user.Password = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_PasswordNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.Password = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_Succeeds()
        {
            // Arrange
            const int quantity = 10;
            var repository = GetUserRepository(_options, _output, out _);
            var users = Fakers.GetNewUserFaker().Generate(quantity);
            var added = new List<UserModel>();

            foreach (var saveuser in users)
            {
                added.Add(await repository.AddUser(saveuser).ConfigureAwait(false));
            }

            var model = new UpdateUserModel
            {
                Id = added[0].Id,
                UserName = added[0].UserName,
                FirstName = added[0].FirstName,
                LastName = added[0].LastName,
                Password = "new Password"
            };

            // Act
            await repository.UpdateUser(model).ConfigureAwait(false);

            // Assert
            using var context = new SecurityDbContext(_options);
            var readEntity = await context.Users
                .FirstAsync(x => x.UserName == model.UserName)
                .ConfigureAwait(false);

            var mapper = GetMapper(typeof(SecurityDbContext));
            var updated = mapper.Map<UserModel>(readEntity);

            Assert.NotNull(updated);
            Assert.NotNull(updated.UserName);
            Assert.Equal(model.UserName, updated.UserName);
            Assert.Equal(model.FirstName, updated.FirstName);
            Assert.Equal(model.LastName, updated.LastName);
            Assert.Equal(readEntity.PasswordHash, updated.PasswordHash);

            // Ensure that password was hashed on update
            Assert.NotEqual(model.Password, updated.PasswordHash);
            Assert.Equal(MockHashedPassword, updated.PasswordHash);
        }

        public async Task UpdateUser_UserNameEmpty_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Empty out required property
            user.UserName = "";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_UserNameNull_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Null out required property
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            user.UserName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_UserNameTooLong_ThrowsValidationException()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Set to a value with invalid length
            user.UserName = CreateString(65);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                 await repository.UpdateUser(user).ConfigureAwait(false)
            ).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ValidationException>(exception);
        }

        public async Task UpdateUser_UserNotExist_Throws()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);
            var user = Fakers.GetUpdateUserFaker().Generate();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await repository.UpdateUser(user).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<InvalidOperationException>(exception);
        }

        public async Task UpdateUser_UserNull_Throws()
        {
            // Arrange
            var repository = GetUserRepository(_options, _output, out var _);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await repository.UpdateUser(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception);
            _ = Assert.IsType<ArgumentNullException>(exception);
        }
    }
}
