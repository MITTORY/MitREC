using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MitREC
{
    public partial class AboutOwner : Window
    {
        public AboutOwner()
        {
            InitializeComponent();
        }

        private void EmailButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                Clipboard.SetText(textBlock.Text);
                MessageBox.Show("Скопировано!", "", MessageBoxButton.OK);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.tinkoff.ru/cf/9bWaXYq2j0n"; // Замените на нужный вам URL
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии ссылки: {ex.Message}");
            }
        }

    }
}
