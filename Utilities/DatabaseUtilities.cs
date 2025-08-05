using Microsoft.EntityFrameworkCore;
using System.IO;
using TimeWise.Data;
using TimeWise.Models;
using TimeWise.Services;

namespace TimeWise.Utilities
{
    /// <summary>
    /// ????????
    /// </summary>
    public static class DatabaseUtilities
    {
        /// <summary>
        /// ?????????
        /// </summary>
        public static string GetDatabasePath()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TimeWise");
            
            Directory.CreateDirectory(appDataPath);
            return Path.Combine(appDataPath, "timewise.db");
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public static bool DatabaseExists()
        {
            return File.Exists(GetDatabasePath());
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        public static long GetDatabaseSize()
        {
            var dbPath = GetDatabasePath();
            return File.Exists(dbPath) ? new FileInfo(dbPath).Length : 0;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public static string FormatByteSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }

        /// <summary>
        /// ???????
        /// </summary>
        public static string BackupDatabase(string? backupPath = null)
        {
            var sourcePath = GetDatabasePath();
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Database file not found.");

            if (string.IsNullOrEmpty(backupPath))
            {
                var backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TimeWise Backups");
                Directory.CreateDirectory(backupDir);
                
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                backupPath = Path.Combine(backupDir, $"timewise_backup_{timestamp}.db");
            }

            File.Copy(sourcePath, backupPath, true);
            return backupPath;
        }

        /// <summary>
        /// ????????
        /// </summary>
        public static void RestoreDatabase(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found.");

            var targetPath = GetDatabasePath();
            File.Copy(backupPath, targetPath, true);
        }

        /// <summary>
        /// ???????
        /// </summary>
        public static async Task<bool> ValidateDatabaseConnectionAsync()
        {
            try
            {
                using var context = new TimeWiseDbContext();
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public static async Task<int> CleanupOldDataAsync(int daysToKeep = 365)
        {
            try
            {
                using var context = new TimeWiseDbContext();
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                var oldAppointments = await context.Appointments
                    .Where(a => a.Date < cutoffDate)
                    .ToListAsync();

                if (oldAppointments.Any())
                {
                    context.Appointments.RemoveRange(oldAppointments);
                    await context.SaveChangesAsync();
                }

                return oldAppointments.Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}