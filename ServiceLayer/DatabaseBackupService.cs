using System;
using System.IO;
using RepairAndMaintenanceApp.DataLayer;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public static class DatabaseBackupService
    {
        public static string CreateBackup(string destinationDirectory)
        {
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                throw new ArgumentException("Select a folder for the database backup.", nameof(destinationDirectory));
            }

            var directory = Path.GetFullPath(destinationDirectory);
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"The selected backup folder does not exist: {directory}");
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var fileName = $"RepairMaintenanceAccounting_{timestamp}.db";
            var backupPath = Path.Combine(directory, fileName);
            var suffix = 1;
            while (File.Exists(backupPath))
            {
                backupPath = Path.Combine(directory, $"RepairMaintenanceAccounting_{timestamp}_{suffix++}.db");
            }

            DatabaseBackupDataAccess.CreateBackup(backupPath);
            return backupPath;
        }
    }
}
