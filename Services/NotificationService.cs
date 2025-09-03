using System.Windows;
using System.Windows.Threading;
using TimeWise.Models;
using TimeWise.Utilities;

namespace TimeWise.Services
{
    /// <summary>
    /// 通知服务实现
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IAppointmentService _appointmentService;
        private DispatcherTimer? _notificationTimer;
        private readonly HashSet<int> _notifiedAppointments = new();
        private const int CHECK_INTERVAL_SECONDS = 30; // 每30秒检查一次
        private const int NOTIFICATION_MINUTES = 15; // 提前15分钟通知

        public event EventHandler<AppointmentNotificationEventArgs>? NotificationShown;

        public NotificationService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public void StartNotificationMonitoring()
        {
            try
            {
                FileLogger.Log("Starting notification monitoring");
                
                if (_notificationTimer != null)
                {
                    _notificationTimer.Stop();
                    _notificationTimer = null;
                }

                _notificationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS)
                };
                
                _notificationTimer.Tick += async (s, e) => await CheckAndShowNotificationsAsync();
                _notificationTimer.Start();

                FileLogger.Log("Notification monitoring started successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("StartNotificationMonitoring", ex);
            }
        }

        public void StopNotificationMonitoring()
        {
            try
            {
                FileLogger.Log("Stopping notification monitoring");
                
                _notificationTimer?.Stop();
                _notificationTimer = null;
                _notifiedAppointments.Clear();

                FileLogger.Log("Notification monitoring stopped");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("StopNotificationMonitoring", ex);
            }
        }

        public async Task CheckAndShowNotificationsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var checkUntil = now.AddMinutes(NOTIFICATION_MINUTES + 5); // 检查未来20分钟内的预约

                // 获取今天和明天的所有预约
                var todayAppointments = await _appointmentService.GetAppointmentsByDateAsync(now.Date);
                var tomorrowAppointments = await _appointmentService.GetAppointmentsByDateAsync(now.Date.AddDays(1));
                
                var allAppointments = todayAppointments.Concat(tomorrowAppointments);

                foreach (var appointment in allAppointments)
                {
                    // 如果已经通知过这个预约，跳过
                    if (_notifiedAppointments.Contains(appointment.Id))
                        continue;

                    var appointmentStart = appointment.StartDateTime;
                    var minutesToStart = (appointmentStart - now).TotalMinutes;

                    // 检查是否需要显示通知（提前15分钟，或者已经开始但在过去15分钟内）
                    if (minutesToStart <= NOTIFICATION_MINUTES && minutesToStart >= -NOTIFICATION_MINUTES)
                    {
                        _notifiedAppointments.Add(appointment.Id);
                        
                        // 在UI线程中显示通知
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowAppointmentNotification(appointment, minutesToStart);
                        });

                        FileLogger.Log($"Notification shown for appointment: {appointment.Title}, Minutes to start: {minutesToStart:F1}");
                    }
                }

                // 清理已过期的通知记录（超过30分钟的预约）
                CleanupExpiredNotifications(now);
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CheckAndShowNotificationsAsync", ex);
            }
        }

        public void ShowAppointmentNotification(Appointment appointment, double minutesToStart)
        {
            try
            {
                var notificationWindow = new AppointmentNotificationWindow(appointment, minutesToStart);
                notificationWindow.Show();

                // 触发通知显示事件
                NotificationShown?.Invoke(this, new AppointmentNotificationEventArgs
                {
                    Appointment = appointment,
                    MinutesToStart = minutesToStart,
                    NotificationTime = DateTime.Now
                });

                FileLogger.Log($"Notification window shown for appointment: {appointment.Title}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("ShowAppointmentNotification", ex);
            }
        }

        private async void CleanupExpiredNotifications(DateTime now)
        {
            try
            {
                var expiredIds = new List<int>();
                
                foreach (var appointmentId in _notifiedAppointments)
                {
                    var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                    if (appointment != null)
                    {
                        var appointmentEnd = appointment.EndDateTime;
                        if (appointmentEnd.AddMinutes(30) < now) // 预约结束后30分钟清理
                        {
                            expiredIds.Add(appointmentId);
                        }
                    }
                    else
                    {
                        // 如果预约不存在了，也清理
                        expiredIds.Add(appointmentId);
                    }
                }

                foreach (var id in expiredIds)
                {
                    _notifiedAppointments.Remove(id);
                }

                if (expiredIds.Count > 0)
                {
                    FileLogger.Log($"Cleaned up {expiredIds.Count} expired notification records");
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CleanupExpiredNotifications", ex);
            }
        }
    }
}
