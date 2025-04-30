using System;
using System.Windows;
using System.Windows.Documents;
using System.IO;
using System.Text;
using System.Windows.Markup;

namespace WpfLb1
{
    public partial class InfoWindow : Window
    {
        private readonly DatabaseManager _dbManager = new DatabaseManager();

        public InfoWindow(string topicName)
        {
            InitializeComponent();
            LoadInformation(topicName);
        }

        private void LoadInformation(string topicName)
        {
            try
            {
                // Используем новый метод GetTopic вместо GetInformation
                var topic = _dbManager.GetTopic(topicName);

                if (topic == null || string.IsNullOrEmpty(topic.Content))
                {
                    ContentRun.Text = "Информация отсутствует.";
                    return;
                }

                // Отображаем Markdown-контент как есть
                ContentRun.Text = topic.Content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ContentRun.Text = "Ошибка при загрузке информации.";
            }
        }
    }
}
