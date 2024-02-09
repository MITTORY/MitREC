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

namespace MitREC.Windows
{
    public partial class StartReg : Window
    {
        ApplicationContext db;
        Start start = new Start();

        public StartReg()
        {
            InitializeComponent();

            db = new ApplicationContext();
        }

        // Действия с окном
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            WindowState = WindowState.Minimized;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        // Кнопки
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            start.Show();
            Close();
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            string email = mailBox.Text.ToLower().Trim();
            string login = loginBox.Text.Trim();
            string pass1 = passBox1.Password.Trim();
            string pass2 = passBox2.Password.Trim();

            try
            {
                if (!email.Contains("@") || !email.Contains("."))
                {
                    mailBox.ToolTip = "Это поле введено некорректно!";
                }
                else if (login.Length < 3)
                {
                    loginBox.ToolTip = "Недопустимое количество символов!";
                }
                else if (pass1.Length < 8)
                {
                    passBox1.ToolTip = "Пароль должен быть не меньше 8 символов!";
                }
                else if (pass2 != pass1)
                {
                    passBox2.ToolTip = "Пароли не совпадают!";
                }
                else
                {
                    mailBox.ToolTip = " ";
                    loginBox.ToolTip = " ";
                    passBox1.ToolTip = " ";
                    passBox2.ToolTip = " ";

                    User user = new User(email, login, pass1);
                    db.Users.Add(user);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно!\nМожете войти в аккаунт.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                    start.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при регистрации: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
