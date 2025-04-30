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
    public partial class AddSectionWindow : Window
    {
        public AddSectionWindow()
        {
            InitializeComponent();
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var sectionName = SectionNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(sectionName))
            {
                var dbManager = new DatabaseManager();
                dbManager.AddSection(sectionName);
                DashboardHelper.UpdateDashboard();
                MessageBox.Show("Секция добавлена успешно!");
                Close();
            }
            else
            {
                MessageBox.Show("Введите название секции.");
            }
        }
    }
}