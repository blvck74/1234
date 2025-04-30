using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO; // Добавлено для устранения ошибки CS0103
using OfficeOpenXml;
using System.Windows.Documents;
using System.Diagnostics;
using Npgsql;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using static MaterialDesignThemes.Wpf.Theme;

namespace WpfLb1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager = new DatabaseManager();
        private ObservableCollection<DashboardItem> _dashboardItems = new();
        public ObservableCollection<DashboardItem> DashboardItems
        {
            get => _dashboardItems;
            set
            {
                _dashboardItems = value;
                OnPropertyChanged(nameof(DashboardItems));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            _dashboardItems = new ObservableCollection<DashboardItem>();
        }

        public interface IRefreshable
        {
            void RefreshData();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Явно указываем тип TabControl
            if (e.Source is System.Windows.Controls.TabControl)
            {
                var storyboard = (Storyboard)FindResource("TabChangeStoryboard");
                storyboard.Begin(MainTabControl);
            }
        }
        public void ShowWorkDetailsFromTab(Work work)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab &&
                selectedTab.Content is WorksTab worksTab)
            {
                worksTab.ShowWorkDetails(work);
            }
            else
            {
                MessageBox.Show("Вкладка 'Работы' не активна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public class Work
        {
            public int Id { get; set; }
            public required string ServiceName { get; set; }
            public required string IncidentNumber { get; set; }
            public required string WorkName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? WorkLink { get; set; }
            public string Status { get; set; } = "Active";
            public bool HasDowntime { get; set; }
            public TimeSpan TotalPauseDuration { get; set; }
            public DateTime? PauseStartTime { get; set; }
        }
    }
}