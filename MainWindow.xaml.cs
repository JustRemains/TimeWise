using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using TimeWise.Models;
using TimeWise.Services;
using TimeWise.Utilities;
using System.IO;
using System.Globalization;

namespace TimeWise
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICategoryService _categoryService;
        private readonly INoteService _noteService;
        private readonly INotificationService _notificationService;
        private DateTime _selectedDate;
        private bool _isDarkMode = false;
        private int _selectedCategoryFilter = 0; // 0表示显示所有categories

        public MainWindow(IAppointmentService appointmentService, ICategoryService categoryService, INoteService noteService, INotificationService notificationService)
        {
            FileLogger.Log("MainWindow constructor called");
            try
            {
                // Load theme preference BEFORE InitializeComponent
                LoadThemePreference();
                
                InitializeComponent();
                FileLogger.Log("InitializeComponent completed");
                
                _appointmentService = appointmentService;
                _categoryService = categoryService;
                _noteService = noteService;
                _notificationService = notificationService;
                _selectedDate = DateTime.Today;
                
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing; // 添加窗口关闭事件处理
                
                // 连接日历选择事件
                var calendar = FindName("MainCalendar") as TimeWise.Controls.SimpleCalendar;
                if (calendar != null)
                {
                    calendar.DateSelected += Calendar_DateSelected;
                    calendar.SelectedDate = DateTime.Today;
                }
                
                // 连接TabControl选择事件
                var tabControl = FindName("MainTabControl") as TabControl;
                if (tabControl != null)
                {
                    tabControl.SelectionChanged += TabControl_SelectionChanged;
                }
                
                FileLogger.Log("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("MainWindow constructor", ex);
                throw;
            }
        }

        private void LoadThemePreference()
        {
            try
            {
                string settingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeWise", "settings.txt");
                if (File.Exists(settingsPath))
                {
                    string themePreference = File.ReadAllText(settingsPath).Trim();
                    _isDarkMode = themePreference.Equals("Dark", StringComparison.OrdinalIgnoreCase);
                    ApplyTheme(_isDarkMode);
                    UpdateThemeButtonAppearance();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LoadThemePreference", ex);
                // If loading fails, use default light theme
                _isDarkMode = false;
            }
        }

        private void SaveThemePreference()
        {
            try
            {
                string settingsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeWise");
                Directory.CreateDirectory(settingsDir);
                string settingsPath = System.IO.Path.Combine(settingsDir, "settings.txt");
                File.WriteAllText(settingsPath, _isDarkMode ? "Dark" : "Light");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("SaveThemePreference", ex);
            }
        }

        private void Darkmode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isDarkMode = !_isDarkMode;
                ApplyTheme(_isDarkMode);
                UpdateThemeButtonAppearance();
                SaveThemePreference();
                FileLogger.Log($"Theme switched to: {(_isDarkMode ? "Dark" : "Light")}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Darkmode_Click", ex);
                MessageBox.Show($"Error switching theme: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsPopup = FindName("SettingsPopup") as Popup;
                if (settingsPopup != null)
                {
                    settingsPopup.IsOpen = !settingsPopup.IsOpen;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Settings_Click", ex);
            }
        }

        private void LightMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isDarkMode = false;
                ApplyTheme(_isDarkMode);
                UpdateThemeButtonAppearance();
                SaveThemePreference();
                FileLogger.Log("Theme switched to Light mode via settings");
                
                // Close the popup
                var settingsPopup = FindName("SettingsPopup") as Popup;
                if (settingsPopup != null)
                {
                    settingsPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LightMode_Click", ex);
                MessageBox.Show($"Error switching to light mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isDarkMode = true;
                ApplyTheme(_isDarkMode);
                UpdateThemeButtonAppearance();
                SaveThemePreference();
                FileLogger.Log("Theme switched to Dark mode via settings");
                
                // Close the popup
                var settingsPopup = FindName("SettingsPopup") as Popup;
                if (settingsPopup != null)
                {
                    settingsPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("DarkMode_Click", ex);
                MessageBox.Show($"Error switching to dark mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("Opening notification test window");
                
                var testWindow = new NotificationTestWindow(_appointmentService, _categoryService, _notificationService);
                testWindow.Owner = this;
                testWindow.Show();
                
                // Close the popup
                var settingsPopup = FindName("SettingsPopup") as Popup;
                if (settingsPopup != null)
                {
                    settingsPopup.IsOpen = false;
                }
                
                FileLogger.Log("Notification test window opened successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("TestNotification_Click", ex);
                MessageBox.Show($"Error opening notification test window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show file save dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"TimeWise_Appointments_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Get all appointments
                    var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                    var allCategories = await _categoryService.GetAllCategoriesAsync();
                    
                    // Create CSV content
                    var csvContent = new StringBuilder();
                    
                    // CSV Header
                    csvContent.AppendLine("Date,Time,Title,Description,Primary Category,Additional Categories,Duration");
                    
                    // CSV Data
                    foreach (var appointment in allAppointments.OrderBy(a => a.Date).ThenBy(a => a.StartTime))
                    {
                        var primaryCategory = allCategories.FirstOrDefault(c => c.Id == appointment.CategoryId)?.Name ?? "None";
                        
                        // Get additional categories
                        var additionalCategories = appointment.AppointmentCategories?
                            .Where(ac => ac.CategoryId != appointment.CategoryId)
                            .Select(ac => allCategories.FirstOrDefault(c => c.Id == ac.CategoryId)?.Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Cast<string>()
                            .ToList() ?? new List<string>();
                        
                        var additionalCategoriesString = string.Join("; ", additionalCategories);
                        
                        // Calculate duration
                        var duration = (appointment.EndTime - appointment.StartTime).TotalMinutes;
                        
                        // Escape quotes and commas in text fields
                        var title = EscapeCsvField(appointment.Title);
                        var description = EscapeCsvField(appointment.Description ?? "");
                        var primaryCategoryEscaped = EscapeCsvField(primaryCategory);
                        var additionalCategoriesEscaped = EscapeCsvField(additionalCategoriesString);
                        
                        csvContent.AppendLine($"{appointment.Date:yyyy-MM-dd},{appointment.StartTime:hh\\:mm},{title},{description},{primaryCategoryEscaped},{additionalCategoriesEscaped},{duration} minutes");
                    }
                    
                    // Write to file
                    await File.WriteAllTextAsync(saveFileDialog.FileName, csvContent.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show($"Appointments successfully exported to:\n{saveFileDialog.FileName}", 
                                  "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    FileLogger.Log($"CSV export completed: {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("ExportCsv_Click", ex);
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            // If field contains comma, quote, or newline, wrap in quotes and escape internal quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }
            
            return field;
        }

        private void UpdateThemeButtonAppearance()
        {
            try
            {
                var button = FindName("Darkmode") as Button;
                if (button != null)
                {
                    // Update button text and tooltip
                    button.Content = _isDarkMode ? "LIGHT" : "DARK";
                    button.ToolTip = _isDarkMode ? "Switch to Light Mode" : "Switch to Dark Mode";
                    
                    // Optional: Change button background to indicate current mode
                    if (_isDarkMode)
                    {
                        button.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)); // Subtle white overlay
                    }
                    else
                    {
                        button.Background = null; // Transparent for light mode
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateThemeButtonAppearance", ex);
            }
        }

        private void ApplyTheme(bool isDark)
        {
            try
            {
                var app = Application.Current;
                var existingDict = app.Resources.MergedDictionaries.FirstOrDefault();
                
                if (existingDict != null)
                {
                    app.Resources.MergedDictionaries.Remove(existingDict);
                }

                var newDict = new ResourceDictionary();
                if (isDark)
                {
                    newDict.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
                else
                {
                    newDict.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }
                
                app.Resources.MergedDictionaries.Add(newDict);
                
                // Force update all UI elements immediately
                this.UpdateLayout();
                this.InvalidateVisual();
                
                // Force apply styles to specific controls
                ForceApplyControlStyles(isDark);
                
                // Update existing appointment cards with new theme
                UpdateExistingCardsTheme(isDark);
                
                // Force refresh the calendar control
                if (MainCalendar != null)
                {
                    MainCalendar.UpdateLayout();
                    MainCalendar.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("ApplyTheme", ex);
            }
        }

        private void ForceApplyControlStyles(bool isDark)
        {
            try
            {
                var foregroundColor = isDark ? Brushes.White : Brushes.Black;
                
                // Force custom calendar styles
                var calendar = FindName("MainCalendar") as TimeWise.Controls.SimpleCalendar;
                if (calendar != null)
                {
                    // The custom calendar will handle its own theming through DynamicResource
                    calendar.UpdateLayout();
                }
                
                // Force TabControl styles
                var tabControl = FindName("MainTabControl") as TabControl;
                if (tabControl != null)
                {
                    tabControl.Foreground = foregroundColor;
                    
                    // Force update each TabItem
                    foreach (TabItem item in tabControl.Items)
                    {
                        item.Foreground = foregroundColor;
                        if (item.Header is TextBlock textBlock)
                        {
                            textBlock.Foreground = foregroundColor;
                        }
                        item.UpdateLayout();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("ForceApplyControlStyles", ex);
            }
        }

        private void UpdateExistingCardsTheme(bool isDark)
        {
            try
            {
                // Update card shadows for current appointments
                UpdateCardShadows(appointmentList, isDark);
                UpdateCardShadows(weekAppointmentList, isDark);
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateExistingCardsTheme", ex);
            }
        }

        private void UpdateCardShadows(StackPanel container, bool isDark)
        {
            try
            {
                foreach (UIElement element in container.Children)
                {
                    if (element is Grid cardGrid)
                    {
                        foreach (UIElement child in cardGrid.Children)
                        {
                            if (child is Border border && border.Effect is DropShadowEffect shadow)
                            {
                                // Update shadow color based on theme
                                shadow.Color = isDark ? Colors.Black : Colors.Gray;
                                shadow.Opacity = isDark ? 0.6 : 0.3;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateCardShadows", ex);
            }
        }

        private async void Calendar_DateSelected(object? sender, DateTime selectedDate)
        {
            try
            {
                _selectedDate = selectedDate;
                FileLogger.Log($"Calendar date selected: {_selectedDate:yyyy-MM-dd}");
                
                // 检查当前是哪个标签页，然后相应地更新视图
                var tabControl = FindName("MainTabControl") as TabControl;
                if (tabControl?.SelectedItem is TabItem selectedTab)
                {
                    string tabHeader = "";
                    if (selectedTab.Header is TextBlock textBlock)
                    {
                        tabHeader = textBlock.Text ?? "";
                    }
                    
                    FileLogger.Log($"Current tab when calendar date selected: {tabHeader}");
                    
                    if (tabHeader == "Week")
                    {
                        // 如果当前在Week视图，更新Week视图
                        FileLogger.Log("Updating Week view after calendar selection");
                        await LoadWeeklyAppointmentsAsync();
                    }
                    else
                    {
                        // 否则更新Day视图
                        FileLogger.Log("Updating Day view after calendar selection");
                        await LoadAppointmentsAsync();
                    }
                }
                else
                {
                    // 默认更新Day视图
                    FileLogger.Log("No tab selected, updating Day view by default");
                    await LoadAppointmentsAsync();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Calendar_DateSelected", ex);
            }
        }

        private async void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                var calendar = sender as System.Windows.Controls.Calendar;
                if (calendar?.SelectedDate.HasValue == true)
                {
                    _selectedDate = calendar.SelectedDate.Value;
                    FileLogger.Log($"Calendar date selected: {_selectedDate:yyyy-MM-dd}");
                    
                    // 检查当前是哪个标签页，然后相应地更新视图
                    var tabControl = FindName("MainTabControl") as TabControl;
                    if (tabControl?.SelectedItem is TabItem selectedTab)
                    {
                        string tabHeader = "";
                        if (selectedTab.Header is TextBlock textBlock)
                        {
                            tabHeader = textBlock.Text ?? "";
                        }
                        
                        if (tabHeader == "Week")
                        {
                            // 如果当前在Week视图，更新Week视图
                            await LoadWeeklyAppointmentsAsync();
                        }
                        else
                        {
                            // 否则更新Day视图
                            await LoadAppointmentsAsync();
                        }
                    }
                    else
                    {
                        // 默认更新Day视图
                        await LoadAppointmentsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Calendar_SelectedDatesChanged", ex);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FileLogger.Log("MainWindow_Loaded called");
            try
            {
                // Force apply theme styles after window is fully loaded
                ForceApplyControlStyles(_isDarkMode);
                
                // Load category filters
                await LoadCategoryFilters();
                
                // Load notes first, independently
                await LoadNotesToLeftPanel();
                
                await LoadAppointmentsAsync();
                
                // Add event handler to close popup when clicking elsewhere
                this.MouseDown += MainWindow_MouseDown;
                
                // Start notification monitoring
                _notificationService.StartNotificationMonitoring();
                FileLogger.Log("Notification monitoring started");
                
                FileLogger.Log("MainWindow loaded successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("MainWindow_Loaded", ex);
                MessageBox.Show($"Error loading main window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var settingsPopup = FindName("SettingsPopup") as Popup;
                if (settingsPopup != null && settingsPopup.IsOpen)
                {
                    // Check if click is outside the popup
                    var settingsButton = FindName("SettingsButton") as Button;
                    if (settingsButton != null)
                    {
                        var position = e.GetPosition(this);
                        var buttonBounds = new Rect(settingsButton.TranslatePoint(new Point(0, 0), this), 
                                                   new Size(settingsButton.ActualWidth, settingsButton.ActualHeight));
                        
                        if (!buttonBounds.Contains(position))
                        {
                            settingsPopup.IsOpen = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("MainWindow_MouseDown", ex);
            }
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                FileLogger.Log("MainWindow closing, saving current notes...");
                // 在窗口关闭之前保存当前Notes内容
                await SaveNotesFromLeftPanel();
                FileLogger.Log("Notes saved successfully during window closing");
                
                // Stop notification monitoring
                _notificationService.StopNotificationMonitoring();
                FileLogger.Log("Notification monitoring stopped");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("MainWindow_Closing", ex);
                // 即使保存失败也继续关闭窗口，但记录错误
            }
        }

        private async void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("AddAppointment_Click called");
                var addAppointmentWindow = new AddAppointment(_categoryService);
                
                // 设置默认日期为当前选择的日期
                addAppointmentWindow.SetDefaultDate(_selectedDate);
                
                // Show dialog and check if user clicked Save
                if (addAppointmentWindow.ShowDialog() == true)
                {
                    FileLogger.Log("Creating appointment from dialog data");
                    
                    // 获取选中的所有categories
                    var selectedCategories = addAppointmentWindow.SelectedCategories;
                    var primaryCategoryId = addAppointmentWindow.SelectedCategoryId;
                    
                    // Create appointment from dialog data  
                    var appointment = new Appointment
                    {
                        Title = addAppointmentWindow.SelectedTitle,
                        Description = addAppointmentWindow.SelectedDescription,
                        Date = addAppointmentWindow.SelectedDate ?? _selectedDate,
                        StartTime = TimeSpan.Parse(addAppointmentWindow.SelectedStartTime),
                        EndTime = TimeSpan.Parse(addAppointmentWindow.SelectedEndTime),
                        CategoryId = primaryCategoryId
                    };

                    // 使用新的多category创建方法
                    FileLogger.Log($"Saving appointment with {selectedCategories.Count} categories: {appointment.Title} on {appointment.Date:yyyy-MM-dd}");
                    await _appointmentService.CreateAppointmentWithCategoriesAsync(appointment, selectedCategories.Select(c => c.Id).ToList());

                    // 根据当前选中的标签页刷新相应视图
                    FileLogger.Log("Refreshing UI based on current tab");
                    var tabControl = FindName("MainTabControl") as TabControl;
                    if (tabControl?.SelectedItem is TabItem selectedTab)
                    {
                        // 正确获取TabItem Header中的TextBlock的Text属性
                        string tabHeader = "";
                        if (selectedTab.Header is TextBlock textBlock)
                        {
                            tabHeader = textBlock.Text ?? "";
                        }
                        
                        FileLogger.Log($"Current tab header: {tabHeader}");
                        if (tabHeader == "Week")
                        {
                            FileLogger.Log("Refreshing Week view");
                            await LoadWeeklyAppointmentsAsync();
                        }
                        else
                        {
                            FileLogger.Log("Refreshing Day view");
                            await LoadAppointmentsAsync();
                        }
                    }
                    else
                    {
                        // 默认刷新Day视图
                        FileLogger.Log("No tab selected, refreshing Day view by default");
                        await LoadAppointmentsAsync();
                    }

                    MessageBox.Show("Appointment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("AddAppointment_Click", ex);
                MessageBox.Show($"Error adding appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                FileLogger.Log($"LoadAppointmentsAsync called for date: {_selectedDate:yyyy-MM-dd}");
                // Clear existing appointments
                appointmentList.Children.Clear();

                // Load appointments for selected date
                FileLogger.Log("Loading appointments from database");
                var appointments = await _appointmentService.GetAppointmentsByDateAsync(_selectedDate);
                
                // Apply category filter
                if (_selectedCategoryFilter > 0)
                {
                    appointments = appointments.Where(a => a.CategoryId == _selectedCategoryFilter);
                }
                
                var filteredAppointments = appointments.ToList();
                FileLogger.Log($"Loaded {filteredAppointments.Count} appointments for {_selectedDate:yyyy-MM-dd} (filtered by category: {_selectedCategoryFilter})");

                if (!filteredAppointments.Any())
                {
                    // 显示一个占位符消息
                    var noAppointmentsText = new TextBlock
                    {
                        Text = _selectedCategoryFilter > 0 ? 
                            $"No appointments for {_selectedDate:yyyy-MM-dd} in selected category" :
                            $"No appointments for {_selectedDate:yyyy-MM-dd}",
                        FontSize = 16,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    noAppointmentsText.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryTextBrush");
                    appointmentList.Children.Add(noAppointmentsText);
                }
                else
                {
                    // Create UI cards for each appointment
                    foreach (var appointment in filteredAppointments)
                    {
                        FileLogger.Log($"Creating card for appointment: {appointment.Title}");
                        CreateAppointmentCard(appointment);
                    }
                }
                
                // 更新标题显示当前日期
                UpdateDateDisplay();
                
                FileLogger.Log("LoadAppointmentsAsync completed successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LoadAppointmentsAsync", ex);
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDateDisplay()
        {
            try
            {
                // 在标题中显示当前选择的日期
                var title = $"TimeWise - {_selectedDate:yyyy-MM-dd}";
                if (_selectedDate.Date == DateTime.Today)
                {
                    title += " (Today)";
                }
                this.Title = title;
                
                // 更新Day视图中的日期显示文本
                if (DayDisplayText != null)
                {
                    DayDisplayText.Text = _selectedDate.ToString("MMMM dd, yyyy");
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateDateDisplay", ex);
            }
        }

        private async void PreviousDayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("PreviousDayButton_Click called");
                
                // 将选中日期向前移动1天
                _selectedDate = _selectedDate.AddDays(-1);
                
                // 更新日历显示
                if (MainCalendar != null)
                {
                    MainCalendar.SelectedDate = _selectedDate;
                }
                
                // 重新加载日视图
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("PreviousDayButton_Click", ex);
                MessageBox.Show($"Error navigating to previous day: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void NextDayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("NextDayButton_Click called");
                
                // 将选中日期向后移动1天
                _selectedDate = _selectedDate.AddDays(1);
                
                // 更新日历显示
                if (MainCalendar != null)
                {
                    MainCalendar.SelectedDate = _selectedDate;
                }
                
                // 重新加载日视图
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("NextDayButton_Click", ex);
                MessageBox.Show($"Error navigating to next day: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAppointmentCard(Appointment appointment) // Day视图卡片
        {
            try
            {
                FileLogger.Log($"Creating appointment card for: {appointment.Title}");
                
                // Create the main grid container
                Grid cardGrid = new Grid
                {
                    Margin = new Thickness(5),
                    Tag = appointment.Id // Store appointment ID for deletion
                };

                // Convert hex color to brush
                Brush categoryColor = GetBrushFromHex(appointment.Category.ColorHex);
                
                // Get appropriate text color based on background brightness
                Brush textColor = GetContrastingTextColor(categoryColor);

                // Create the appointment content with improved layout and text sizes
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = appointment.Title,
                    FontSize = 18, // 增大标题字体
                    FontWeight = FontWeights.Bold,
                    Foreground = textColor,
                    Margin = new Thickness(0, 0, 0, 8) // 添加底部间距
                };

                // 创建信息行的容器
                StackPanel infoPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"📅 {appointment.Date:MM/dd}",
                    FontSize = 12,
                    Foreground = textColor,
                    Margin = new Thickness(0, 0, 15, 0)
                };

                TextBlock timeTextBlock = new TextBlock
                {
                    Text = $"🕐 {appointment.StartTime:hh\\:mm}-{appointment.EndTime:hh\\:mm}",
                    FontSize = 12,
                    Foreground = textColor
                };

                infoPanel.Children.Add(dateTextBlock);
                infoPanel.Children.Add(timeTextBlock);

                // Categories容器 - 显示真正的多个categories
                WrapPanel categoriesPanel = new WrapPanel
                {
                    Margin = new Thickness(0, 0, 0, 5)
                };

                // 获取所有关联的categories
                var allCategories = new List<Category> { appointment.Category }; // 主要category
                
                // 添加通过多对多关系关联的其他categories
                if (appointment.AppointmentCategories.Any())
                {
                    var additionalCategories = appointment.AppointmentCategories
                        .Select(ac => ac.Category)
                        .Where(c => c.Id != appointment.CategoryId) // 排除主要category
                        .ToList();
                    allCategories.AddRange(additionalCategories);
                }

                // 限制显示数量并去重
                var displayCategories = allCategories
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .Take(4)
                    .ToList();

                foreach (var (category, index) in displayCategories.Select((cat, i) => (cat, i)))
                {
                    Border categoryTag = new Border
                    {
                        Background = index == 0 ? 
                            new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)) : // 主category半透明白色
                            new SolidColorBrush(Color.FromArgb(60, 100, 150, 255)), // 额外category蓝色调
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock categoryText = new TextBlock
                    {
                        Text = index == 0 ? $"🏷️ {category.Name}" : $"+ {category.Name}",
                        FontSize = index == 0 ? 10 : 8,
                        FontWeight = index == 0 ? FontWeights.Medium : FontWeights.Normal,
                        Foreground = textColor
                    };

                    categoryTag.Child = categoryText;
                    categoriesPanel.Children.Add(categoryTag);
                }

                // 如果还有更多categories，显示"..."
                var totalCategoryCount = allCategories.GroupBy(c => c.Id).Count();
                if (totalCategoryCount > 4)
                {
                    Border moreTag = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock moreText = new TextBlock
                    {
                        Text = $"...+{totalCategoryCount - 4}",
                        FontSize = 8,
                        FontWeight = FontWeights.Normal,
                        Foreground = textColor
                    };

                    moreTag.Child = moreText;
                    categoriesPanel.Children.Add(moreTag);
                }

                // 描述文本（如果有）
                TextBlock? descriptionTextBlock = null;
                if (!string.IsNullOrWhiteSpace(appointment.Description))
                {
                    descriptionTextBlock = new TextBlock
                    {
                        Text = appointment.Description,
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 5, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = textColor,
                        MaxHeight = 40, // 限制描述高度
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                }

                // 组装内容面板
                StackPanel contentPanel = new StackPanel();
                contentPanel.Children.Add(titleTextBlock);
                contentPanel.Children.Add(infoPanel);
                contentPanel.Children.Add(categoriesPanel);
                
                if (descriptionTextBlock != null)
                {
                    contentPanel.Children.Add(descriptionTextBlock);
                }

                // Create the enhanced border with appointment content
                Border appointmentBorder = new Border
                {
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(15),
                    Background = categoryColor,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(5),
                    Child = contentPanel,
                    Cursor = Cursors.Hand // 添加手型光标提示可点击
                };

                // 添加点击事件以编辑appointment
                appointmentBorder.MouseLeftButtonDown += (sender, e) => EditAppointment(appointment);

                // Apply theme-aware shadow
                var shadowColor = _isDarkMode ? Colors.Black : Colors.Gray;
                var shadowOpacity = _isDarkMode ? 0.6 : 0.3;
                appointmentBorder.Effect = new DropShadowEffect
                {
                    Color = shadowColor,
                    BlurRadius = 6,
                    Opacity = shadowOpacity,
                    ShadowDepth = 3
                };

                // Apply the enhanced card style if it exists
                try
                {
                    appointmentBorder.Style = (Style)FindResource("AppointmentCard");
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    FileLogger.Log("Warning: AppointmentCard style not found, using default styling");
                }

                // Create the enhanced delete button
                Button deleteButton = new Button
                {
                    Content = "×",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Width = 30,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 8, 8, 0),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.DarkRed
                };

                // Create gradient background for delete button
                LinearGradientBrush deleteButtonBrush = new LinearGradientBrush();
                deleteButtonBrush.StartPoint = new Point(0, 0);
                deleteButtonBrush.EndPoint = new Point(1, 1);
                deleteButtonBrush.GradientStops.Add(new GradientStop(Color.FromRgb(220, 20, 60), 0)); // Crimson
                deleteButtonBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 0, 0), 1));   // DarkRed
                deleteButton.Background = deleteButtonBrush;

                // Apply the enhanced circular delete button style if it exists
                try
                {
                    deleteButton.Style = (Style)FindResource("CircularDeleteButton");
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    FileLogger.Log("Warning: CircularDeleteButton style not found, using default styling");
                }

                // Add click event to delete button
                deleteButton.Click += async (sender, e) => await DeleteAppointmentFromDatabase(sender, e, cardGrid);

                // Add both border and button to the grid
                cardGrid.Children.Add(appointmentBorder);
                cardGrid.Children.Add(deleteButton);

                // Add the complete card to the appointment list
                appointmentList.Children.Add(cardGrid);
                
                FileLogger.Log($"Successfully created appointment card for: {appointment.Title}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CreateAppointmentCard", ex);
                MessageBox.Show($"Error creating appointment card: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAppointmentCard(string title, string date, string time, string category, Brush categoryColor)
        {
            // This method is kept for backward compatibility but should not be used with the new database system
            // Instead, use the overload that takes an Appointment object
        }

        private async Task DeleteAppointmentFromDatabase(object sender, RoutedEventArgs e, Grid cardGrid)
        {
            try
            {
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete this appointment?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Get appointment ID from the card's Tag
                    if (cardGrid.Tag is int appointmentId)
                    {
                        FileLogger.Log($"Deleting appointment ID: {appointmentId}");
                        // Delete from database
                        bool success = await _appointmentService.DeleteAppointmentAsync(appointmentId);
                        
                        if (success)
                        {
                            // Remove the card from the UI
                            appointmentList.Children.Remove(cardGrid);
                            MessageBox.Show("Appointment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            FileLogger.Log($"Successfully deleted appointment ID: {appointmentId}");
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete appointment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            FileLogger.Log($"Failed to delete appointment ID: {appointmentId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("DeleteAppointmentFromDatabase", ex);
                MessageBox.Show($"Error deleting appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAppointmentCard_Click(object sender, RoutedEventArgs e)
        {
            // This method handles XAML event bindings for existing appointment cards
            if (sender is Button deleteButton)
            {
                // Find the parent Grid (the appointment card)
                Grid? parentCard = FindParent<Grid>(deleteButton);
                
                if (parentCard != null)
                {
                    // Show confirmation dialog
                    MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to delete this appointment?", 
                        "Confirm Delete", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Remove the card from the appointment list
                        appointmentList.Children.Remove(parentCard);
                        MessageBox.Show("Appointment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void DeleteWeeklyCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton)
            {
                // Find the parent Grid (the weekly card)
                Grid? parentCard = FindParent<Grid>(deleteButton);
                
                if (parentCard != null)
                {
                    // Show confirmation dialog
                    MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to delete this weekly overview?", 
                        "Confirm Delete", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Remove the card from the weekly appointment list
                        weekAppointmentList.Children.Remove(parentCard);
                        MessageBox.Show("Weekly overview deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        // Helper method to find parent control of specific type
        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            
            if (parentObject == null) return null;
            
            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        // Helper method to convert hex color to brush with theme-aware adjustment
        private Brush GetBrushFromHex(string hexColor)
        {
            try
            {
                var brush = (Brush)new BrushConverter().ConvertFromString(hexColor)!;
                
                // In dark mode, lighten very dark colors for better visibility
                if (_isDarkMode && brush is SolidColorBrush solidBrush)
                {
                    var color = solidBrush.Color;
                    var brightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;
                    
                    // If the color is too dark, lighten it
                    if (brightness < 0.3)
                    {
                        var factor = 1.5; // Lighten factor
                        var newR = Math.Min(255, (int)(color.R * factor));
                        var newG = Math.Min(255, (int)(color.G * factor));
                        var newB = Math.Min(255, (int)(color.B * factor));
                        
                        return new SolidColorBrush(Color.FromRgb((byte)newR, (byte)newG, (byte)newB));
                    }
                }
                
                return brush;
            }
            catch
            {
                return _isDarkMode ? Brushes.DarkGray : Brushes.LightGray;
            }
        }

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
                {
                    string tabHeader = "";
                    
                    // Handle TextBlock header
                    if (selectedTab.Header is TextBlock textBlock)
                    {
                        tabHeader = textBlock.Text;
                    }
                    else
                    {
                        tabHeader = selectedTab.Header?.ToString() ?? "";
                    }
                    
                    FileLogger.Log($"Tab changed to: {tabHeader}");
                    
                    if (tabHeader == "Week")
                    {
                        await LoadWeeklyAppointmentsAsync();
                    }
                    else if (tabHeader == "Day")
                    {
                        await LoadAppointmentsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("TabControl_SelectionChanged", ex);
                MessageBox.Show($"Error switching tabs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadWeeklyAppointmentsAsync()
        {
            try
            {
                FileLogger.Log("LoadWeeklyAppointmentsAsync called");
                
                // 清空现有的周视图内容
                weekAppointmentList.Children.Clear();

                // 计算本周的开始和结束日期（以周一为开始）
                DateTime startOfWeek = GetStartOfWeek(_selectedDate);
                DateTime endOfWeek = startOfWeek.AddDays(6);
                
                FileLogger.Log($"Loading weekly appointments from {startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}");

                // 获取本周的所有预约
                var weeklyAppointments = await _appointmentService.GetAppointmentsByDateRangeAsync(startOfWeek, endOfWeek);
                
                // Apply category filter
                if (_selectedCategoryFilter > 0)
                {
                    weeklyAppointments = weeklyAppointments.Where(a => a.CategoryId == _selectedCategoryFilter);
                }
                
                var appointmentsList = weeklyAppointments.ToList();
                
                FileLogger.Log($"Loaded {appointmentsList.Count} appointments for the week (filtered by category: {_selectedCategoryFilter})");

                if (!appointmentsList.Any())
                {
                    // 显示无预约的提示
                    var noAppointmentsText = new TextBlock
                    {
                        Text = _selectedCategoryFilter > 0 ? 
                            $"No appointments for week of {startOfWeek:yyyy-MM-dd} in selected category" :
                            $"No appointments for week of {startOfWeek:yyyy-MM-dd}",
                        FontSize = 16,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    noAppointmentsText.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryTextBrush");
                    weekAppointmentList.Children.Add(noAppointmentsText);
                }
                else
                {
                    // 按日期分组显示预约
                    var groupedAppointments = appointmentsList
                        .GroupBy(a => a.Date.Date)
                        .OrderBy(g => g.Key);

                    foreach (var dayGroup in groupedAppointments)
                    {
                        // 创建每日标题
                        CreateDayHeader(dayGroup.Key, dayGroup.Count());
                        
                        // 为该日的每个预约创建卡片
                        foreach (var appointment in dayGroup.OrderBy(a => a.StartTime))
                        {
                            CreateWeeklyAppointmentCard(appointment);
                        }
                        
                        // 添加日期分隔符
                        AddDaySeparator();
                    }
                }
                
                // 更新标题显示本周信息
                UpdateWeeklyDateDisplay(startOfWeek, endOfWeek);
                
                FileLogger.Log("LoadWeeklyAppointmentsAsync completed successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LoadWeeklyAppointmentsAsync", ex);
                MessageBox.Show($"Error loading weekly appointments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            // 获取本周的周一
            DayOfWeek currentDay = date.DayOfWeek;
            int daysFromMonday = currentDay == DayOfWeek.Sunday ? 6 : (int)currentDay - 1;
            return date.AddDays(-daysFromMonday).Date;
        }

        private void CreateDayHeader(DateTime date, int appointmentCount)
        {
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 15, 5, 5)
            };

            // Get English day name using DayOfWeek enum
            string dayName = date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Monday",
                DayOfWeek.Tuesday => "Tuesday", 
                DayOfWeek.Wednesday => "Wednesday",
                DayOfWeek.Thursday => "Thursday",
                DayOfWeek.Friday => "Friday",
                DayOfWeek.Saturday => "Saturday",
                DayOfWeek.Sunday => "Sunday",
                _ => date.DayOfWeek.ToString()
            };

            var dateText = new TextBlock
            {
                Text = $"{dayName}, {date:yyyy-MM-dd}",
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            
            // Use theme-aware colors
            if (date.Date == DateTime.Today)
            {
                dateText.Foreground = Brushes.DarkBlue; // Keep special color for today
            }
            else
            {
                dateText.SetResourceReference(TextBlock.ForegroundProperty, "TextForegroundBrush");
            }

            var countText = new TextBlock
            {
                Text = $"({appointmentCount} appointment{(appointmentCount != 1 ? "s" : "")})",
                FontSize = 14,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(10, 2, 0, 0)
            };
            countText.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryTextBrush");

            headerPanel.Children.Add(dateText);
            headerPanel.Children.Add(countText);
            
            if (date.Date == DateTime.Today)
            {
                var todayBadge = new Border
                {
                    Background = Brushes.Orange,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(10, 0, 0, 0),
                    Child = new TextBlock
                    {
                        Text = "TODAY",
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    }
                };
                headerPanel.Children.Add(todayBadge);
            }

            weekAppointmentList.Children.Add(headerPanel);
        }

        private void CreateWeeklyAppointmentCard(Appointment appointment) // Week视图卡片
        {
            try
            {
                FileLogger.Log($"Creating weekly appointment card for: {appointment.Title}");
                
                // 使用与Day视图相同的Grid容器
                Grid cardGrid = new Grid
                {
                    Margin = new Thickness(5),
                    Tag = appointment.Id // Store appointment ID for deletion
                };

                // Convert hex color to brush
                Brush categoryColor = GetBrushFromHex(appointment.Category.ColorHex);
                
                // Get appropriate text color based on background brightness
                Brush textColor = GetContrastingTextColor(categoryColor);

                // 创建与Day视图相同的改进布局
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = appointment.Title,
                    FontSize = 18, // 增大标题字体
                    FontWeight = FontWeights.Bold,
                    Foreground = textColor,
                    Margin = new Thickness(0, 0, 0, 8) // 添加底部间距
                };

                // 创建信息行的容器
                StackPanel infoPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"📅 {appointment.Date:MM/dd}",
                    FontSize = 12,
                    Foreground = textColor,
                    Margin = new Thickness(0, 0, 15, 0)
                };

                TextBlock timeTextBlock = new TextBlock
                {
                    Text = $"🕐 {appointment.StartTime:hh\\:mm}-{appointment.EndTime:hh\\:mm}",
                    FontSize = 12,
                    Foreground = textColor
                };

                infoPanel.Children.Add(dateTextBlock);
                infoPanel.Children.Add(timeTextBlock);

                // Categories容器 - 与Day视图保持一致的真实多category显示
                WrapPanel categoriesPanel = new WrapPanel
                {
                    Margin = new Thickness(0, 0, 0, 5)
                };

                // 获取所有关联的categories
                var allCategories = new List<Category> { appointment.Category }; // 主要category
                
                // 添加通过多对多关系关联的其他categories
                if (appointment.AppointmentCategories.Any())
                {
                    var additionalCategories = appointment.AppointmentCategories
                        .Select(ac => ac.Category)
                        .Where(c => c.Id != appointment.CategoryId) // 排除主要category
                        .ToList();
                    allCategories.AddRange(additionalCategories);
                }

                // 限制显示数量并去重
                var displayCategories = allCategories
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .Take(4)
                    .ToList();

                foreach (var (category, index) in displayCategories.Select((cat, i) => (cat, i)))
                {
                    Border categoryTag = new Border
                    {
                        Background = index == 0 ? 
                            new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)) : // 主category半透明白色
                            new SolidColorBrush(Color.FromArgb(60, 100, 150, 255)), // 额外category蓝色调
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock categoryText = new TextBlock
                    {
                        Text = index == 0 ? $"🏷️ {category.Name}" : $"+ {category.Name}",
                        FontSize = index == 0 ? 10 : 8,
                        FontWeight = index == 0 ? FontWeights.Medium : FontWeights.Normal,
                        Foreground = textColor
                    };

                    categoryTag.Child = categoryText;
                    categoriesPanel.Children.Add(categoryTag);
                }

                // 如果还有更多categories，显示"..."
                var totalCategoryCount = allCategories.GroupBy(c => c.Id).Count();
                if (totalCategoryCount > 4)
                {
                    Border moreTag = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock moreText = new TextBlock
                    {
                        Text = $"...+{totalCategoryCount - 4}",
                        FontSize = 8,
                        FontWeight = FontWeights.Normal,
                        Foreground = textColor
                    };

                    moreTag.Child = moreText;
                    categoriesPanel.Children.Add(moreTag);
                }

                // 创建内容面板
                StackPanel contentPanel = new StackPanel();
                contentPanel.Children.Add(titleTextBlock);
                contentPanel.Children.Add(infoPanel);
                contentPanel.Children.Add(categoriesPanel);
                
                // 如果有描述，也显示出来
                if (!string.IsNullOrWhiteSpace(appointment.Description))
                {
                    TextBlock descriptionTextBlock = new TextBlock
                    {
                        Text = appointment.Description,
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 5, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = textColor,
                        MaxHeight = 40, // 限制描述高度
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    contentPanel.Children.Add(descriptionTextBlock);
                }

                // 创建与Day视图相同的边框样式
                Border appointmentBorder = new Border
                {
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(15),
                    Background = categoryColor,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(5),
                    Child = contentPanel,
                    Cursor = Cursors.Hand // 添加手型光标提示可点击
                };

                // 添加点击事件以编辑appointment
                appointmentBorder.MouseLeftButtonDown += (sender, e) => EditAppointment(appointment);

                // Apply theme-aware shadow
                var shadowColor = _isDarkMode ? Colors.Black : Colors.Gray;
                var shadowOpacity = _isDarkMode ? 0.6 : 0.3;
                appointmentBorder.Effect = new DropShadowEffect
                {
                    Color = shadowColor,
                    BlurRadius = 6,
                    Opacity = shadowOpacity,
                    ShadowDepth = 3
                };

                // 应用与Day视图相同的卡片样式
                try
                {
                    appointmentBorder.Style = (Style)FindResource("AppointmentCard");
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    FileLogger.Log("Warning: AppointmentCard style not found, using default styling");
                }

                // 创建与Day视图相同的删除按钮
                Button deleteButton = new Button
                {
                    Content = "×",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Width = 30,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 8, 8, 0),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.DarkRed
                };

                // 创建与Day视图相同的删除按钮渐变背景
                LinearGradientBrush deleteButtonBrush = new LinearGradientBrush();
                deleteButtonBrush.StartPoint = new Point(0, 0);
                deleteButtonBrush.EndPoint = new Point(1, 1);
                deleteButtonBrush.GradientStops.Add(new GradientStop(Color.FromRgb(220, 20, 60), 0)); // Crimson
                deleteButtonBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 0, 0), 1));   // DarkRed
                deleteButton.Background = deleteButtonBrush;

                // 应用与Day视图相同的删除按钮样式
                try
                {
                    deleteButton.Style = (Style)FindResource("CircularDeleteButton");
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    FileLogger.Log("Warning: CircularDeleteButton style not found, using default styling");
                }

                // 添加删除事件处理器
                deleteButton.Click += async (sender, e) => await DeleteAppointmentFromWeekView(sender, e, cardGrid);

                // 组装卡片
                cardGrid.Children.Add(appointmentBorder);
                cardGrid.Children.Add(deleteButton);

                // 添加到周视图列表
                weekAppointmentList.Children.Add(cardGrid);
                
                FileLogger.Log($"Successfully created weekly appointment card for: {appointment.Title}");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CreateWeeklyAppointmentCard", ex);
                MessageBox.Show($"Error creating weekly appointment card: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddDaySeparator()
        {
            var separator = new Border
            {
                Height = 1,
                Margin = new Thickness(5, 5, 5, 10)
            };
            separator.SetResourceReference(Border.BackgroundProperty, "SeparatorBrush");
            weekAppointmentList.Children.Add(separator);
        }

        private void UpdateWeeklyDateDisplay(DateTime startOfWeek, DateTime endOfWeek)
        {
            try
            {
                var title = $"TimeWise - Week of {startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}";
                this.Title = title;
                
                // 更新Week视图中的周显示文本
                if (WeekDisplayText != null)
                {
                    WeekDisplayText.Text = $"Week of {startOfWeek:MMM dd} - {endOfWeek:MMM dd}, {startOfWeek:yyyy}";
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateWeeklyDateDisplay", ex);
            }
        }

        private async void PreviousWeekButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("PreviousWeekButton_Click called");
                
                // 在切换日期之前，先保存当前日期的Notes内容
                // 将选中日期向前移动7天到上一周
                _selectedDate = _selectedDate.AddDays(-7);
                
                // 更新日历显示（如果用户切换回Day视图时能看到正确的选中日期）
                if (MainCalendar != null)
                {
                    MainCalendar.SelectedDate = _selectedDate;
                }
                
                // 重新加载周视图
                await LoadWeeklyAppointmentsAsync();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("PreviousWeekButton_Click", ex);
                MessageBox.Show($"Error navigating to previous week: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileLogger.Log("NextWeekButton_Click called");
                
                // 将选中日期向后移动7天到下一周
                _selectedDate = _selectedDate.AddDays(7);
                
                // 更新日历显示（如果用户切换回Day视图时能看到正确的选中日期）
                if (MainCalendar != null)
                {
                    MainCalendar.SelectedDate = _selectedDate;
                }
                
                // 重新加载周视图
                await LoadWeeklyAppointmentsAsync();
            }
            catch (Exception ex)
            {
                FileLogger.LogException("NextWeekButton_Click", ex);
                MessageBox.Show($"Error navigating to next week: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAppointmentFromWeekView(object sender, RoutedEventArgs e, Grid cardGrid)
        {
            try
            {
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete this appointment?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Get appointment ID from the card's Tag
                    if (cardGrid.Tag is int appointmentId)
                    {
                        FileLogger.Log($"Deleting appointment ID: {appointmentId} from week view");
                        // Delete from database
                        bool success = await _appointmentService.DeleteAppointmentAsync(appointmentId);
                        
                        if (success)
                        {
                            // 刷新整个周视图而不是只移除单个卡片
                            // 这样可以确保日期标题、分隔符等都得到正确更新
                            await LoadWeeklyAppointmentsAsync();
                            
                            MessageBox.Show("Appointment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            FileLogger.Log($"Successfully deleted appointment ID: {appointmentId} and refreshed week view");
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete appointment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            FileLogger.Log($"Failed to delete appointment ID: {appointmentId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("DeleteAppointmentFromWeekView", ex);
                MessageBox.Show($"Error deleting appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private Brush GetContrastingTextColor(Brush backgroundBrush)
        {
            try
            {
                if (backgroundBrush is SolidColorBrush solidBrush)
                {
                    Color color = solidBrush.Color;
                    
                    // Calculate relative luminance using the standard formula
                    double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                    
                    // Return black for light backgrounds, white for dark backgrounds
                    return luminance > 0.5 ? Brushes.Black : Brushes.White;
                }
                
                // Default to black text if we can't determine the background color
                return Brushes.Black;
            }
            catch
            {
                return Brushes.Black;
            }
        }

        private void EditAppointment(Appointment appointment)
        {
            try
            {
                FileLogger.Log($"EditAppointment called for appointment: {appointment.Title}");
                var editWindow = new AddAppointment(_categoryService);
                
                // 设置窗口标题为编辑模式
                editWindow.Title = "Edit Appointment";
                
                // 预填充现有数据
                editWindow.SetDefaultDate(appointment.Date);
                editWindow.PreFillAppointmentData(appointment);
                
                // 显示编辑窗口
                if (editWindow.ShowDialog() == true)
                {
                    // 获取选中的所有categories
                    var selectedCategories = editWindow.SelectedCategories;
                    var primaryCategoryId = editWindow.SelectedCategoryId;
                    
                    // 更新appointment数据
                    appointment.Title = editWindow.SelectedTitle;
                    appointment.Description = editWindow.SelectedDescription;
                    appointment.Date = editWindow.SelectedDate ?? appointment.Date;
                    appointment.StartTime = TimeSpan.Parse(editWindow.SelectedStartTime);
                    appointment.EndTime = TimeSpan.Parse(editWindow.SelectedEndTime);
                    appointment.CategoryId = primaryCategoryId;
                    
                    // 保存到数据库
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 更新appointment基本信息
                            await _appointmentService.UpdateAppointmentAsync(appointment);
                            
                            // 更新多category关系
                            await _appointmentService.UpdateAppointmentCategoriesAsync(appointment.Id, selectedCategories.Select(c => c.Id).ToList());
                            
                            // 在UI线程中刷新显示
                            await Dispatcher.InvokeAsync(async () =>
                            {
                                var tabControl = FindName("MainTabControl") as TabControl;
                                if (tabControl?.SelectedItem is TabItem selectedTab)
                                {
                                    if (selectedTab.Header is TextBlock textBlock && textBlock.Text == "Week")
                                    {
                                        await LoadWeeklyAppointmentsAsync();
                                    }
                                    else
                                    {
                                        await LoadAppointmentsAsync();
                                    }
                                }
                                else
                                {
                                    await LoadAppointmentsAsync();
                                }
                                
                                MessageBox.Show("Appointment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                FileLogger.LogException("EditAppointment - Update", ex);
                                MessageBox.Show($"Error updating appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("EditAppointment", ex);
                MessageBox.Show($"Error opening edit dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCategoryFilters()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                
                // Load Day tab filter
                var dayFilterComboBox = FindName("categoryFilterComboBox") as ComboBox;
                if (dayFilterComboBox != null)
                {
                    dayFilterComboBox.Items.Clear();
                    dayFilterComboBox.Items.Add(new ComboBoxItem { Content = "All Categories", Tag = 0 });
                    
                    foreach (var category in categories)
                    {
                        var item = new ComboBoxItem
                        {
                            Content = category.Name,
                            Tag = category.Id,
                            Background = GetBrushFromHex(category.ColorHex)
                        };
                        dayFilterComboBox.Items.Add(item);
                    }
                    dayFilterComboBox.SelectedIndex = 0;
                }
                
                // Load Week tab filter
                var weekFilterComboBox = FindName("categoryFilterComboBoxWeek") as ComboBox;
                if (weekFilterComboBox != null)
                {
                    weekFilterComboBox.Items.Clear();
                    weekFilterComboBox.Items.Add(new ComboBoxItem { Content = "All Categories", Tag = 0 });
                    
                    foreach (var category in categories)
                    {
                        var item = new ComboBoxItem
                        {
                            Content = category.Name,
                            Tag = category.Id,
                            Background = GetBrushFromHex(category.ColorHex)
                        };
                        weekFilterComboBox.Items.Add(item);
                    }
                    weekFilterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LoadCategoryFilters", ex);
            }
        }

        private async void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    _selectedCategoryFilter = (int)(selectedItem.Tag ?? 0);
                    await LoadAppointmentsAsync();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CategoryFilter_SelectionChanged", ex);
            }
        }

        private async void CategoryFilterWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    _selectedCategoryFilter = (int)(selectedItem.Tag ?? 0);
                    await LoadWeeklyAppointmentsAsync();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("CategoryFilterWeek_SelectionChanged", ex);
            }
        }

        private string _searchQuery = string.Empty;

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _searchQuery = textBox.Text.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(_searchQuery))
                {
                    // If search is empty, just reload normally
                    await LoadAppointmentsAsync();
                    return;
                }

                // Search across all appointments
                var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                var matches = allAppointments
                    .Where(a => a.Title.ToLower().Contains(_searchQuery))
                    .OrderBy(a => a.Date)
                    .ToList();

                if (matches.Any())
                {
                    var firstMatch = matches.First();
                    _selectedDate = firstMatch.Date;

                    // Move calendar to that day
                    if (MainCalendar != null)
                        MainCalendar.SelectedDate = _selectedDate;

                    // Load appointments for that day
                    await LoadAppointmentsAsync();
                }
                else
                {
                    // No matches, clear the list and show message
                    appointmentList.Children.Clear();
                    appointmentList.Children.Add(new TextBlock
                    {
                        Text = "No appointments found",
                        FontSize = 16,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
            }
        }



        #region Notes Functionality

        private bool _isLoadingNotes = false;  // 防止在加载时触发TextChanged事件
        private string _lastLoadedNotesContent = "";  // 记录最后加载的内容，用于检测真实变化

        /// <summary>
        /// 加载所有笔记到左侧Notes面板
        /// </summary>
        private async Task LoadNotesToLeftPanel()
        {
            try
            {
                _isLoadingNotes = true; // 设置标志，防止触发TextChanged
                
                var notes = await _noteService.GetAllNotesAsync();
                
                // 合并所有笔记到一个字符串，按时间排序，用换行分隔
                var combinedNotes = string.Join("\n", notes.Select(n => n.Content));
                
                // 记录当前加载的内容
                _lastLoadedNotesContent = combinedNotes;
                
                // 显示在左侧NotesBox中
                if (NotesBox != null)
                {
                    NotesBox.Text = combinedNotes;
                    
                    // 添加事件处理程序（如果还没添加的话）
                    NotesBox.LostFocus -= NotesBox_LostFocus;
                    NotesBox.LostFocus += NotesBox_LostFocus;
                    
                    NotesBox.TextChanged -= NotesBox_TextChanged;
                    NotesBox.TextChanged += NotesBox_TextChanged;
                }
                
                FileLogger.Log($"Loaded {notes.Count()} notes to left panel");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("LoadNotesToLeftPanel", ex);
                MessageBox.Show($"Error loading notes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoadingNotes = false; // 重置标志
            }
        }

        /// <summary>
        /// Notes文本框失去焦点时自动保存
        /// </summary>
        private async void NotesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await SaveNotesFromLeftPanel();
        }

        /// <summary>
        /// Notes文本框内容改变时延迟保存
        /// </summary>
        private void NotesBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 如果正在加载笔记，忽略文本变化事件
            if (_isLoadingNotes)
                return;
                
            // 创建一个延迟保存机制
            _noteSaveTimer?.Stop();
            _noteSaveTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // 2秒后自动保存
            };
            _noteSaveTimer.Tick += async (s, args) =>
            {
                _noteSaveTimer.Stop();
                await SaveNotesFromLeftPanel();
            };
            _noteSaveTimer.Start();
        }

        private System.Windows.Threading.DispatcherTimer? _noteSaveTimer;

        /// <summary>
        /// 从左侧Notes面板保存笔记
        /// </summary>
        private async Task SaveNotesFromLeftPanel()
        {
            try
            {
                if (NotesBox == null || _isLoadingNotes)
                    return;

                var currentContent = NotesBox.Text?.Trim() ?? "";
                
                // 如果内容与最后加载的内容相同，则无需保存
                if (currentContent == _lastLoadedNotesContent)
                {
                    return;
                }

                // 清除所有现有笔记
                await _noteService.ClearAllNotesAsync();
                
                // 如果有新内容，保存为单个笔记
                if (!string.IsNullOrWhiteSpace(currentContent))
                {
                    var note = new Note
                    {
                        Content = currentContent
                    };

                    await _noteService.AddNoteAsync(note);
                    FileLogger.Log($"Saved notes with {currentContent.Length} characters");
                }
                else
                {
                    FileLogger.Log("Cleared all notes");
                }

                // 更新最后加载的内容
                _lastLoadedNotesContent = currentContent;
            }
            catch (Exception ex)
            {
                FileLogger.LogException("SaveNotesFromLeftPanel", ex);
                MessageBox.Show($"Error saving notes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 刷新Notes显示（仅在用户不活跃时）
        /// </summary>
        private async Task RefreshNotesDisplay()
        {
            try
            {
                // 只有在NotesBox没有焦点时才刷新
                if (NotesBox != null && !NotesBox.IsFocused)
                {
                    await LoadNotesToLeftPanel();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("RefreshNotesDisplay", ex);
            }
        }

        #endregion

        #region Category Display Helper

        /// <summary>
        /// 创建统一样式的category标签
        /// </summary>
        private Border CreateCategoryTag(Category category, Brush textColor)
        {
            Border categoryTag = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), // 统一使用半透明白色
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 5, 0)
            };

            TextBlock categoryText = new TextBlock
            {
                Text = $"🏷️ {category.Name}", // 统一使用标签图标
                FontSize = 10, // 统一字体大小
                FontWeight = FontWeights.Medium, // 统一字体粗细
                Foreground = textColor
            };

            categoryTag.Child = categoryText;
            return categoryTag;
        }

        #endregion
    }
}