using System;
using System.Windows;
using System.Windows.Threading;
using TimeWise.Models;
using TimeWise.Utilities;

namespace TimeWise
{
    /// <summary>
    /// 预约通知窗口
    /// </summary>
    public partial class AppointmentNotificationWindow : Window
    {
        private readonly Appointment _appointment;
        private readonly double _initialMinutesToStart;
        private DispatcherTimer? _updateTimer;
        private DateTime _notificationStartTime;

        public AppointmentNotificationWindow(Appointment appointment, double minutesToStart)
        {
            InitializeComponent();
            
            _appointment = appointment;
            _initialMinutesToStart = minutesToStart;
            _notificationStartTime = DateTime.Now;
            
            InitializeWindow();
            StartCountdownTimer();
            
            // 设置窗口位置到右下角
            SetWindowPosition();
        }

        private void InitializeWindow()
        {
            try
            {
                // 设置预约信息
                AppointmentTitleText.Text = _appointment.Title;
                
                // 设置时间信息
                var dateStr = _appointment.Date.Date == DateTime.Today ? "Today" : 
                             _appointment.Date.Date == DateTime.Today.AddDays(1) ? "Tomorrow" : 
                             _appointment.Date.ToString("MMM dd");
                
                var startTime = DateTime.Today.Add(_appointment.StartTime).ToString("h:mm tt");
                var endTime = DateTime.Today.Add(_appointment.EndTime).ToString("h:mm tt");
                
                AppointmentTimeText.Text = $"{dateStr} {startTime} - {endTime}";
                
                // 更新倒计时显示
                UpdateCountdownDisplay();

                FileLogger.Log($"Notification window initialized for appointment: {_appointment.Title}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("InitializeWindow", ex);
            }
        }

        private void SetWindowPosition()
        {
            try
            {
                // 确保窗口已经加载完成
                this.UpdateLayout();
                
                // 获取主屏幕工作区域
                var workArea = SystemParameters.WorkArea;
                
                // 计算实际窗口大小（包括边距）
                var actualWidth = this.Width;
                var actualHeight = this.Height;
                
                // 设置窗口位置到右下角，添加合适的边距
                Left = workArea.Right - actualWidth - 40;
                Top = workArea.Bottom - actualHeight - 60;
                
                // 确保窗口不会超出屏幕边界
                if (Left < workArea.Left)
                    Left = workArea.Left + 10;
                if (Top < workArea.Top)
                    Top = workArea.Top + 10;
                
                FileLogger.Log($"Window positioned at: Left={Left}, Top={Top}, Size={actualWidth}x{actualHeight}, WorkArea={workArea}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("SetWindowPosition", ex);
            }
        }

        /// <summary>
        /// 处理标题栏鼠标左键按下事件，启用窗口拖动
        /// </summary>
        private void HeaderBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("HeaderBorder_MouseLeftButtonDown", ex);
            }
        }

        /// <summary>
        /// 处理窗口鼠标左键按下事件，启用整个窗口拖动
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Window_MouseLeftButtonDown", ex);
            }
        }

        private void StartCountdownTimer()
        {
            try
            {
                _updateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1) // 每秒更新一次
                };
                
                _updateTimer.Tick += UpdateTimer_Tick;
                _updateTimer.Start();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("StartCountdownTimer", ex);
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                UpdateCountdownDisplay();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateTimer_Tick", ex);
            }
        }

        private void UpdateCountdownDisplay()
        {
            try
            {
                var now = DateTime.Now;
                var appointmentStart = _appointment.StartDateTime;
                var minutesToStart = (appointmentStart - now).TotalMinutes;

                if (minutesToStart > 0)
                {
                    // 还没开始
                    CountdownLabel.Text = "Time remaining:";
                    
                    if (minutesToStart >= 60)
                    {
                        var hours = (int)(minutesToStart / 60);
                        var minutes = (int)(minutesToStart % 60);
                        if (hours == 1)
                            CountdownText.Text = minutes > 0 ? $"1h {minutes}m" : "1h";
                        else
                            CountdownText.Text = minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
                    }
                    else if (minutesToStart >= 1)
                    {
                        var mins = (int)minutesToStart;
                        CountdownText.Text = mins == 1 ? "1 minute" : $"{mins} minutes";
                    }
                    else
                    {
                        var seconds = (int)(minutesToStart * 60);
                        CountdownText.Text = seconds <= 1 ? "Starting now!" : $"{seconds} seconds";
                    }
                    
                    CountdownText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34)); // #e67e22
                }
                else if (minutesToStart >= -_appointment.Duration.TotalMinutes)
                {
                    // 正在进行中
                    CountdownLabel.Text = "In progress:";
                    var minutesRunning = Math.Abs(minutesToStart);
                    
                    if (minutesRunning >= 60)
                    {
                        var hours = (int)(minutesRunning / 60);
                        var minutes = (int)(minutesRunning % 60);
                        if (hours == 1)
                            CountdownText.Text = minutes > 0 ? $"1h {minutes}m ago" : "1h ago";
                        else
                            CountdownText.Text = minutes > 0 ? $"{hours}h {minutes}m ago" : $"{hours}h ago";
                    }
                    else
                    {
                        var mins = (int)minutesRunning;
                        CountdownText.Text = mins == 1 ? "1m ago" : $"{mins}m ago";
                    }
                    
                    CountdownText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)); // #27ae60
                }
                else
                {
                    // 已结束
                    CountdownLabel.Text = "Finished:";
                    var minutesOverdue = Math.Abs(minutesToStart + _appointment.Duration.TotalMinutes);
                    
                    if (minutesOverdue >= 60)
                    {
                        var hours = (int)(minutesOverdue / 60);
                        var minutes = (int)(minutesOverdue % 60);
                        if (hours == 1)
                            CountdownText.Text = minutes > 0 ? $"1h {minutes}m ago" : "1h ago";
                        else
                            CountdownText.Text = minutes > 0 ? $"{hours}h {minutes}m ago" : $"{hours}h ago";
                    }
                    else
                    {
                        var mins = (int)minutesOverdue;
                        CountdownText.Text = mins == 1 ? "1m ago" : $"{mins}m ago";
                    }
                    
                    CountdownText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)); // #95a5a6
                    
                    // 如果已经结束超过5分钟，自动关闭窗口
                    if (minutesOverdue > 5)
                    {
                        CloseWindow();
                    }
                }

                // 强制UI更新
                CountdownText.UpdateLayout();
                CountdownLabel.UpdateLayout();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateCountdownDisplay", ex);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void SnoozeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建一个5分钟后的提醒
                var snoozeTime = DateTime.Now.AddMinutes(5);
                var appointmentStart = _appointment.StartDateTime;
                var minutesToStartAtSnooze = (appointmentStart - snoozeTime).TotalMinutes;
                
                // 创建延迟任务
                var snoozeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(5)
                };
                
                snoozeTimer.Tick += (s, e) =>
                {
                    snoozeTimer.Stop();
                    
                    // 显示新的通知窗口
                    var snoozeNotification = new AppointmentNotificationWindow(_appointment, minutesToStartAtSnooze);
                    snoozeNotification.Show();
                };
                
                snoozeTimer.Start();
                
                FileLogger.Log($"Snoozed notification for appointment: {_appointment.Title} for 5 minutes");
                CloseWindow();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("SnoozeButton_Click", ex);
            }
        }

        private void CloseWindow()
        {
            try
            {
                _updateTimer?.Stop();
                _updateTimer = null;
                Close();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CloseWindow", ex);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _updateTimer?.Stop();
                _updateTimer = null;
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                FileLogger.LogException("OnClosed", ex);
            }
        }
    }
}
