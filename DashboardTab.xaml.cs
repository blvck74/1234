using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfLb1
{
    public partial class DashboardTab : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager = new DatabaseManager();
        private string? _currentSection;
        private string? _currentTopic;

        public ObservableCollection<DashboardItem> DashboardItems { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public DashboardTab()
        {
            InitializeComponent();
            DataContext = this;
            LoadDashboard();
        }


        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Загружает данные для дашборда (список секций).
        /// </summary>
        private void LoadDashboard()
        {
            try
            {
                _currentSection = null;
                _currentTopic = null;

                // На экране с секциями все кнопки управления скрыты
                BackButton.Visibility = Visibility.Collapsed;
                AddInfoButton.Visibility = Visibility.Collapsed; // Изменено с Visible на Collapsed
                DeleteButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;

                // Загружаем секции из базы данных
                var sections = _dbManager.GetSections();
                DashboardItems.Clear();

                foreach (var section in sections)
                {
                    DashboardItems.Add(new DashboardItem { Name = section, Section = section });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке дашборда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSection(string sectionName)
        {
            try
            {
                _currentSection = sectionName;
                _currentTopic = null;

                // Показываем кнопки назад и добавления информации
                BackButton.Visibility = Visibility.Visible;
                AddInfoButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;

                var topics = _dbManager.GetTopics(sectionName);
                DashboardItems.Clear();

                foreach (var topic in topics)
                {
                    DashboardItems.Add(new DashboardItem { Name = topic, Section = sectionName });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке секции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTopic(string sectionName, string topicName)
        {
            try
            {
                _currentSection = sectionName;
                _currentTopic = topicName;

                SetControlPanelVisibility(true, true, true, true);

                var topic = _dbManager.GetTopic(topicName);
                DashboardItems.Clear();

                var item = new DashboardItem
                {
                    Name = topic?.Name ?? "Тема не найдена",
                    Section = null!, // null означает что это контент темы
                    Content = topic?.Content ?? "Информация отсутствует." // Добавляем контент
                };
                DashboardItems.Add(item);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (ServicesPanel.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter container)
                        {
                            container.ApplyTemplate();
                            if (container.ContentTemplate.FindName("ContentTextBox", container) is TextBox textBox)
                            {
                                textBox.Text = item.Content; // Используем контент из item
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отображении контента: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке темы: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }




        /// <summary>
        /// Обновляет интерфейс в зависимости от текущего состояния.
        /// </summary>
        public void RefreshData()
        {
            if (_currentTopic != null)
            {
                LoadTopic(_currentSection!, _currentTopic);
            }
            else if (_currentSection != null)
            {
                LoadSection(_currentSection);
            }
            else
            {
                LoadDashboard();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие на элемент дашборда.
        /// </summary>
        private void OnDashboardButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DashboardItem item)
            {
                if (_currentSection == null)
                {
                    LoadSection(item.Section);
                }
                else
                {
                    LoadTopic(_currentSection, item.Name);
                }
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Назад".
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (_currentTopic != null)
            {
                LoadSection(_currentSection!);
            }
            else if (_currentSection != null)
            {
                LoadDashboard();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Добавить информацию".
        /// </summary>
        private void OnAddInfoClick(object sender, RoutedEventArgs e)
        {
            if (_currentSection == null)
            {
                // Если мы на главном экране, открываем окно для создания новой секции
                var sectionName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите название новой секции:",
                    "Новая секция",
                    "");

                if (!string.IsNullOrWhiteSpace(sectionName))
                {
                    try
                    {
                        _dbManager.AddSection(sectionName);
                        MessageBox.Show($"Секция \"{sectionName}\" успешно создана.",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        LoadDashboard();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании секции: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // Если мы внутри секции, открываем окно добавления информации
                var addInfoWindow = new AddInformationWindow(_currentSection);
                addInfoWindow.ShowDialog();
                RefreshData();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Удалить".
        /// </summary>
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (_currentTopic != null && _currentSection != null) // Добавлена проверка на секцию
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить тему \"{_currentTopic}\" с её содержимым?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbManager.DeleteTopic(_currentTopic);

                        // Проверяем, что тема действительно удалена
                        var topics = _dbManager.GetTopics(_currentSection);
                        if (!topics.Contains(_currentTopic))
                        {
                            MessageBox.Show($"Тема \"{_currentTopic}\" успешно удалена.",
                                          "Удаление завершено",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);

                            // Сначала возвращаемся в секцию
                            LoadSection(_currentSection);
                        }
                        else
                        {
                            throw new Exception("Не удалось удалить тему из базы данных");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении темы: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Редактировать".
        /// </summary>
        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (_currentTopic != null)
            {
                var topic = _dbManager.GetTopic(_currentTopic); // Используйте этот метод вместо GetInformation
                if (topic != null)
                {
                    var editWindow = new EditTopicWindow(topic);
                    if (editWindow.ShowDialog() == true)
                    {
                        try
                        {
                            _dbManager.SaveTopic(topic);
                            MessageBox.Show("Изменения успешно сохранены.",
                                          "Успех",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                            LoadTopic(_currentSection!, _currentTopic);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}",
                                          "Ошибка",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Устанавливает видимость кнопок в ControlPanel.
        /// </summary>
        private void SetControlPanelVisibility(bool backVisible, bool addInfoVisible, bool deleteVisible, bool editVisible)
        {
            BackButton.Visibility = backVisible ? Visibility.Visible : Visibility.Collapsed;
            AddInfoButton.Visibility = addInfoVisible ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = deleteVisible ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = editVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        /// <summary>
        /// Тестирует сохранение и извлечение RTF контента
        /// </summary>
        

    }

    public class DashboardItem
    {
        public required string Name { get; set; }
        public required string Section { get; set; }
        public string Content { get; set; } = string.Empty;  // Добавляем свойство для контента
    }
}
