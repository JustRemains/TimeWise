using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// 通知服务接口
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 开始通知监控
        /// </summary>
        void StartNotificationMonitoring();

        /// <summary>
        /// 停止通知监控
        /// </summary>
        void StopNotificationMonitoring();

        /// <summary>
        /// 检查即将开始的预约并显示通知
        /// </summary>
        Task CheckAndShowNotificationsAsync();

        /// <summary>
        /// 显示预约通知
        /// </summary>
        /// <param name="appointment">预约信息</param>
        /// <param name="minutesToStart">距离开始的分钟数</param>
        void ShowAppointmentNotification(Appointment appointment, double minutesToStart);

        /// <summary>
        /// 通知显示事件
        /// </summary>
        event EventHandler<AppointmentNotificationEventArgs>? NotificationShown;
    }

    /// <summary>
    /// 预约通知事件参数
    /// </summary>
    public class AppointmentNotificationEventArgs : EventArgs
    {
        public Appointment Appointment { get; set; } = null!;
        public double MinutesToStart { get; set; }
        public DateTime NotificationTime { get; set; }
    }
}
