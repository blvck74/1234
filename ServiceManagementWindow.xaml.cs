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

namespace WpfLb1
{
    /// <summary>
    /// Логика взаимодействия для ServiceManagementWindow.xaml
    /// </summary>
    public partial class ServiceManagementWindow : Window
    {
        private readonly DatabaseManager _dbManager;

        public ServiceManagementWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;
            RefreshServices();
        }

        private void OnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServiceNameTextBox.Text))
            {
                MessageBox.Show("Введите название сервиса", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _dbManager.AddService(ServiceNameTextBox.Text);
                ServiceNameTextBox.Clear();
                RefreshServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сервиса: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serviceName)
            {
                var result = MessageBox.Show($"Удалить сервис '{serviceName}'?",
                                           "Подтверждение",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbManager.DeleteService(serviceName);
                        RefreshServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении сервиса: {ex.Message}",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshServices()
        {
            ServicesList.ItemsSource = _dbManager.GetServices();
        }
    }
}
