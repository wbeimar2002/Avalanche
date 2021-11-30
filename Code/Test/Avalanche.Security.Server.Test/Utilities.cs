using System;
using System.Reflection;
using AutoMapper;
using Avalanche.Security.Server.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Avalanche.Security.Server.Test
{
    public class Utilities
    {
        public static UserRepository GetRepository(DbContextOptions<SecurityDbContext> options) => new UserRepository(GetDatabaseWriter(options));

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

        public static SecurityDbContext GetDatabaseWriter(DbContextOptions<SecurityDbContext> options) => new SecurityDbContext(options);

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
    }
}