using System;
using System.IO;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;

namespace RepairAndMaintenanceApp.DataLayer
{
    public static class DatabaseBackupDataAccess
    {
        public static void CreateBackup(string backupFilePath)
        {
            var databasePath = SqliteDataAccess.DatabasePath;
            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException("The application database was not found.", databasePath);
            }

            var fullBackupPath = Path.GetFullPath(backupFilePath);
            if (string.Equals(databasePath, fullBackupPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The backup location cannot be the live database file.");
            }

            using (new FileStream(fullBackupPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
            }

            try
            {
                using var source = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadOnly
                }.ToString());
                using var destination = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = fullBackupPath,
                    Mode = SqliteOpenMode.ReadWrite
                }.ToString());

                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
            }
            catch
            {
                File.Delete(fullBackupPath);
                throw;
            }
        }
    }
}
