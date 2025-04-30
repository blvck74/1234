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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfLb1.MainWindow;

namespace WpfLb1
{
    public partial class WorksTab : UserControl
    {
        private readonly DatabaseManager _dbManager;
        private Window? _detailsWindow;

        public WorksTab()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
        }

        private void OnAddWork_Click(object sender, RoutedEventArgs e)
        {
            var addWorkWindow = new AddWorkWindow(_dbManager);
            if (addWorkWindow.ShowDialog() == true)
            {
                RefreshWorks();
            }
        }

        private void OnDeleteWork_Click(object sender, RoutedEventArgs e)
        {
            // Будет реализовано позже
        }

        private void ServicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServicesList.SelectedItem is string selectedService)
            {
                LoadWorksByService(selectedService);
            }
        }

        public void ShowWorkDetails(Work work)
        {
            var updatedWork = _dbManager.GetWorkById(work.Id);
            if (updatedWork == null)
            {
                MessageBox.Show("Работа не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_detailsWindow == null || !_detailsWindow.IsVisible)
            {
                _detailsWindow = new Window
                {
                    Title = updatedWork.WorkName,
                    Width = 500,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };
            }

            var scrollViewer = new ScrollViewer();
            var mainPanel = new StackPanel { Margin = new Thickness(20) };

            var infoTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                Text = $"Сервис: {updatedWork.ServiceName}\n" +
                       $"Номер инцидента: {updatedWork.IncidentNumber}\n" +
                       $"Название: {updatedWork.WorkName}\n" +
                       $"Статус: {updatedWork.Status}\n" +
                       $"Даунтайм: {(updatedWork.HasDowntime ? "Да" : "Нет")}\n" +
                       $"Дата и время начала: {updatedWork.StartDate:dd.MM.yyyy HH:mm}\n" +
                       $"Дата и время окончания: {updatedWork.EndDate:dd.MM.yyyy HH:mm}",
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(infoTextBox);

            if (updatedWork.Status != "Completed")
            {
                var buttonsPanel = new WrapPanel { Margin = new Thickness(0, 20, 0, 0) };

                var pauseButton = new Button
                {
                    Content = updatedWork.Status == "Paused" ? "Возобновить" : "Пауза",
                    Margin = new Thickness(0, 0, 10, 0)
                };
                pauseButton.Click += (s, e) =>
                {
                    if (updatedWork.Status == "Paused")
                        _dbManager.ResumeWork(updatedWork.Id);
                    else
                        _dbManager.PauseWork(updatedWork.Id);
                    ShowWorkDetails(updatedWork);
                };
                buttonsPanel.Children.Add(pauseButton);

                var extendButton = new Button
                {
                    Content = "Продлить",
                    Margin = new Thickness(0, 0, 10, 0)
                };
                extendButton.Click += (s, e) => ShowExtendWorkDialog(updatedWork);
                buttonsPanel.Children.Add(extendButton);

                var completeButton = new Button
                {
                    Content = "Завершить",
                    Margin = new Thickness(0, 0, 10, 0)
                };
                completeButton.Click += (s, e) => CompleteWork(updatedWork);
                buttonsPanel.Children.Add(completeButton);

                mainPanel.Children.Add(buttonsPanel);
            }

            var logsButton = new Button
            {
                Content = "Логи",
                Margin = new Thickness(0, 10, 0, 0)
            };
            logsButton.Click += (s, e) => ShowWorkLogs(updatedWork.Id);
            mainPanel.Children.Add(logsButton);

            scrollViewer.Content = mainPanel;
            _detailsWindow.Content = scrollViewer;

            if (!_detailsWindow.IsVisible)
            {
                _detailsWindow.Show();
            }
        }

        private void RefreshWorks()
        {
            LoadServicesList(); // Исправлено имя метода
            if (ServicesList.SelectedItem is string selectedService)
            {
                LoadWorksByService(selectedService);
            }
            else if (ServicesList.Items.Count > 0)
            {
                ServicesList.SelectedIndex = 0;
            }
        }

        private void LoadServicesList()
        {
            ServicesList.Items.Clear();
            var services = _dbManager.GetServices();
            foreach (var service in services)
            {
                ServicesList.Items.Add(service);
            }
        }

        private void ShowArchive_Click(object sender, RoutedEventArgs e)
        {
            var archiveWindow = new CompletedWorksWindow(_dbManager)
            {
                Owner = Window.GetWindow(this)
            };
            archiveWindow.ShowDialog();
        }

        private void LoadWorksByService(string serviceName)
        {
            WorksList.Children.Clear();

            var works = _dbManager.GetWorksByService(serviceName)
                .Where(w => w.Status != "Completed")
                .ToList();

            foreach (var work in works)
            {
                var workButton = new Button
                {
                    Content = work.WorkName,
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 40,
                    Style = (Style)FindResource("MaterialDesignRaisedButton")
                };
                workButton.Click += (s, e) => ShowWorkDetails(work);
                WorksList.Children.Add(workButton);
            }
        }

        private void ShowExtendWorkDialog(Work work)
        {
            var extendWindow = new Window
            {
                Title = "Продление работы",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                Style = (Style)FindResource("MaterialDesignWindow")
            };

            var mainStackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            var radioGroup = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

            var hoursRadio = new RadioButton
            {
                Content = "Продлить на часы",
                IsChecked = true,
                Style = (Style)FindResource("MaterialDesignRadioButton"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var dateRadio = new RadioButton
            {
                Content = "Указать новое время окончания",
                Style = (Style)FindResource("MaterialDesignRadioButton"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            radioGroup.Children.Add(hoursRadio);
            radioGroup.Children.Add(dateRadio);

            var hoursPanel = new StackPanel
            {
                Margin = new Thickness(20, 0, 0, 0)
            };

            var hoursInput = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignTextBox"),
                Margin = new Thickness(0, 0, 0, 10)
            };
            MaterialDesignThemes.Wpf.HintAssist.SetHint(hoursInput, "Количество часов");
            hoursPanel.Children.Add(hoursInput);

            var dateTimePanel = new StackPanel
            {
                Margin = new Thickness(20, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var datePicker = new DatePicker
            {
                Style = (Style)FindResource("MaterialDesignDatePicker"),
                SelectedDate = work.EndDate.Date,
                Margin = new Thickness(0, 0, 0, 10)
            };
            MaterialDesignThemes.Wpf.HintAssist.SetHint(datePicker, "Дата окончания");

            var timePicker = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignTextBox"),
                Text = work.EndDate.ToString("HH:mm"),
                Margin = new Thickness(0, 0, 0, 10)
            };
            MaterialDesignThemes.Wpf.HintAssist.SetHint(timePicker, "Время окончания (ЧЧ:мм)");

            dateTimePanel.Children.Add(datePicker);
            dateTimePanel.Children.Add(timePicker);

            hoursRadio.Checked += (s, e) =>
            {
                hoursPanel.Visibility = Visibility.Visible;
                dateTimePanel.Visibility = Visibility.Collapsed;
            };

            dateRadio.Checked += (s, e) =>
            {
                hoursPanel.Visibility = Visibility.Collapsed;
                dateTimePanel.Visibility = Visibility.Visible;
            };

            var extendButton = new Button
            {
                Content = "Продлить",
                Style = (Style)FindResource("MaterialDesignRaisedButton"),
                Margin = new Thickness(0, 20, 0, 0)
            };

            extendButton.Click += (s, e) =>
            {
                try
                {
                    DateTime newEndDate;

                    if (hoursRadio.IsChecked == true)
                    {
                        if (!int.TryParse(hoursInput.Text, out int hours))
                        {
                            MessageBox.Show("Введите корректное количество часов",
                                          "Ошибка",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                            return;
                        }
                        newEndDate = work.EndDate.AddHours(hours);
                    }
                    else
                    {
                        if (datePicker.SelectedDate == null ||
                            !TimeSpan.TryParse(timePicker.Text, out TimeSpan time))
                        {
                            MessageBox.Show("Введите корректную дату и время",
                                          "Ошибка",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                            return;
                        }
                        newEndDate = datePicker.SelectedDate.Value.Add(time);
                    }

                    if (newEndDate <= work.EndDate)
                    {
                        MessageBox.Show("Новое время окончания должно быть позже текущего",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                        return;
                    }

                    _dbManager.ExtendWork(work.Id, newEndDate);
                    MessageBox.Show("Работа успешно продлена",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    extendWindow.DialogResult = true;
                    extendWindow.Close();
                    RefreshWorkDetails(work.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при продлении работы: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            };

            mainStackPanel.Children.Add(radioGroup);
            mainStackPanel.Children.Add(hoursPanel);
            mainStackPanel.Children.Add(dateTimePanel);
            mainStackPanel.Children.Add(extendButton);

            extendWindow.Content = new ScrollViewer
            {
                Content = mainStackPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            extendWindow.ShowDialog();
        }

        private void CompleteWork(MainWindow.Work work)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите завершить работу \"{work.WorkName}\"?",
                                         "Подтверждение завершения",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _dbManager.CompleteWork(work.Id);

                var duration = work.EndDate - work.StartDate;
                var totalMinutes = (int)duration.TotalMinutes;
                var hours = totalMinutes / 60;
                var minutes = totalMinutes % 60;

                MessageBox.Show($"Работа \"{work.WorkName}\" завершена.\n" +
                                $"Длительность: {hours} ч {minutes} мин ({totalMinutes} мин).",
                                "Работа завершена",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                if (_detailsWindow != null && _detailsWindow.IsVisible)
                {
                    _detailsWindow.Close();
                }

                if (ServicesList.SelectedItem is string selectedService)
                {
                    LoadWorksByService(selectedService);
                }
            }
        }

        private void RefreshWorkDetails(int workId)
        {
            var updatedWork = _dbManager.GetWorkById(workId);
            if (updatedWork != null)
            {
                ShowWorkDetails(updatedWork);
            }
            else
            {
                MessageBox.Show("Работа не найдена или была удалена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowWorkLogs(int workId)
        {
            var logs = _dbManager.GetWorkLogs(workId);
            var logsWindow = new Window
            {
                Title = "Логи работы",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            var logsStack = new StackPanel { Margin = new Thickness(10) };

            foreach (var log in logs)
            {
                var logEntry = new TextBlock
                {
                    Text = $"[{log.ActionTime:dd.MM.yyyy HH:mm:ss}] {log.UserId}\n{log.Action}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                logsStack.Children.Add(logEntry);
            }

            var scrollViewer = new ScrollViewer
            {
                Content = logsStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            logsWindow.Content = scrollViewer;
            logsWindow.ShowDialog();
        }
    }
}