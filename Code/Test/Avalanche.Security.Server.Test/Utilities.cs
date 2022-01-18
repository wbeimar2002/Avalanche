using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Managers;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Validators;
using Avalanche.Security.Server.Security.Hashing;
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
        private static readonly Random Random = new Random();
        private static readonly IPasswordHasher passwordHasher = new PasswordHasher();

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
                new UserValidator(),
                passwordHasher
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
                new UserValidator(),
                passwordHasher
            );
        }

        public static UsersManager GetUserManager(IUserRepository repository)
        {
            //logger = GetLoggerMock<UserRepository>(output);

            return new UsersManager(repository);
        }

        public static void SetAutoPropertyBackingField<T>(T obj, string propertyName, object value)
        {
            var privateField = GetAutoPropertyBackingField(typeof(T).GetProperty(propertyName));
            privateField?.SetValue(obj, value);
        }

        private static FieldInfo? GetAutoPropertyBackingField(PropertyInfo pi)
        {
            if (!pi.CanRead || !pi.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
            {
                return null;
            }

            var backingField = pi.DeclaringType.GetField($"<{pi.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
            {
                return null;
            }

            return backingField;
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
            using (var context = (TContext)Activator.CreateInstance(typeof(TContext), options))
            {
                _ = context.Database.EnsureCreated();
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
