using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using TimeWise.Data;
using TimeWise.Services;
using TimeWise.Utilities;

namespace TimeWise.Tools
{
    /// <summary>
    /// ???????
    /// </summary>
    public static class DatabaseTestTool
    {
        public static async Task RunTestAsync()
        {
            Console.WriteLine("=== TimeWise Database Test Tool ===");
            
            try
            {
                // 1. ???????
                Console.WriteLine("1. Checking database path...");
                var dbPath = DatabaseUtilities.GetDatabasePath();
                Console.WriteLine($"   Database path: {dbPath}");
                Console.WriteLine($"   Database exists: {File.Exists(dbPath)}");
                
                var directory = Path.GetDirectoryName(dbPath);
                Console.WriteLine($"   Directory exists: {Directory.Exists(directory)}");
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                    Console.WriteLine($"   Created directory: {directory}");
                }
                
                // 2. ???????
                Console.WriteLine("\n2. Testing database connection...");
                var options = new DbContextOptionsBuilder<TimeWiseDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .Options;
                
                using var context = new TimeWiseDbContext(options);
                
                bool canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"   Can connect: {canConnect}");
                
                if (!canConnect)
                {
                    Console.WriteLine("   Creating database...");
                    await context.Database.EnsureCreatedAsync();
                    canConnect = await context.Database.CanConnectAsync();
                    Console.WriteLine($"   Can connect after creation: {canConnect}");
                }
                
                // 3. ???
                Console.WriteLine("\n3. Checking tables...");
                try
                {
                    var categoryCount = await context.Categories.CountAsync();
                    Console.WriteLine($"   Categories table accessible: Yes (count: {categoryCount})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Categories table error: {ex.Message}");
                }
                
                try
                {
                    var appointmentCount = await context.Appointments.CountAsync();
                    Console.WriteLine($"   Appointments table accessible: Yes (count: {appointmentCount})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Appointments table error: {ex.Message}");
                }
                
                // 4. ????????
                Console.WriteLine("\n4. Testing database initialization...");
                var dbInitService = new DatabaseInitializationService(context);
                await dbInitService.InitializeDatabaseAsync();
                Console.WriteLine("   Database initialization completed successfully");
                
                // 5. ??????
                Console.WriteLine("\n5. Getting database statistics...");
                var stats = await dbInitService.GetDatabaseStatisticsAsync();
                Console.WriteLine($"   Total appointments: {stats.TotalAppointments}");
                Console.WriteLine($"   Total categories: {stats.TotalCategories}");
                Console.WriteLine($"   Default categories: {stats.DefaultCategories}");
                Console.WriteLine($"   Custom categories: {stats.CustomCategories}");
                Console.WriteLine($"   Database size: {stats.DatabaseSizeFormatted}");
                
                // 6. ????
                Console.WriteLine("\n6. Testing services...");
                var categoryService = new CategoryService(context);
                var appointmentService = new AppointmentService(context);
                
                var categories = await categoryService.GetAllCategoriesAsync();
                Console.WriteLine($"   Category service: {categories.Count()} categories loaded");
                
                var appointments = await appointmentService.GetAppointmentsByDateAsync(DateTime.Today);
                Console.WriteLine($"   Appointment service: {appointments.Count()} appointments for today");
                
                Console.WriteLine("\n=== Database test completed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== Database test failed! ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}