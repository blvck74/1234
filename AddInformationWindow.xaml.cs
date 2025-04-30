using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfLb1
{
    public partial class AddInformationWindow : Window
    {
        private readonly string _sectionName;
        private readonly DatabaseManager _dbManager = new DatabaseManager();

        public AddInformationWindow(string sectionName)
        {
            InitializeComponent();
            _sectionName = sectionName;
            ContentTextBox.Focus();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TopicNameTextBox.Text))
                {
                    MessageBox.Show("Введите название темы",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var topic = new Topic
                {
                    Name = TopicNameTextBox.Text,
                    Content = ContentTextBox.Text,
                    Section = _sectionName
                };

                _dbManager.SaveTopic(topic);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Методы для форматирования Markdown
        private void OnBoldClick(object sender, RoutedEventArgs e)
        {
            WrapSelection("**");
        }

        private void OnItalicClick(object sender, RoutedEventArgs e)
        {
            WrapSelection("*");
        }

        private void OnCodeClick(object sender, RoutedEventArgs e)
        {
            WrapSelection("`");
        }

        private void OnLinkClick(object sender, RoutedEventArgs e)
        {
            var selection = ContentTextBox.SelectedText;
            var text = string.IsNullOrEmpty(selection) ? "текст" : selection;
            ContentTextBox.SelectedText = $"[{text}](url)";
        }

        private void WrapSelection(string wrapper)
        {
            var selection = ContentTextBox.SelectedText;
            var text = string.IsNullOrEmpty(selection) ? "текст" : selection;
            ContentTextBox.SelectedText = $"{wrapper}{text}{wrapper}";
        }
    }
}
