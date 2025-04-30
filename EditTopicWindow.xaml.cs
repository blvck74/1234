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
    /// Логика взаимодействия для EditTopicWindow.xaml
    /// </summary>
    public partial class EditTopicWindow : Window
    {
        private readonly Topic _topic;

        public EditTopicWindow(Topic topic)
        {
            InitializeComponent();
            _topic = topic;
            ContentEditor.Text = topic.Content;
        }
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (MarkdownPreview != null)
            {
                MarkdownPreview.Markdown = ContentEditor.Text;
            }
        }
        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _topic.Content = ContentEditor.Text;
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

        // Вспомогательные методы для форматирования Markdown
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
            var selection = ContentEditor.SelectedText;
            var text = string.IsNullOrEmpty(selection) ? "текст" : selection;
            ContentEditor.SelectedText = $"[{text}](url)";
        }

        private void WrapSelection(string wrapper)
        {
            var selection = ContentEditor.SelectedText;
            var text = string.IsNullOrEmpty(selection) ? "текст" : selection;
            ContentEditor.SelectedText = $"{wrapper}{text}{wrapper}";
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
