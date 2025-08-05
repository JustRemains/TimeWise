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

        public AddAppointment(ICategoryService categoryService)
        {
            InitializeComponent();
            _categoryService = categoryService;
            InitializeTimeComboBoxes();
            Loaded += AddAppointment_Loaded;
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
                    selectedCategoryBorder.Background = GetBrushFromHex(category.ColorHex);
                    selectedCategoryText.Text = categoryName;
                    selectedCategoryBorder.Visibility = Visibility.Visible;
                }
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
                            selectedCategoryBorder.Visibility = Visibility.Collapsed;
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
        public string SelectedCategory => selectedCategoryText.Text;
        public string SelectedDescription => appointmentDescription.Text;
        public Brush SelectedCategoryColor => selectedCategoryBorder.Background;
        
        public int SelectedCategoryId
        {
            get
            {
                if (categoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int categoryId)
                {
                    return categoryId;
                }
                return _categories.FirstOrDefault()?.Id ?? 1; // Return first category ID as fallback
            }
        }

        /// <summary>
        /// 设置默认日期
        /// </summary>
        public void SetDefaultDate(DateTime date)
        {
            appointmentDate.SelectedDate = date;
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

                if (string.IsNullOrWhiteSpace(selectedCategoryText.Text))
                {
                    MessageBox.Show("Please select a category for the appointment.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
