using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace WpfLb1
{
    public partial class AdminWindow : Window
    {
        private ObservableCollection<string> _sections;
        private DatabaseManager _dbManager;

        public AdminWindow(ObservableCollection<string> sections)
        {
            InitializeComponent();
            _sections = sections;
            _dbManager = new DatabaseManager();
        }

        private void OnAddSectionButtonClick(object sender, RoutedEventArgs e)
        {
            var addSectionWindow = new AddSectionWindow();
            addSectionWindow.ShowDialog();
        }

        private void OnOpenDeleteSectionWindowClick(object sender, RoutedEventArgs e)
        {
            var deleteSectionWindow = new DeleteSectionWindow();
            deleteSectionWindow.ShowDialog();
        }
        

        
        private void OnOpenServiceManagement_Click(object sender, RoutedEventArgs e)
        {
            var serviceManagementWindow = new ServiceManagementWindow(_dbManager)
            {
                Owner = this
            };
            serviceManagementWindow.ShowDialog();
        }
    }
}
    

