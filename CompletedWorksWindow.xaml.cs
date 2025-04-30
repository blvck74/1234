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
    /// Логика взаимодействия для CompletedWorksWindow.xaml
    /// </summary>
    public partial class CompletedWorksWindow : Window
    {
        private readonly DatabaseManager _dbManager;

        public CompletedWorksWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;
            LoadWorks();
        }

        private void LoadWorks(string searchTerm = "")
        {
            WorksList.ItemsSource = _dbManager.GetCompletedWorks(searchTerm);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadWorks(SearchBox.Text);
        }

        private void WorksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WorksList.SelectedItem is Work work)
            {
                var mainWindow = Owner as MainWindow;
                mainWindow?.ShowWorkDetailsFromTab(work);
            }
        }

        private void WorksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorksList.SelectedItem is Work selectedWork)
            {
                var mainWindow = Owner as MainWindow;
                mainWindow?.ShowWorkDetailsFromTab(selectedWork);
            }
        }

        private void CompletedWorksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WorksList.SelectedItem is MainWindow.Work selectedWork)
            {
                var mainWindow = Owner as MainWindow;
                mainWindow?.ShowWorkDetailsFromTab(selectedWork);
            }
        }

    }
}
