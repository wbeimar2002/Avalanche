using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Validators;
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
        private static readonly IPasswordHasher _passwordHasher;

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

            // Moq doesn't support type matchers like It.IsAnyType nested inside another type
            // Workaround it to us It.IsAny<object>() and cast to the desired generic Func
            // https://github.com/moq/moq4/issues/1001
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
            var passwordHasher = _passwordHasher;

            /*
              ILogger<UserRepository> logger,
            IMapper mapper,
            SecurityDbContext context,
            IDatabaseWriter<SecurityDbContext> writer,
            IValidator<UserModel> validator,
            IPasswordHasher passwordHasher
             */

            return new UserRepository(
                logger.Object,
                GetMapper(typeof(SecurityDbContext)),
                new SecurityDbContext(options),
                dbWriter.Object,
                new UserValidator(),
                passwordHasher
            );
        }

        //public static LabelRepository GetBuggyLabelRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<LabelRepository>> logger)
        //{
        //    var dbWriter = GetBuggyDatabaseWriter<SecurityDbContext>();
        //    logger = GetLoggerMock<LabelRepository>(output);

        //    return new LabelRepository(
        //        logger.Object,
        //        GetMapper(typeof(SecurityDbContext)),
        //        new SecurityDbContext(options),
        //        dbWriter.Object,
        //        new LabelValidator()
        //    );
        //}

        //public static ProcedureTypeRepository GetBuggyProcedureTypeRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<ProcedureTypeRepository>> logger)
        //{
        //    var dbWriter = GetBuggyDatabaseWriter<SecurityDbContext>();
        //    logger = GetLoggerMock<ProcedureTypeRepository>(output);

        //    return new ProcedureTypeRepository(
        //        logger.Object,
        //        GetMapper(typeof(SecurityDbContext)),
        //        new SecurityDbContext(options),
        //        dbWriter.Object,
        //        new ProcedureTypeValidator()
        //    );
        //}

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
            var passwordHasher = _passwordHasher;

            return new UserRepository(
                logger.Object,
                GetMapper(typeof(SecurityDbContext)),
                new SecurityDbContext(options),
                databaseWriter,
                new UserValidator(),
                passwordHasher
            );
        }

        public static T PickRandom<T>(ICollection<T> enumerable)
        {
            var rand = new Random();
            var randomIndex = rand.Next(0, enumerable.Count - 1);
            return enumerable.ElementAt(randomIndex);
        }

        //public static LabelRepository GetLabelRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<LabelRepository>> logger) =>
        //    GetLabelRepository(options, output, GetDatabaseWriter(options, output), out logger);

        //public static LabelRepository GetLabelRepository(DbContextOptions<SecurityDbContext> options,
        //    ITestOutputHelper output,
        //    DatabaseWriter<SecurityDbContext> databaseWriter,
        //    out Mock<ILogger<LabelRepository>> logger)
        //{
        //    logger = GetLoggerMock<LabelRepository>(output);

        //    return new LabelRepository(
        //        logger.Object,
        //        GetMapper(typeof(SecurityDbContext)),
        //        new SecurityDbContext(options),
        //        databaseWriter,
        //        new LabelValidator()
        //    );
        //}

        //public static ProcedureTypeRepository GetProcedureTypeRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, out Mock<ILogger<ProcedureTypeRepository>> logger) =>
        //                    GetProcedureTypeRepository(options, output, GetDatabaseWriter(options, output), out logger);

        //public static ProcedureTypeRepository GetProcedureTypeRepository(DbContextOptions<SecurityDbContext> options, ITestOutputHelper output, DatabaseWriter<SecurityDbContext> databaseWriter, out Mock<ILogger<ProcedureTypeRepository>> logger)
        //{
        //    logger = GetLoggerMock<ProcedureTypeRepository>(output);

        //    return new ProcedureTypeRepository(
        //        logger.Object,
        //        GetMapper(typeof(SecurityDbContext)),
        //        new SecurityDbContext(options),
        //        databaseWriter,
        //        new ProcedureTypeValidator()
        //    );
        //}

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
