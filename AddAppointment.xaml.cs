using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeWise.Models;
using TimeWise.Services;

namespace TimeWise
{
    /// <summary>
    /// Interaction logic for AddAppointment.xaml
    /// </summary>
    public partial class AddAppointment : Window
    {
        private readonly ICategoryService _categoryService;
        private List<Category> _categories = new();
        private List<Category> _selectedCategories = new(); // 存储选中的多个categories

        public AddAppointment(ICategoryService categoryService)
        {
            InitializeComponent();
            _categoryService = categoryService;
            LoadCurrentTheme();
            InitializeTimeComboBoxes();
            Loaded += AddAppointment_Loaded;
        }

        private void LoadCurrentTheme()
        {
            try
            {
                // 获取主窗口的当前主题
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // 清除现有资源
                    this.Resources.MergedDictionaries.Clear();
                    
                    // 复制主窗口的主题资源
                    foreach (ResourceDictionary dict in mainWindow.Resources.MergedDictionaries)
                    {
                        this.Resources.MergedDictionaries.Add(dict);
                    }
                }
                else
                {
                    // 默认加载暗黑主题
                    var darkTheme = new ResourceDictionary();
                    darkTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                    this.Resources.MergedDictionaries.Add(darkTheme);
                }
            }
            catch (Exception ex)
            {
                // 如果加载主题失败，使用默认样式
                System.Diagnostics.Debug.WriteLine($"Failed to load theme: {ex.Message}");
            }
        }

        private async void AddAppointment_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private void InitializeTimeComboBoxes()
        {
            // Initialize hour combo boxes (0-23)
            for (int i = 0; i < 24; i++)
            {
                startHour.Items.Add(i.ToString("00"));
                endHour.Items.Add(i.ToString("00"));
            }

            // Initialize minute combo boxes (0, 15, 30, 45)
            string[] minutes = { "00", "15", "30", "45" };
            foreach (string minute in minutes)
            {
                startMinute.Items.Add(minute);
                endMinute.Items.Add(minute);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                _categories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                RefreshCategoryComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCategoryComboBox()
        {
            categoryComboBox.Items.Clear();
            
            foreach (var category in _categories)
            {
                var brush = GetBrushFromHex(category.ColorHex);
                var item = new ComboBoxItem
                {
                    Content = category.Name,
                    Background = brush,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = category.Id
                };
                categoryComboBox.Items.Add(item);
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string categoryName = selectedItem.Content.ToString()!;
                var category = _categories.FirstOrDefault(c => c.Name == categoryName);
                
                if (category != null)
                {
                    // 检查是否已经选中这个category
                    if (!_selectedCategories.Any(c => c.Id == category.Id))
                    {
                        _selectedCategories.Add(category);
                        AddSelectedCategoryToDisplay(category);
                    }
                    
                    // 清空ComboBox选择，允许继续选择其他categories
                    categoryComboBox.SelectedIndex = -1;
                }
            }
        }

        private void AddSelectedCategoryToDisplay(Category category)
        {
            // 创建一个显示选中category的Border
            Border categoryBorder = new Border
            {
                Background = GetBrushFromHex(category.ColorHex),
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5),
                Tag = category.Id
            };

            // 创建包含category名称和删除按钮的StackPanel
            StackPanel categoryContent = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Category名称文本，设置为黑色
            TextBlock categoryText = new TextBlock
            {
                Text = category.Name,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black, // 强制设置为黑色
                VerticalAlignment = VerticalAlignment.Center
            };

            // 删除按钮
            Button removeButton = new Button
            {
                Content = "×",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Width = 16,
                Height = 16,
                Margin = new Thickness(5, 0, 0, 0),
                Background = Brushes.Red,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            // 删除按钮点击事件
            removeButton.Click += (sender, e) => RemoveSelectedCategory(category.Id);

            categoryContent.Children.Add(categoryText);
            categoryContent.Children.Add(removeButton);
            categoryBorder.Child = categoryContent;

            // 添加到显示面板
            selectedCategoriesPanel.Children.Add(categoryBorder);
        }

        private void RemoveSelectedCategory(int categoryId)
        {
            // 从选中列表中移除
            _selectedCategories.RemoveAll(c => c.Id == categoryId);

            // 从显示面板中移除对应的UI元素
            var borderToRemove = selectedCategoriesPanel.Children
                .OfType<Border>()
                .FirstOrDefault(b => b.Tag is int id && id == categoryId);

            if (borderToRemove != null)
            {
                selectedCategoriesPanel.Children.Remove(borderToRemove);
            }
        }

        private async void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string newCategoryName = categoryComboBox.Text.Trim();
            
            if (string.IsNullOrEmpty(newCategoryName))
            {
                MessageBox.Show("Please enter a category name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_categories.Any(c => c.Name.Equals(newCategoryName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This category already exists.", "Duplicate Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newCategory = new Category
                {
                    Name = newCategoryName,
                    IsDefault = false
                };

                await _categoryService.CreateCategoryAsync(newCategory);
                await LoadCategoriesAsync();

                // Select the newly added category
                for (int i = 0; i < categoryComboBox.Items.Count; i++)
                {
                    if (categoryComboBox.Items[i] is ComboBoxItem item && item.Content.ToString() == newCategoryName)
                    {
                        categoryComboBox.SelectedIndex = i;
                        break;
                    }
                }

                MessageBox.Show($"Category '{newCategoryName}' added successfully!", "Category Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (categoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int categoryId)
            {
                var category = _categories.FirstOrDefault(c => c.Id == categoryId);
                if (category == null) return;

                // Check if category can be deleted
                if (!await _categoryService.CanDeleteCategoryAsync(categoryId))
                {
                    if (category.IsDefault)
                    {
                        MessageBox.Show("Cannot delete default categories.", "Delete Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete category that has appointments.", "Delete Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    return;
                }

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the category '{category.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _categoryService.DeleteCategoryAsync(categoryId);
                        if (success)
                        {
                            await LoadCategoriesAsync();
                            MessageBox.Show($"Category '{category.Name}' deleted successfully!", "Category Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete category.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Public properties to access the selected values
        public string SelectedTitle => appointmentTitle.Text;
        public DateTime? SelectedDate => appointmentDate.SelectedDate;
        public string SelectedStartTime => $"{startHour.Text}:{startMinute.Text}";
        public string SelectedEndTime => $"{endHour.Text}:{endMinute.Text}";
        public string SelectedDescription => appointmentDescription.Text;
        
        // 多选categories支持
        public List<Category> SelectedCategories => _selectedCategories.ToList();
        public string SelectedCategoriesString => string.Join(", ", _selectedCategories.Select(c => c.Name));
        
        public int SelectedCategoryId
        {
            get
            {
                // 返回第一个选中的category ID，如果没有选中则返回默认值
                return _selectedCategories.FirstOrDefault()?.Id ?? _categories.FirstOrDefault()?.Id ?? 1;
            }
        }

        // 为了保持向后兼容性，保留原有属性但基于新的多选逻辑
        public string SelectedCategory => _selectedCategories.FirstOrDefault()?.Name ?? "";
        public Brush SelectedCategoryColor => _selectedCategories.Any() ? 
            GetBrushFromHex(_selectedCategories.First().ColorHex) : Brushes.LightGray;

        /// <summary>
        /// 设置默认日期
        /// </summary>
        public void SetDefaultDate(DateTime date)
        {
            appointmentDate.SelectedDate = date;
        }

        /// <summary>
        /// 预填充appointment数据用于编辑
        /// </summary>
        public void PreFillAppointmentData(Appointment appointment)
        {
            try
            {
                appointmentTitle.Text = appointment.Title;
                appointmentDescription.Text = appointment.Description ?? "";
                appointmentDate.SelectedDate = appointment.Date;
                
                // 设置时间
                startHour.SelectedItem = appointment.StartTime.Hours.ToString("00");
                startMinute.SelectedItem = appointment.StartTime.Minutes.ToString("00");
                endHour.SelectedItem = appointment.EndTime.Hours.ToString("00");
                endMinute.SelectedItem = appointment.EndTime.Minutes.ToString("00");
                
                // 设置categories - 加载所有相关的categories
                _selectedCategories.Clear();
                selectedCategoriesPanel.Children.Clear();
                
                // 首先添加主category
                var primaryCategory = _categories.FirstOrDefault(c => c.Id == appointment.CategoryId);
                if (primaryCategory != null)
                {
                    _selectedCategories.Add(primaryCategory);
                    AddSelectedCategoryToDisplay(primaryCategory);
                }
                
                // 然后添加通过多对多关系关联的其他categories
                if (appointment.AppointmentCategories?.Any() == true)
                {
                    foreach (var appointmentCategory in appointment.AppointmentCategories)
                    {
                        var category = appointmentCategory.Category ?? _categories.FirstOrDefault(c => c.Id == appointmentCategory.CategoryId);
                        if (category != null && category.Id != appointment.CategoryId && !_selectedCategories.Any(c => c.Id == category.Id))
                        {
                            _selectedCategories.Add(category);
                            AddSelectedCategoryToDisplay(category);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pre-filling appointment data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(appointmentTitle.Text))
                {
                    MessageBox.Show("Please enter a title for the appointment.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (appointmentDate.SelectedDate == null)
                {
                    MessageBox.Show("Please select a date for the appointment.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!_selectedCategories.Any())
                {
                    MessageBox.Show("Please select at least one category for the appointment.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate time logic
                if (!TimeSpan.TryParse($"{startHour.Text}:{startMinute.Text}", out TimeSpan startTime) ||
                    !TimeSpan.TryParse($"{endHour.Text}:{endMinute.Text}", out TimeSpan endTime))
                {
                    MessageBox.Show("Invalid time format.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (endTime <= startTime)
                {
                    MessageBox.Show("End time must be after start time.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🎯 简化方案：完全取消冲突检查，允许用户自由安排时间
                // 这样用户就可以在同一天添加任意数量的预约，无论时间是否重叠

                // Set DialogResult to true to indicate successful save
                this.DialogResult = true;
                
                // Close the window after saving
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}
