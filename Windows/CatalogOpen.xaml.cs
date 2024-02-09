using System;
using System.IO;
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
using System.Collections.Generic;
using System.Windows.Forms;

namespace MitREC
{
    public partial class CatalogOpen : Window
    {
        private string selectedPath = null;
        private string currentPath = null;
        private Stack<string> pathHistory = new Stack<string>();

        public CatalogOpen()
        {
            InitializeComponent();

            // Загрузите последние выбранный и открытый пути из настроек
            string lastSelectedPath = Properties.Settings.Default.LastSelectedPath;
            string lastOpenedPath = Properties.Settings.Default.LastOpenedPath;

            if (!string.IsNullOrEmpty(lastSelectedPath))
            {
                // Восстановите последний выбранный путь в TextBox
                WinSaveRoad.Text = lastSelectedPath;
            }

            if (!string.IsNullOrEmpty(lastOpenedPath) && Directory.Exists(lastOpenedPath))
            {
                // Восстановите последний открытый путь в ListView и текстовом поле
                UpdateListView(lastOpenedPath);
                WinSaveRoad.Text = lastOpenedPath;
            }

            // Заполнение списка дисков
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem driveItem = new TreeViewItem();
                driveItem.Header = drive.Name;
                driveItem.Tag = drive.Name; // Сохраняем путь к диску в Tag
                DiskTreeView.Items.Add(driveItem);
            }

            // Устанавливаем обработчик события выбора элемента в DiskTreeView
            DiskTreeView.SelectedItemChanged += DiskTreeView_SelectedItemChanged;
        }

        private void DriveItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem driveItem = (TreeViewItem)sender;
            if (driveItem.Items.Count == 1 && driveItem.Items[0] == null)
            {
                driveItem.Items.Clear(); // Убираем заглушку

                string drivePath = (string)driveItem.Tag; // Получаем путь к диску
                try
                {
                    foreach (string directory in Directory.GetDirectories(drivePath))
                    {
                        TreeViewItem directoryItem = new TreeViewItem();
                        directoryItem.Header = System.IO.Path.GetFileName(directory);
                        directoryItem.Tag = directory; // Сохраняем путь к каталогу в Tag
                        directoryItem.Items.Add(null); // Добавляем заглушку
                        directoryItem.Expanded += DriveItem_Expanded; // Обработчик разворачивания
                        driveItem.Items.Add(directoryItem);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Обработка ошибки доступа
                    System.Windows.MessageBox.Show("Ошибка доступа к директориям на диске: " + drivePath);
                }
            }
        }

        private void UpdateListView(string path)
        {
            try
            {
                ListView.Items.Clear();
                foreach (string directory in Directory.GetDirectories(path))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem();
                    item.Content = dirInfo.Name;
                    item.Tag = dirInfo.FullName;
                    item.MouseDoubleClick += ListViewItem_MouseDoubleClick;
                    ListView.Items.Add(item);
                }
                currentPath = path;
                pathHistory.Push(currentPath);

                // Сохраните текущий путь в настройках
                Properties.Settings.Default.LastOpenedPath = currentPath;
                Properties.Settings.Default.Save();
            }
            catch (UnauthorizedAccessException)
            {
                System.Windows.MessageBox.Show("Ошибка доступа к директориям на диске: " + path);
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListViewItem selectedItem = (System.Windows.Controls.ListViewItem)sender;
            string selectedPath = (string)selectedItem.Tag;
            try
            {
                if (Directory.Exists(selectedPath))
                {
                    WinSaveRoad.Text = selectedPath; // Устанавливаем выбранный путь в TextBox
                    UpdateListView(selectedPath); // Обновляем ListView
                }
            }
            catch (UnauthorizedAccessException)
            {
                System.Windows.MessageBox.Show("Ошибка доступа к директории: " + selectedPath);
            }
        }

        private void DiskTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = (TreeViewItem)DiskTreeView.SelectedItem;
            if (selectedItem != null)
            {
                string selectedPath = (string)selectedItem.Tag;
                try
                {
                    WinSaveRoad.Text = selectedPath; // Устанавливаем выбранный путь в TextBox
                    UpdateListView(selectedPath); // Обновляем ListView

                    // Сохраните выбранный путь в настройках
                    Properties.Settings.Default.LastSelectedPath = selectedPath;
                    Properties.Settings.Default.Save();
                }
                catch (UnauthorizedAccessException)
                {
                    System.Windows.MessageBox.Show("Ошибка доступа к диску: " + selectedPath);
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            if (!string.IsNullOrEmpty(selectedPath))
            {
                mainWindow.SaveRoad.Text = selectedPath;
            }
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathHistory.Count > 1)
            {
                pathHistory.Pop(); // Удаляем текущий путь, так как мы уже находимся в этой папке
                string previousPath = pathHistory.Pop(); // Получаем предыдущий путь
                UpdateListView(previousPath);
                WinSaveRoad.Text = previousPath; // Обновляем текст в WinSaveRoad
            }
            else if (pathHistory.Count == 1)
            {
                // Если остался только один путь в истории (корень), то перейдите в корень
                string rootPath = pathHistory.Pop();
                UpdateListView(rootPath);
                WinSaveRoad.Text = rootPath; // Обновляем текст в WinSaveRoad
            }
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = WinSaveRoad.Text; // Получаем текущий путь

            if (!string.IsNullOrEmpty(currentPath) && Directory.Exists(currentPath))
            {
                // Открываем окно FolderName для ввода имени папки
                FolderName folderNameWindow = new FolderName();
                if (folderNameWindow.ShowDialog() == true)
                {
                    string newFolderName = folderNameWindow.FolderNameText;

                    if (!string.IsNullOrEmpty(newFolderName))
                    {
                        string newFolderPath = System.IO.Path.Combine(currentPath, newFolderName);

                        try
                        {
                            // Проверяем, существует ли уже папка с таким именем
                            if (!Directory.Exists(newFolderPath))
                            {
                                // Создаем новую папку
                                Directory.CreateDirectory(newFolderPath);

                                // Обновляем ListView и WinSaveRoad
                                UpdateListView(currentPath);
                                WinSaveRoad.Text = currentPath;

                                System.Windows.MessageBox.Show($"Папка '{newFolderName}' успешно создана в '{currentPath}'", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Папка с таким именем уже существует в выбранной директории.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Ошибка при создании папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите директорию, в которой хотите создать папку.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}