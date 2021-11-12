using System;
using System.IO;
using System.Reflection;
using DbUp;

namespace Avalanche.Security.Server.Persistence
{
    public class DatabaseMigrationManager
    {
        public static string MakeConnectionString(string databasePath) => $"Data Source={databasePath};";

        public bool UpgradeDatabase(string databasePath, Assembly migrationsFromAssembly)
        {
            var upgradeEngineBuilder = DeployChanges.To
                .SQLiteDatabase(MakeConnectionString(databasePath))
                .WithScriptsEmbeddedInAssembly(migrationsFromAssembly)
                .WithTransaction();

            var upgradeEngine = upgradeEngineBuilder.Build();
            if (!upgradeEngine.IsUpgradeRequired())
            {
                return false;
            }

            BackupDatabase(databasePath);

            var result = upgradeEngine.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException("An error occurred upgrading the database, check inner exception for details", result.Error);
            }

            return result.Successful;
        }

        private static void BackupDatabase(string databasePath)
        {
            // If using Sqlite in memory database don't try and back it up
            if (databasePath == ":memory:")
            {
                return;
            }

            var databaseBackupPath = string.Concat(databasePath, "bak_", DateTime.UtcNow.Ticks);

            File.Copy(databasePath, databaseBackupPath);
        }
    }
}
