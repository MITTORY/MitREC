using System;
using System.Windows.Input;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using NAudio.Wave;
using System.Windows.Controls;
using Accord.Video;
using Accord.Video.FFMPEG;
using System.Drawing;
using Timer = System.Windows.Threading.DispatcherTimer;
using Hardcodet.Wpf.TaskbarNotification;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace MitREC
{
    public partial class MainWindow : Window
    {
        public TextBlock DurationTextBlock { get { return durationTextBlock; } }
        public ICommand ShowMainWindowCommand { get; set; }
        public string Token { get; set; }
        int bitrate = 20000000;
        string videoExtension = null;
        private string savePath;
        private readonly TaskbarIcon taskbarIcon;
        private DateTime lastFrameTime;
        private ScreenCaptureStream screenCapture;
        private string watermarkImagePath;
        private VideoFileWriter videoWriter;
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private Timer timer;
        private TimeSpan recordingTime;
        private bool isRecording;
        private KeyGesture recordingHotkey;
        private KeyGesture stopHotkey;

        public MainWindow()
        {
            InitializeComponent();
            SetRecordingHotkey();
            SetStopHotkey();

            taskbarIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
            taskbarIcon.TrayMouseDoubleClick += (s, e) => ShowMainWindow();

            //Список для хранения информации о дисплеях
            List<DisplayInfo> displayInfoList = new List<DisplayInfo>();

            //Получение списка доступных дисплеев
            foreach (Screen screen in Screen.AllScreens)
            {
                displayInfoList.Add(new DisplayInfo
                {
                    DeviceName = screen.DeviceName,
                    DisplayName = "Дисплей " + (displayInfoList.Count + 1)
                });
            }

            //Источник данных для ComboBox
            DisplayBox.ItemsSource = displayInfoList;
            DisplayBox.DisplayMemberPath = "DisplayName";
            DisplayBox.SelectedValuePath = "DeviceName";
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DisplayBox.Items.Count > 0)
            {
                DisplayBox.SelectedIndex = 0; 
            }
        }
        
        // Основное
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
        private void ShowMainWindow()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        // Горячие клавиши
        private void SetRecordingHotkey()
        {
            // Загрузка горячей клавиши из настроек
            string hotkeyText = Properties.Settings.Default.RecordingHotkey;

            // Парсинг горячей клавиши
            KeyGestureConverter converter = new KeyGestureConverter();
            recordingHotkey = converter.ConvertFromString(hotkeyText) as KeyGesture;

            if (recordingHotkey == null)
            {
                // Если горячая клавиша не удалось загрузить из настроек, используйте значение по умолчанию
                recordingHotkey = new KeyGesture(Key.F9);
            }

            // Зарегистрируем обработчик горячей клавиши
            CommandBinding binding = new CommandBinding(ApplicationCommands.New);
            binding.Executed += RecordingHotkey_Executed;
            CommandBindings.Add(binding);

            // Привязываем горячую клавишу к окну
            InputBinding inputBinding = new InputBinding(ApplicationCommands.New, recordingHotkey);
            InputBindings.Add(inputBinding);
        }
        private void SetStopHotkey()
        {
            string hotkeyText = Properties.Settings.Default.StopHotkey;

            stopHotkey = new KeyGesture(Key.F12);

            CommandBinding binding = new CommandBinding(ApplicationCommands.Close);
            binding.Executed += StopHotkey_Executed;
            CommandBindings.Add(binding);

            InputBinding inputBinding = new InputBinding(ApplicationCommands.Close, stopHotkey);
            InputBindings.Add(inputBinding);
        }
        private void RecordingHotkey_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Обработка нажатия горячей клавиши для начала записи
            RecButton_Click(sender, e);
        }
        private void StopHotkey_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Обработка нажатия горячей клавиши для кнопки "Стоп"
            StopButton_Click(sender, e);
        }

        // Запись видео и водяной знак
        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            RecButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            if (waveIn == null)
            {
                waveIn = new WaveInEvent();
                waveIn.DataAvailable += WaveIn_DataAvailable;

                // Получить выбранный дисплей из ComboBox
                string selectedDeviceName = (DisplayBox.SelectedValue as string);

                // Проверка на то, что пользователь выбрал дисплей
                if (string.IsNullOrEmpty(selectedDeviceName))
                {
                    System.Windows.MessageBox.Show("Выберите дисплей для записи.");
                    return;
                }

                savePath = SaveRoad.Text;
                if (!string.IsNullOrEmpty(savePath))
                {
                    if (audioBox.SelectedItem != null)
                    {
                        string selectedAudio = (audioBox.SelectedItem as ComboBoxItem).Content.ToString();
                        switch (selectedAudio)
                        {
                            case ".wav":
                                string fileNameAudioWav = "audio_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + selectedAudio;
                                writer = new WaveFileWriter(Path.Combine(savePath, fileNameAudioWav), waveIn.WaveFormat);
                                break;
                            case ".mp3":
                                string fileNameAudioMp3 = "audio_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + selectedAudio;
                                writer = new WaveFileWriter(Path.Combine(savePath, fileNameAudioMp3), waveIn.WaveFormat);
                                break;
                        }
                    }

                    // Начать запись
                    waveIn.StartRecording();

                    // Начать отслеживание времени записи
                    recordingTime = TimeSpan.Zero;
                    timer = new Timer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += Timer_Tick;
                    timer.Start();

                    int width = 1920; // Ширина видео
                    int height = 1080; // Высота видео
                    int fps = 30; // Кадры в секунду (FPS)
                    if (videoQualityBox.SelectedItem != null)
                    {
                        string selectedQuality = (videoQualityBox.SelectedItem as ComboBoxItem).Content.ToString();
                        switch (selectedQuality)
                        {
                            case "Низкое":
                                bitrate = 2000000;
                                break;
                            case "Среднее":
                                bitrate = 8000000;
                                break;
                            case "Высокое":
                                bitrate = 20000000;
                                break;
                        }
                    }

                    // Определение экрана для записи на основе выбранного имени
                    Screen selectedScreen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == selectedDeviceName);

                    if (selectedScreen != null)
                    {
                        // Запускаем захват рабочего стола выбранного дисплея
                        screenCapture = new ScreenCaptureStream(selectedScreen.Bounds, fps);
                        screenCapture.NewFrame += ScreenCapture_NewFrame;
                        screenCapture.Start();

                        // Начинаем запись видео
                        videoWriter = new VideoFileWriter();
                        if (videoBox.SelectedItem != null)
                        {
                            string selectedExtension = (videoBox.SelectedItem as ComboBoxItem).Content.ToString();
                            switch (selectedExtension)
                            {
                                case ".mp4":
                                    videoExtension = ".mp4";
                                    break;
                                case ".avi":
                                    videoExtension = ".avi";
                                    break;
                            }
                            string fileName = "video_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + videoExtension;
                            videoWriter.Open(Path.Combine(savePath, fileName), width, height, fps, VideoCodec.MPEG4, bitrate);
                        }
                        isRecording = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Выбранный дисплей не найден.");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Выберите путь для сохранения!");
                }
            }
        }
        private void ApplyWatermark(Bitmap frame)
        {
            if (!string.IsNullOrEmpty(watermarkImagePath))
            {
                // Загружаем изображение водяного знака
                Bitmap watermark = new Bitmap(watermarkImagePath);

                watermark = new Bitmap(watermark, new System.Drawing.Size(75, 75));

                // Размещаем водяной знак на видеокадре
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.DrawImage(watermark, new System.Drawing.Point(frame.Width - watermark.Width - 10, frame.Height - watermark.Height - 10));
                }
            }
        }
        private void ScreenCapture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (IsRecordingValid())
            {
                Bitmap frame = eventArgs.Frame;

                ApplyWatermark(frame);

                videoWriter.WriteVideoFrame(frame);

                lastFrameTime = DateTime.Now;
            }
        }
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer?.Write(e.Buffer, 0, e.BytesRecorded);
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            RecButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;

                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                    writer = null;
                }

                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick;
                    timer = null;
                }

                if (isRecording)
                {
                    isRecording = false;
                    screenCapture.SignalToStop();
                    screenCapture.WaitForStop();
                    videoWriter.Close();
                    videoWriter.Dispose();
                }
            }
        }
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                watermarkImagePath = openFileDialog.FileName;

                BitmapImage bitmapImage = new BitmapImage(new Uri(watermarkImagePath));
                WImage.Source = bitmapImage;
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            WImage.Source = null;
            watermarkImagePath = null;
        }

        // Таймер
        private void Timer_Tick(object sender, EventArgs e)
        {
            recordingTime = recordingTime.Add(TimeSpan.FromSeconds(1));
            durationTextBlock.Text = recordingTime.ToString(@"mm\:ss");
        }

        // Действия с окном
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            WindowState = WindowState.Minimized;

            var taskbarIcon = FindResource("MyNotifyIcon") as TaskbarIcon;

            taskbarIcon.Visibility = Visibility.Visible;
        }

        // Кнопки в трей
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        }
        private void HotKeysMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        // Кнопки для открытия доп. окон на основном окне
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            CatalogOpen catalogWindow = new CatalogOpen();

            bool? result = catalogWindow.ShowDialog();

            if (result == true)
            {
                string selectedPath = catalogWindow.WinSaveRoad.Text;
                SaveRoad.Text = selectedPath;
            }

            SaveRoad.IsEnabled = true;
        }
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            AboutApp aboutApp = new AboutApp();
            aboutApp.ShowDialog();
        }
        private void DeveloperButton_Click(object sender, RoutedEventArgs e)
        {
            AboutOwner aboutOwner = new AboutOwner();
            aboutOwner.ShowDialog();
        }
        private void InstructionsButton_Click(object sender, RoutedEventArgs e)
        {
            InstructionWindow instructionsWindow = new InstructionWindow();
            instructionsWindow.ShowDialog();
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

        // Параметр для смены дисплея
        private void DisplayBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int fps = 30;

            if (isRecording)
            {
                screenCapture.SignalToStop();
                screenCapture.WaitForStop();

                string newDeviceName = (DisplayBox.SelectedValue as string);
                Screen newScreen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == newDeviceName);

                if (newScreen != null)
                {
                    screenCapture = new ScreenCaptureStream(newScreen.Bounds, fps);
                    screenCapture.NewFrame += ScreenCapture_NewFrame;
                    screenCapture.Start();
                }
                else
                {
                    System.Windows.MessageBox.Show("Выбранный дисплей не найден.");
                }
            }
        }

        // Другое
        private bool IsRecordingValid()
        {
            return isRecording && videoWriter != null && videoWriter.IsOpen;
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Start start = new Windows.Start();
            start.Show();
            Close();
        }
    }
}