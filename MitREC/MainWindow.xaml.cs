using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace MitREC
{
    public partial class MainWindow : Window
    {
        private string savePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                savePath = folderDialog.SelectedPath;
                SaveRoad.IsEnabled = true;
                SaveRoad.Text = savePath;
            }
        }

        private void CheckBoxDesktop_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBoxDesktop.IsEnabled)
            {
                ComboBoxWindow.IsEnabled = false;
            }
        }

        private void CheckBoxWindow_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBoxWindow.IsEnabled)
            {
                ComboBoxWindow.IsEnabled = true;
            }
        }

        private void ComboBoxWindow_DropDownOpened(object sender, System.EventArgs e)
        {
            ComboBoxWindow.Items.Clear();
            var processes = Process.GetProcesses();
            for (var i = 1; i < processes.Length; i++)
                ComboBoxWindow.Items.Add($"{processes[i].ProcessName} - {processes[i].Id}");
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Приложение для записи/захвата экрана было создано с целью импортозамещения для дипломной работы.\nПриложение с понятным интерфейсом и простой настройкой.\nВы можете просто запустить программу и за пару кликов уже начать вести запись!\nНадеюсь, что вам помогла эта программа!", "О приложении",MessageBoxButton.OK);
        }

        private void DeveloperButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Здравствуй, меня зовут Айдар.\nЯ студент 4 курса с направления Информационных Систем и Технологий.\n\nЕсли ты нашёл какие-то проблемы в программе, то можешь написать мне на почту: novo1233@mail.ru\n\nСпасибо, что пользуешься данным продуктом!","О разработчике", MessageBoxButton.OK);
        }
    }
}