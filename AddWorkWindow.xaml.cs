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
using static WpfLb1.MainWindow;

namespace WpfLb1
{
    /// <summary>
    /// Логика взаимодействия для AddWorkWindow.xaml
    /// </summary>
    public partial class AddWorkWindow : Window
    {
        private readonly DatabaseManager _dbManager;

        public AddWorkWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;

            // Загружаем список сервисов
            ServiceComboBox.ItemsSource = _dbManager.GetServices();
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var startTime = TimeSpan.Parse(StartTimePicker.Text);
            var endTime = TimeSpan.Parse(EndTimePicker.Text);

            var work = new Work
            {
                ServiceName = ServiceComboBox.SelectedItem.ToString()!,
                IncidentNumber = IncidentTextBox.Text,
                WorkName = WorkNameTextBox.Text,
                StartDate = StartDatePicker.SelectedDate?.Add(startTime) ?? DateTime.MinValue,
                EndDate = EndDatePicker.SelectedDate?.Add(endTime) ?? DateTime.MinValue,
                WorkLink = WorkLinkTextBox.Text,
                HasDowntime = HasDowntimeCheckBox.IsChecked ?? false  // Добавляем это свойство
            };

            try
            {
                _dbManager.AddWork(work);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateInput()
        {
            if (ServiceComboBox.SelectedItem == null ||
                string.IsNullOrEmpty(IncidentTextBox.Text) ||
                string.IsNullOrEmpty(WorkNameTextBox.Text) ||
                StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null ||
                string.IsNullOrEmpty(StartTimePicker.Text) ||
                string.IsNullOrEmpty(EndTimePicker.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверяем формат времени
            if (!TimeSpan.TryParse(StartTimePicker.Text, out TimeSpan startTime) ||
                !TimeSpan.TryParse(EndTimePicker.Text, out TimeSpan endTime))
            {
                MessageBox.Show("Неверный формат времени. Используйте формат ЧЧ:ММ",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
