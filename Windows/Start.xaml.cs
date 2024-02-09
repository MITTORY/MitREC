using Hardcodet.Wpf.TaskbarNotification;
using MitREC.Class;
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
    public partial class Start : Window
    {
        public Start()
        {
            InitializeComponent();
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
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            StartReg startReg = new StartReg();
            startReg.Show();
            Close();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginBox.Text.Trim();
            string pass = passBox.Password.Trim();

            try
            {
                if (login.Length < 3)
                {
                    loginBox.ToolTip = "Недопустимое количество символов!";
                }
                else if (pass.Length < 8)
                {
                    passBox.ToolTip = "Пароль должен быть не меньше 8 символов!";
                    MessageBox.Show("Пароль меньше 8 символов!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    loginBox.ToolTip = " ";
                    passBox.ToolTip = " ";

                    User authUser = null;
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        authUser = db.Users.Where(b => b.Login == login && b.Pass == pass).FirstOrDefault();
                    }

                    if (authUser != null)
                    {
                        //// Генерация токена
                        //string token = TokenService.GenerateToken(authUser.id.ToString(), authUser.Login);
                        //ConfigurationService.SaveTokenToConfiguration(token);

                        //// Сохранение токена в безопасном месте, например, в свойстве пользователя
                        //authUser.Token = token;

                        // Открытие главного окна
                        MainWindow mainWindow = new MainWindow();
                        string welcomeMessage = $"{authUser.Login}";
                        mainWindow.welcomeLabel.Content = welcomeMessage;

                        //// Передача токена в главное окно
                        //mainWindow.Token = token;

                        mainWindow.Show();
                        Close();
                    }
                    else if (login == "admin" && pass == "admin123")
                    {
                        adminPanel admin = new adminPanel();
                        admin.Show();
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Логин и/или пароль неверен!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка!" + ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void rePass_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция пока недоступна!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
