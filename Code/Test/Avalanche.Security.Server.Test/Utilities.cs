using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Validators;
using Avalanche.Shared.Infrastructure.Security.Hashing;
using Ism.Storage.Core.Infrastructure;
using Ism.Storage.Core.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Avalanche.Security.Server.Test
{
    public static class Utilities
    {
        public const string MockHashedPassword = "ThisIsTheValueAlwaysReturnedByMockPasswordHasher";
        private static readonly Random Random = new Random();

        public static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[stringLength];

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[Random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        public static Mock<IDatabaseWriter<TContext>> GetBuggyDatabaseWriter<TContext>() where TContext : DbContext
        {
            var dbWriter = new Mock<IDatabaseWriter<TContext>>();
            _ = dbWriter.Setup(x => x.Write(It.IsAny<Func<TContext, Task>>()))
                .Throws(new InvalidOperationException());

            _ = dbWriter.Setup(x => x.Write((Func<TContext, Task<It.IsAnyType>>)It.IsAny<object>()))
                .Throws(new InvalidOperationException());
            return dbWriter;
        }

        public static Mock<ILogger<T>> GetLoggerMock<T>(ITestOutputHelper output)
        {
            var mock = new Mock<ILogger<T>>();
            _ = mock.Setup(x => x.Log(
                  It.IsAny<LogLevel>(),
                  It.IsAny<EventId>(),
                  It.IsAny<object>(),
                  It.IsAny<Exception>(),
                  (Func<object, Exception, string>)It.IsAny<object>())
            )
            .Callback<IInvocation>(invocation =>
            {
                var state = invocation.Arguments[2];
                output.WriteLine(state?.ToString());
            });
            return mock;
        }

        public static Mapper GetMapper(Type type)
        {
            var mapperConfig = GetMapperConfiguration(type);
            return new Mapper(mapperConfig);
        }

        public static Mock<IPasswordHasher> GetMockPasswordHasher()
        {
            var mock = new Mock<IPasswordHasher>();
            mock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns(MockHashedPassword);
            return mock;
        }

        public static MapperConfiguration GetMapperConfiguration(Type type) =>
            new MapperConfiguration(cfg =>
            {
                var assembly = Assembly.GetAssembly(type);
                cfg.AddMaps(assembly);
            }
        );

        public static ServiceProvider GetServiceProviderWithEfContext<TContext>(Func<TContext> factory) where TContext : DbContext
        {
            var services = new ServiceCollection();

            _ = services.AddTransient((_) => factory());
            return services.BuildServiceProvider();
        }

        public static UserRepository GetBuggyUserRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<UserRepository>> logger)
        {
            var dbWriter = GetBuggyDatabaseWriter<SecurityDbContext>();
            logger = GetLoggerMock<UserRepository>(output);

            return new UserRepository(
                logger.Object,
                GetMapper(typeof(SecurityDbContext)),
                new SecurityDbContext(options),
                dbWriter.Object,
                new NewUserValidator(),
                new UpdateUserValidator(),
                new UpdateUserPasswordValidator(),
                GetMockPasswordHasher().Object
            );
        }

        public static DatabaseWriter<SecurityDbContext> GetDatabaseWriter(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output)
        {
            var serviceProvider = GetServiceProviderWithEfContext(() => new SecurityDbContext(options));
            return new DatabaseWriter<SecurityDbContext>(
                GetLoggerMock<DatabaseWriter<SecurityDbContext>>(output).Object,
                serviceProvider.GetRequiredService<IServiceScopeFactory>()
            );
        }

        public static UserRepository GetUserRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<UserRepository>> logger) =>
            GetUserRepository(options, output, GetDatabaseWriter(options, output), out logger);

        public static UserRepository GetUserRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, DatabaseWriter<SecurityDbContext> databaseWriter, out Mock<ILogger<UserRepository>> logger)
        {
            logger = GetLoggerMock<UserRepository>(output);

            return new UserRepository(
                logger.Object,
                GetMapper(typeof(SecurityDbContext)),
                new SecurityDbContext(options),
                databaseWriter,
                new NewUserValidator(),
                new UpdateUserValidator(),
                new UpdateUserPasswordValidator(),
                GetMockPasswordHasher().Object
            );
        }

        public static T PickRandom<T>(ICollection<T> enumerable)
        {
            var rand = new Random();
            var randomIndex = rand.Next(0, enumerable.Count - 1);
            return enumerable.ElementAt(randomIndex);
        }

        public static DbContextOptions<TContext> GetDbContextOptions<TContext>(SqliteConnection connection) where TContext : DbContext
        {
            connection.Open();

            var options = new DbContextOptionsBuilder<TContext>()
                .UseSqlite(connection)
                .Options;

            // Create the schema in the database
            using (var context = (TContext?)Activator.CreateInstance(typeof(TContext), options))
            {
                _ = context!.Database.EnsureCreated();
            }

            return options;
        }

        public static DatabaseMigrationManager GetDatabaseManager(ITestOutputHelper output) => new DatabaseMigrationManager(GetLoggerMock<DatabaseMigrationManager>(output).Object);

        /// <summary>
        /// SqliteConnection is not thread-safe.
        /// In order to execute concurrent reads on multiple threads, we need to get the underlying connection string to create a connection for each thread
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="options"></param>
        public static string GetConnectionStringFromSqliteOptions<TContext>(DbContextOptions<TContext> options) where TContext : DbContext =>

#pragma warning disable EF1001 // Internal EF Core API usage.
            ((SqliteOptionsExtension)options.Extensions.FirstOrDefault(x => x.GetType() == typeof(SqliteOptionsExtension))).Connection.ConnectionString;
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}
