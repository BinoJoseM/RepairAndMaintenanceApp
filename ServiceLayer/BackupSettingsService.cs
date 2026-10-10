using System;
using System.IO;
using System.Text.Json;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class BackupSettings
    {
        public string BackupFolder { get; set; } = string.Empty;
        public bool AutoBackupOnExit { get; set; }
        public DateTime? LastBackupTime { get; set; }
    }

    public static class BackupSettingsService
    {
        public const int ReminderDays = 7;
        private const int KeepAutoBackups = 20;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RepairAndMaintenanceApp",
            "backup-settings.json");

        public static BackupSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    return JsonSerializer.Deserialize<BackupSettings>(File.ReadAllText(SettingsPath)) ?? new BackupSettings();
                }
            }
            catch
            {
            }

            return new BackupSettings();
        }

        public static void Save(BackupSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings));
        }

        public static string CreateBackupAndRecord(string folder)
        {
            var path = DatabaseBackupService.CreateBackup(folder);
            var settings = Load();
            settings.BackupFolder = folder;
            settings.LastBackupTime = DateTime.Now;
            Save(settings);
            return path;
        }

        public static bool IsBackupOverdue()
        {
            var last = Load().LastBackupTime;
            return !last.HasValue || (DateTime.Now - last.Value).TotalDays >= ReminderDays;
        }

        // Runs on app close; never throws so closing is not blocked.
        public static void RunAutoBackupIfEnabled()
        {
            try
            {
                var settings = Load();
                if (!settings.AutoBackupOnExit || string.IsNullOrWhiteSpace(settings.BackupFolder) || !Directory.Exists(settings.BackupFolder))
                {
                    return;
                }

                CreateBackupAndRecord(settings.BackupFolder);
                PruneOldBackups(settings.BackupFolder);
            }
            catch
            {
            }
        }

        private static void PruneOldBackups(string folder)
        {
            var files = new DirectoryInfo(folder).GetFiles("RepairMaintenanceAccounting_*.db");
            Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
            for (var i = KeepAutoBackups; i < files.Length; i++)
            {
                try
                {
                    files[i].Delete();
                }
                catch
                {
                }
            }
        }
    }
}
