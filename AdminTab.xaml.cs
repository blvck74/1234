using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfLb1.MainWindow;

namespace WpfLb1
{
    public partial class AdminTab : UserControl
    {
        private readonly DatabaseManager _dbManager;
        private readonly MainWindow _mainWindow;

        public AdminTab()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            _mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()
                          ?? throw new InvalidOperationException("MainWindow is not available.");
        }

        private void OnOpenAdminClick(object sender, RoutedEventArgs e)
        {
            // Получение списка секций из базы данных  
            var sections = new ObservableCollection<string>(_dbManager.GetSections());

            // Открытие окна админки с передачей списка секций  
            var adminWindow = new AdminWindow(sections);
            adminWindow.ShowDialog();
        }

        private Button CreateServiceButton(string serviceName, List<Work> works)
        {
            var stackPanel = new StackPanel();
            var serviceTextBlock = new TextBlock
            {
                Text = serviceName,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(serviceTextBlock);

            foreach (var work in works)
            {
                var workButton = new Button
                {
                    Content = work.WorkName,
                    Margin = new Thickness(0, 2, 0, 2),
                    Style = (Style)FindResource("MaterialDesignOutlinedButton")
                };
                
                stackPanel.Children.Add(workButton);
            }

            return new Button
            {
                Content = stackPanel,
                Style = (Style)FindResource("MaterialDesignRaisedButton"),
                Margin = new Thickness(5),
                Width = 200
            };
        }
    }
}
