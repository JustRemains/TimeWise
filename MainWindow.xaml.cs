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
using TimeWise.Models;
using TimeWise.Services;
using TimeWise.Utilities;

namespace TimeWise
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICategoryService _categoryService;
        private DateTime _selectedDate;

        public MainWindow(IAppointmentService appointmentService, ICategoryService categoryService)
        {
            FileLogger.Log("MainWindow constructor called");
            try
            {
                InitializeComponent();
                FileLogger.Log("InitializeComponent completed");
                
                _appointmentService = appointmentService;
                _categoryService = categoryService;
                _selectedDate = DateTime.Today;
                
                Loaded += MainWindow_Loaded;
                
                // 连接日历选择事件
                var calendar = FindName("MainCalendar") as Calendar;
                if (calendar != null)
                {
                    calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
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

        private async void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                var calendar = sender as Calendar;
                if (calendar?.SelectedDate.HasValue == true)
                {
                    _selectedDate = calendar.SelectedDate.Value;
                    FileLogger.Log($"Calendar date changed to: {_selectedDate:yyyy-MM-dd}");
                    
                    // 根据当前选中的标签页刷新相应视图
                    var tabControl = FindName("MainTabControl") as TabControl;
                    if (tabControl?.SelectedItem is TabItem selectedTab)
                    {
                        string tabHeader = selectedTab.Header?.ToString() ?? "";
                        if (tabHeader == "Week")
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
                        // 默认刷新Day视图
                        await LoadAppointmentsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogException("Calendar_SelectedDatesChanged", ex);
                MessageBox.Show($"Error changing date: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FileLogger.Log("MainWindow_Loaded called");
            try
            {
                await LoadAppointmentsAsync();
                FileLogger.Log("MainWindow loaded successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogException("MainWindow_Loaded", ex);
                MessageBox.Show($"Error loading main window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Create appointment from dialog data  
                    var appointment = new Appointment
                    {
                        Title = addAppointmentWindow.SelectedTitle,
                        Description = addAppointmentWindow.SelectedDescription,
                        Date = addAppointmentWindow.SelectedDate ?? _selectedDate,
                        StartTime = TimeSpan.Parse(addAppointmentWindow.SelectedStartTime),
                        EndTime = TimeSpan.Parse(addAppointmentWindow.SelectedEndTime),
                        CategoryId = addAppointmentWindow.SelectedCategoryId
                    };

                    // Save to database
                    FileLogger.Log($"Saving appointment to database: {appointment.Title} on {appointment.Date:yyyy-MM-dd}");
                    await _appointmentService.CreateAppointmentAsync(appointment);

                    // 根据当前选中的标签页刷新相应视图
                    FileLogger.Log("Refreshing UI based on current tab");
                    var tabControl = FindName("MainTabControl") as TabControl;
                    if (tabControl?.SelectedItem is TabItem selectedTab)
                    {
                        string tabHeader = selectedTab.Header?.ToString() ?? "";
                        if (tabHeader == "Week")
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
                        // 默认刷新Day视图
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
                FileLogger.Log($"Loaded {appointments.Count()} appointments for {_selectedDate:yyyy-MM-dd}");

                if (!appointments.Any())
                {
                    // 显示一个占位符消息
                    var noAppointmentsText = new TextBlock
                    {
                        Text = $"No appointments for {_selectedDate:yyyy-MM-dd}",
                        FontSize = 16,
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    appointmentList.Children.Add(noAppointmentsText);
                }
                else
                {
                    // Create UI cards for each appointment
                    foreach (var appointment in appointments)
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
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateDateDisplay", ex);
            }
        }

        private void CreateAppointmentCard(Appointment appointment)
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

                // Create the appointment content
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = appointment.Title,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                };

                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"Date: {appointment.Date:yyyy-MM-dd}",
                    FontSize = 12,
                };

                TextBlock timeTextBlock = new TextBlock
                {
                    Text = $"Time: {appointment.TimeRange}",
                    FontSize = 12,
                };

                TextBlock categoryTextBlock = new TextBlock
                {
                    Text = $"Category: {appointment.Category.Name}",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };

                // 如果有描述，也显示出来
                StackPanel contentPanel = new StackPanel();
                contentPanel.Children.Add(titleTextBlock);
                contentPanel.Children.Add(dateTextBlock);
                contentPanel.Children.Add(timeTextBlock);
                contentPanel.Children.Add(categoryTextBlock);
                
                if (!string.IsNullOrWhiteSpace(appointment.Description))
                {
                    TextBlock descriptionTextBlock = new TextBlock
                    {
                        Text = $"Description: {appointment.Description}",
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 5, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    };
                    contentPanel.Children.Add(descriptionTextBlock);
                }

                // Convert hex color to brush
                Brush categoryColor = GetBrushFromHex(appointment.Category.ColorHex);

                // Create the enhanced border with appointment content
                Border appointmentBorder = new Border
                {
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(15),
                    Background = categoryColor,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(5),
                    Child = contentPanel
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

        // Helper method to convert hex color to brush
        private Brush GetBrushFromHex(string hexColor)
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString(hexColor)!;
            }
            catch
            {
                return Brushes.LightGray;
            }
        }

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
                {
                    string tabHeader = selectedTab.Header?.ToString() ?? "";
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
                var appointmentsList = weeklyAppointments.ToList();
                
                FileLogger.Log($"Loaded {appointmentsList.Count} appointments for the week");

                if (!appointmentsList.Any())
                {
                    // 显示无预约的提示
                    var noAppointmentsText = new TextBlock
                    {
                        Text = $"No appointments for week of {startOfWeek:yyyy-MM-dd}",
                        FontSize = 16,
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
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

            var dateText = new TextBlock
            {
                Text = $"{date:dddd, yyyy-MM-dd}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = date.Date == DateTime.Today ? Brushes.DarkBlue : Brushes.Black
            };

            var countText = new TextBlock
            {
                Text = $"({appointmentCount} appointment{(appointmentCount != 1 ? "s" : "")})",
                FontSize = 14,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                Margin = new Thickness(10, 2, 0, 0)
            };

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

        private void CreateWeeklyAppointmentCard(Appointment appointment)
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

                // 创建与Day视图相同的内容结构
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = appointment.Title,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                };

                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"Date: {appointment.Date:yyyy-MM-dd}",
                    FontSize = 12,
                };

                TextBlock timeTextBlock = new TextBlock
                {
                    Text = $"Time: {appointment.TimeRange}",
                    FontSize = 12,
                };

                TextBlock categoryTextBlock = new TextBlock
                {
                    Text = $"Category: {appointment.Category.Name}",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };

                // 创建内容面板
                StackPanel contentPanel = new StackPanel();
                contentPanel.Children.Add(titleTextBlock);
                contentPanel.Children.Add(dateTextBlock);
                contentPanel.Children.Add(timeTextBlock);
                contentPanel.Children.Add(categoryTextBlock);
                
                // 如果有描述，也显示出来
                if (!string.IsNullOrWhiteSpace(appointment.Description))
                {
                    TextBlock descriptionTextBlock = new TextBlock
                    {
                        Text = $"Description: {appointment.Description}",
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 5, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    };
                    contentPanel.Children.Add(descriptionTextBlock);
                }

                // 使用与Day视图相同的类别颜色
                Brush categoryColor = GetBrushFromHex(appointment.Category.ColorHex);

                // 创建与Day视图相同的边框样式
                Border appointmentBorder = new Border
                {
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(15),
                    Background = categoryColor,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(5),
                    Child = contentPanel
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
                Background = Brushes.LightGray,
                Margin = new Thickness(5, 5, 5, 10)
            };
            weekAppointmentList.Children.Add(separator);
        }

        private void UpdateWeeklyDateDisplay(DateTime startOfWeek, DateTime endOfWeek)
        {
            try
            {
                var title = $"TimeWise - Week of {startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}";
                this.Title = title;
            }
            catch (Exception ex)
            {
                FileLogger.LogException("UpdateWeeklyDateDisplay", ex);
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
    }
}