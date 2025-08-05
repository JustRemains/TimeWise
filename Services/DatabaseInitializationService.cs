using Microsoft.EntityFrameworkCore;
using System.IO;
using TimeWise.Data;

namespace TimeWise.Services
{
    /// <summary>
    /// ????????
    /// </summary>
    public class DatabaseInitializationService
    {
        private readonly TimeWiseDbContext _context;

        public DatabaseInitializationService(TimeWiseDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ??????
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // ???????
                await _context.Database.EnsureCreatedAsync();

                // ?????????
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                }

                // ???????
                await ValidateDataIntegrityAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        private async Task ValidateDataIntegrityAsync()
        {
            // ?????????
            var defaultCategoriesCount = await _context.Categories
                .CountAsync(c => c.IsDefault);

            if (defaultCategoriesCount == 0)
            {
                // ??????????????
                await SeedDefaultCategoriesAsync();
            }

            // ??????????????????
            var orphanedAppointments = await _context.Appointments
                .Where(a => !_context.Categories.Any(c => c.Id == a.CategoryId))
                .ToListAsync();

            if (orphanedAppointments.Any())
            {
                // ????????????
                var defaultCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.IsDefault);

                if (defaultCategory != null)
                {
                    foreach (var appointment in orphanedAppointments)
                    {
                        appointment.CategoryId = defaultCategory.Id;
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        private async Task SeedDefaultCategoriesAsync()
        {
            var defaultCategories = new[]
            {
                new { Name = "Work", ColorHex = "#ADD8E6" },
                new { Name = "Personal", ColorHex = "#90EE90" },
                new { Name = "Health", ColorHex = "#FFFFE0" },
                new { Name = "Hobbies", ColorHex = "#FFDAB9" },
                new { Name = "Meeting", ColorHex = "#DDA0DD" },
                new { Name = "Appointment", ColorHex = "#FFC0CB" },
                new { Name = "Travel", ColorHex = "#B0C4DE" },
                new { Name = "Education", ColorHex = "#F0E68C" }
            };

            foreach (var categoryData in defaultCategories)
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == categoryData.Name);

                if (existingCategory == null)
                {
                    _context.Categories.Add(new Models.Category
                    {
                        Name = categoryData.Name,
                        ColorHex = categoryData.ColorHex,
                        IsDefault = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public async Task<DatabaseStatistics> GetDatabaseStatisticsAsync()
        {
            return new DatabaseStatistics
            {
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                DefaultCategories = await _context.Categories.CountAsync(c => c.IsDefault),
                CustomCategories = await _context.Categories.CountAsync(c => !c.IsDefault),
                DatabaseSizeInBytes = GetDatabaseFileSize()
            };
        }

        /// <summary>
        /// ?????????
        /// </summary>
        private long GetDatabaseFileSize()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TimeWise");
                string dbPath = Path.Combine(appDataPath, "timewise.db");

                if (File.Exists(dbPath))
                {
                    return new FileInfo(dbPath).Length;
                }
            }
            catch
            {
                // ???????0
            }
            return 0;
        }
    }

    /// <summary>
    /// ???????
    /// </summary>
    public class DatabaseStatistics
    {
        public int TotalAppointments { get; set; }
        public int TotalCategories { get; set; }
        public int DefaultCategories { get; set; }
        public int CustomCategories { get; set; }
        public long DatabaseSizeInBytes { get; set; }
        public string DatabaseSizeFormatted => FormatBytes(DatabaseSizeInBytes);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}