using System.Windows;
using System.Windows.Input;

namespace MitREC
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            PlayTextBox.Text = Properties.Settings.Default.RecordingHotkey;
            StopTextBox.Text = Properties.Settings.Default.StopHotkey;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение новой горячей клавиши из TextBox в настройки
            string PlayHotkeyText = PlayTextBox.Text;
            Properties.Settings.Default.RecordingHotkey = PlayHotkeyText;
            Properties.Settings.Default.Save();

            string StopHotkeyText = StopTextBox.Text;
            Properties.Settings.Default.StopHotkey = StopHotkeyText;
            Properties.Settings.Default.Save();

            MessageBox.Show("Настройки сохранены.");
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PlayTextBlock_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Получите нажатую клавишу
            Key pressedKey = e.Key;

            // Преобразуйте клавишу в строку и установите ее в TextBlock
            PlayTextBox.Text = pressedKey.ToString();

            // Остановите дальнейшую обработку события, чтобы избежать ввода символа в TextBlock
            e.Handled = true;
        }

        private void StopTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Получите нажатую клавишу
            Key pressedKey = e.Key;

            // Преобразуйте клавишу в строку и установите ее в TextBlock
            StopTextBox.Text = pressedKey.ToString();

            // Остановите дальнейшую обработку события, чтобы избежать ввода символа в TextBlock
            e.Handled = true;
        }
    }
}
