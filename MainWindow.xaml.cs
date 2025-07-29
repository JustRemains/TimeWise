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

namespace TimeWise
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            // Logic to add an appointment
            MessageBox.Show("Add Appointment button clicked!");

            AddAppointment addAppointment = new();
            addAppointment.ShowDialog();

            string titleText = addAppointment.appointmentTitle.Text;
            string dateText = addAppointment.date.Text;
            string timeText = addAppointment.time.Text;
            string categoryText = addAppointment.category.Text;

            TextBlock titleTextBlock = new TextBlock
            {
                Text = titleText,
                FontSize = 16,
            };

            TextBlock dateTextBlock = new TextBlock
            {
                Text = dateText,
                FontSize = 12,
            };

            TextBlock timeTextBlock = new TextBlock
            {
                Text = timeText,
                FontSize = 12,
            };

            TextBlock categoryTextBlock = new TextBlock
            {
                Text = categoryText,
                FontSize = 12,
            };

            Border border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = Brushes.LightGray,
                Child = new StackPanel
                {
                    Children =
                    {
                        titleTextBlock,
                        dateTextBlock,
                        timeTextBlock,
                        categoryTextBlock
                    }
                }
            };
            appointmentList.Children.Add(border);
           
        }

        private void DeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            DeleteAppointment deleteAppointment = new();
            deleteAppointment.ShowDialog();


            // Logic to delete an appointment
            MessageBox.Show("Delete Appointment button clicked!");
            // Implement deletion logic here

            if( appointmentList.Children.Count > 0)
            {
                // Remove the last appointment for demonstration purposes
                appointmentList.Children.RemoveAt(appointmentList.Children.Count - 1);
            }
            else
            {
                MessageBox.Show("No appointments to delete.");
            }
        }

    }
}