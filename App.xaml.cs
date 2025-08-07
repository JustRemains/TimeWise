using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TimeWise.Data;
using TimeWise.Services;
using TimeWise.Utilities;

namespace TimeWise
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                FileLogger.Log("TimeWise Application Starting");
                
                // 检查数据库目录
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TimeWise");
                Directory.CreateDirectory(appDataPath);
                
                string dbPath = Path.Combine(appDataPath, "timewise.db");
                
                // 配置依赖注入
                FileLogger.Log("Configuring dependency injection");
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // 注册数据库上下文
                        services.AddDbContext<TimeWiseDbContext>(options =>
                        {
                            options.UseSqlite($"Data Source={dbPath}");
                        });

                        // 注册服务
                        services.AddScoped<IAppointmentService, AppointmentService>();
                        services.AddScoped<ICategoryService, CategoryService>();
                        services.AddScoped<INoteService, NoteService>();
                        services.AddScoped<DatabaseInitializationService>();

                        // 注册窗口
                        services.AddTransient<MainWindow>();
                        services.AddTransient<AddAppointment>();
                    })
                    .Build();

                await _host.StartAsync();
                FileLogger.Log("Host started successfully");

                // 初始化数据库
                using var scope = _host.Services.CreateScope();
                var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
                await dbInitService.InitializeDatabaseAsync();
                FileLogger.Log("Database initialized successfully");

                // 显示主窗口
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                FileLogger.Log("TimeWise Application Started Successfully");

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Application startup", ex);
                MessageBox.Show($"Application failed to start: {ex.Message}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            FileLogger.Log("TimeWise Application Exiting");
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static T GetService<T>() where T : class
        {
            var app = Current as App;
            return app?._host?.Services.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
        }
    }
}
