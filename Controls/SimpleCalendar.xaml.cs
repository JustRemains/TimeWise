using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeWise.Controls
{
    public partial class SimpleCalendar : UserControl
    {
        private DateTime _currentDate = DateTime.Today;
        public event EventHandler<DateTime>? DateSelected;

        public DateTime SelectedDate
        {
            get { return _currentDate; }
            set 
            { 
                _currentDate = value;
                UpdateCalendar();
            }
        }

        public SimpleCalendar()
        {
            InitializeComponent();
            UpdateCalendar();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            UpdateCalendar();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            UpdateCalendar();
        }

        private void UpdateCalendar()
        {
            MonthYearText.Text = _currentDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            CalendarGrid.Children.Clear();

            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

            for (int i = 0; i < 42; i++) // 6 weeks * 7 days
            {
                var date = startDate.AddDays(i);
                var button = CreateDayButton(date);
                CalendarGrid.Children.Add(button);
            }
        }

        private Button CreateDayButton(DateTime date)
        {
            var button = new Button
            {
                Content = date.Day.ToString(),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Margin = new Thickness(1),
                FontSize = 12
            };

            // Set foreground color using DynamicResource binding
            button.SetResourceReference(Button.ForegroundProperty, "TextForegroundBrush");

            // Different styling for different date types
            if (date.Month != _currentDate.Month)
            {
                button.Opacity = 0.4; // Previous/next month dates
            }
            else if (date.Date == DateTime.Today)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)); // Today
                button.Foreground = Brushes.White;
                button.FontWeight = FontWeights.Bold;
            }
            else if (date.Date == _currentDate.Date)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 78, 212)); // Selected
                button.Foreground = Brushes.White;
                button.FontWeight = FontWeights.Bold;
            }

            button.Click += (s, e) =>
            {
                _currentDate = date;
                DateSelected?.Invoke(this, date);
                UpdateCalendar();
            };

            return button;
        }
    }
}
