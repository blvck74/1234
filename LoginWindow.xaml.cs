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
    public partial class LoginWindow : Window
    {
        private DatabaseManager _dbManager = new DatabaseManager();

        public LoginWindow()
        {
            InitializeComponent();

            // Загружаем последний введенный никнейм
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastNickname))
            {
                NicknameTextBox.Text = Properties.Settings.Default.LastNickname;
                NicknameTextBox.Foreground = Brushes.Black;
            }
        }

        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            string nickname = NicknameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите корректный никнейм и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверяем, существует ли пользователь
            if (_dbManager.AuthenticateUser(nickname, password))
            {
                // Сохраняем последний введенный никнейм
                Properties.Settings.Default.LastNickname = nickname;
                Properties.Settings.Default.Save();

                // Открываем главное окно
                var dashboard = new MainWindow();
                dashboard.Show();
                this.Close();
            }
            else
            {
                // Если пользователь не найден, создаем нового
                if (_dbManager.CreateUser(nickname, password))
                {
                    MessageBox.Show("Пользователь создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Сохраняем последний введенный никнейм
                    Properties.Settings.Default.LastNickname = nickname;
                    Properties.Settings.Default.Save();

                    // Открываем главное окно
                    var dashboard = new MainWindow();
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = Brushes.Gray;
            }
        }
    }
}