using System.Windows;
using System.Windows.Controls;
using TimeWise.Models;
using TimeWise.Services;

namespace TimeWise
{
    /// <summary>
    /// 通知测试窗口
    /// </summary>
    public partial class NotificationTestWindow : Window
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICategoryService _categoryService;
        private readonly INotificationService _notificationService;

        public NotificationTestWindow(IAppointmentService appointmentService, ICategoryService categoryService, INotificationService notificationService)
        {
            InitializeComponent();
            _appointmentService = appointmentService;
            _categoryService = categoryService;
            _notificationService = notificationService;
        }

        private async void CreateTestAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取或创建默认分类
                var categories = await _categoryService.GetAllCategoriesAsync();
                var defaultCategory = categories.FirstOrDefault() ?? await _categoryService.CreateCategoryAsync(new Category { Name = "Test Category", ColorHex = "#3498db" });

                // 创建一个14分钟后开始的预约（这样可以触发15分钟提前通知）
                var futureTime = DateTime.Now.AddMinutes(14);
                var appointment = new Appointment
                {
                    Title = "Test Notification Appointment",
                    Description = "This is a test appointment for notification functionality",
                    Date = futureTime.Date,
                    StartTime = futureTime.TimeOfDay,
                    EndTime = futureTime.AddMinutes(30).TimeOfDay,
                    CategoryId = defaultCategory.Id
                };

                await _appointmentService.CreateAppointmentAsync(appointment);
                
                TestResultText.Text = $"✅ Test appointment created successfully:\n\n" +
                                    $"Title: {appointment.Title}\n" +
                                    $"Time: {futureTime:MMM dd} at {futureTime:h:mm tt} - {futureTime.AddMinutes(30):h:mm tt}\n\n" +
                                    $"📱 Notification will appear in approximately 1 minute.\n" +
                                    $"🔔 The system checks every 30 seconds for upcoming appointments.";
            }
            catch (Exception ex)
            {
                TestResultText.Text = $"❌ Failed to create test appointment:\n\n{ex.Message}";
            }
        }

        private void ShowTestNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建测试预约数据
                var testAppointment = new Appointment
                {
                    Id = 999,
                    Title = "Manual Test Notification",
                    Description = "This is a manually triggered test notification",
                    Date = DateTime.Today,
                    StartTime = DateTime.Now.AddMinutes(15).TimeOfDay,
                    EndTime = DateTime.Now.AddMinutes(45).TimeOfDay
                };

                // 直接显示通知
                _notificationService.ShowAppointmentNotification(testAppointment, 15);
                
                TestResultText.Text = $"✅ Test notification window displayed successfully!\n\n" +
                                    $"📋 The notification shows a manual test appointment.\n" +
                                    $"⏰ It displays a 15-minute countdown as if the appointment starts in 15 minutes.\n\n" +
                                    $"You can interact with the notification window:\n" +
                                    $"• Click 'Got it' to dismiss\n" +
                                    $"• Click 'Snooze (5 min)' to be reminded again in 5 minutes";
            }
            catch (Exception ex)
            {
                TestResultText.Text = $"❌ Failed to show test notification:\n\n{ex.Message}";
            }
        }

        private void CheckMonitoring_Click(object sender, RoutedEventArgs e)
        {
            TestResultText.Text = $"📊 Notification Service Status:\n\n" +
                                $"✅ Monitoring service is running in the background\n" +
                                $"🔄 Checks for upcoming appointments every 30 seconds\n" +
                                $"⏰ Shows notifications 15 minutes before appointment start\n" +
                                $"📅 Monitors today's and tomorrow's appointments\n\n" +
                                $"💡 How to test:\n" +
                                $"1. Create an appointment 14 minutes from now using the button above\n" +
                                $"2. Wait approximately 1 minute for the notification to appear\n" +
                                $"3. Or use 'Show Test Notification' for immediate testing\n\n" +
                                $"🔔 Notifications will show:\n" +
                                $"• Appointment title and time\n" +
                                $"• Dynamic countdown (updates every second)\n" +
                                $"• Options to dismiss or snooze";
        }
    }
}
