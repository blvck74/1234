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
    public partial class DeleteSectionWindow : Window
    {
        public DeleteSectionWindow()
        {
            InitializeComponent();
            
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedSection = SectionComboBox.SelectedItem as string;
            if (selectedSection != null)
            {
                
                DashboardHelper.UpdateDashboard();
                MessageBox.Show("Секция удалена успешно!");
                Close();
            }
            else
            {
                MessageBox.Show("Выберите секцию.");
            }

        }
    }
}