using Microsoft.EntityFrameworkCore;
using System.IO;
using TimeWise.Data;
using TimeWise.Models;
using TimeWise.Services;

namespace TimeWise.Utilities
{
    /// <summary>
    /// ???????
    /// </summary>
    public static class DatabaseDiagnostics
    {
        /// <summary>
        /// ??????????
        /// </summary>
        public static async Task<bool> RunFullDiagnosticsAsync()
        {
            Console.WriteLine("=== Database Diagnostics Starting ===");
            
            try
            {
                // 1. ??????????
                Console.WriteLine("1. Checking database path and permissions...");
                if (!CheckDatabasePath())
                {
                    return false;
                }
                
                // 2. ???????
                Console.WriteLine("2. Testing database connection...");
                if (!await TestDatabaseConnectionAsync())
                {
                    return false;
                }
                
                // 3. ???????
                Console.WriteLine("3. Validating database structure...");
                if (!await ValidateDatabaseStructureAsync())
                {
                    return false;
                }
                
                // 4. ??????
                Console.WriteLine("4. Checking seed data...");
                if (!await CheckSeedDataAsync())
                {
                    return false;
                }
                
                // 5. ??????
                Console.WriteLine("5. Testing basic operations...");
                if (!await TestBasicOperationsAsync())
                {
                    return false;
                }
                
                Console.WriteLine("=== Database Diagnostics Completed Successfully ===");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Diagnostics failed with exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        private static bool CheckDatabasePath()
        {
            try
            {
                var dbPath = DatabaseUtilities.GetDatabasePath();
                Console.WriteLine($"   Database path: {dbPath}");
                
                var directory = Path.GetDirectoryName(dbPath);
                Console.WriteLine($"   Directory: {directory}");
                Console.WriteLine($"   Directory exists: {Directory.Exists(directory)}");
                
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"   Creating directory...");
                    Directory.CreateDirectory(directory!);
                }
                
                Console.WriteLine($"   Database file exists: {File.Exists(dbPath)}");
                
                if (File.Exists(dbPath))
                {
                    var fileInfo = new FileInfo(dbPath);
                    Console.WriteLine($"   Database size: {DatabaseUtilities.FormatByteSize(fileInfo.Length)}");
                    Console.WriteLine($"   Last modified: {fileInfo.LastWriteTime}");
                    
                    // ??????
                    try
                    {
                        using var stream = File.OpenWrite(dbPath);
                        Console.WriteLine($"   Database is writable: True");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   Database is writable: False - {ex.Message}");
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Path check failed: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                var dbPath = DatabaseUtilities.GetDatabasePath();
                var options = new DbContextOptionsBuilder<TimeWiseDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;
                
                using var context = new TimeWiseDbContext(options);
                
                Console.WriteLine($"   Testing connection...");
                bool canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"   Can connect: {canConnect}");
                
                if (!canConnect)
                {
                    Console.WriteLine($"   Attempting to create database...");
                    await context.Database.EnsureCreatedAsync();
                    canConnect = await context.Database.CanConnectAsync();
                    Console.WriteLine($"   Can connect after creation: {canConnect}");
                }
                
                return canConnect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Connection test failed: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> ValidateDatabaseStructureAsync()
        {
            try
            {
                var dbPath = DatabaseUtilities.GetDatabasePath();
                var options = new DbContextOptionsBuilder<TimeWiseDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;
                
                using var context = new TimeWiseDbContext(options);
                
                // ???????
                var categoriesCount = await context.Categories.CountAsync();
                Console.WriteLine($"   Categories table accessible: True (count: {categoriesCount})");
                
                var appointmentsCount = await context.Appointments.CountAsync();
                Console.WriteLine($"   Appointments table accessible: True (count: {appointmentsCount})");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Structure validation failed: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> CheckSeedDataAsync()
        {
            try
            {
                var dbPath = DatabaseUtilities.GetDatabasePath();
                var options = new DbContextOptionsBuilder<TimeWiseDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;
                
                using var context = new TimeWiseDbContext(options);
                
                var defaultCategories = await context.Categories
                    .Where(c => c.IsDefault)
                    .ToListAsync();
                
                Console.WriteLine($"   Default categories count: {defaultCategories.Count}");
                
                foreach (var category in defaultCategories)
                {
                    Console.WriteLine($"   - {category.Name} ({category.ColorHex})");
                }
                
                return defaultCategories.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Seed data check failed: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> TestBasicOperationsAsync()
        {
            try
            {
                var dbPath = DatabaseUtilities.GetDatabasePath();
                var options = new DbContextOptionsBuilder<TimeWiseDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;
                
                using var context = new TimeWiseDbContext(options);
                
                // ??????
                var testCategory = new Category
                {
                    Name = $"Test_{DateTime.Now.Ticks}",
                    ColorHex = "#FF0000",
                    IsDefault = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                context.Categories.Add(testCategory);
                await context.SaveChangesAsync();
                Console.WriteLine($"   Test category created with ID: {testCategory.Id}");
                
                // ??????
                var testAppointment = new Appointment
                {
                    Title = $"Test Appointment {DateTime.Now.Ticks}",
                    Description = "Test description",
                    Date = DateTime.Today,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(10),
                    CategoryId = testCategory.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                context.Appointments.Add(testAppointment);
                await context.SaveChangesAsync();
                Console.WriteLine($"   Test appointment created with ID: {testAppointment.Id}");
                
                // ????
                var retrievedAppointment = await context.Appointments
                    .Include(a => a.Category)
                    .FirstOrDefaultAsync(a => a.Id == testAppointment.Id);
                
                if (retrievedAppointment != null)
                {
                    Console.WriteLine($"   Retrieved appointment: {retrievedAppointment.Title}");
                    Console.WriteLine($"   Category: {retrievedAppointment.Category.Name}");
                }
                
                // ??????
                context.Appointments.Remove(testAppointment);
                context.Categories.Remove(testCategory);
                await context.SaveChangesAsync();
                Console.WriteLine($"   Test data cleaned up");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Basic operations test failed: {ex.Message}");
                return false;
            }
        }
    }
}