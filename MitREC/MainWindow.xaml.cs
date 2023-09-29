using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NAudio.Wave;
using Timer = System.Windows.Threading.DispatcherTimer;
using Accord.Video;
using Accord.Video.DirectShow;
using Accord.Video.FFMPEG;
using System.Windows.Controls;
using System.Drawing;

namespace MitREC
{
    public partial class MainWindow : Window
    {
        int bitrate = 0;
        private string savePath;
        private VideoCaptureDevice videoSource;
        private ScreenCaptureStream screenCapture;
        private VideoFileWriter videoWriter;
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private Timer timer;
        private TimeSpan recordingTime;
        private bool isRecording;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            if (waveIn == null)
            {
                waveIn = new WaveInEvent();
                waveIn.DataAvailable += WaveIn_DataAvailable;

                savePath = SaveRoad.Text;
                if (!string.IsNullOrEmpty(savePath))
                {
                    if (audioBox.SelectedItem != null)
                    {
                        string selectedQuality = (audioBox.SelectedItem as ComboBoxItem).Content.ToString();
                        switch (selectedQuality)
                        {
                            case ".wav":
                                // Создаем файл .wav для записи аудио
                                string fileNameAudioWav = "audio_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav";
                                writer = new WaveFileWriter(Path.Combine(savePath, fileNameAudioWav), waveIn.WaveFormat);
                                break;
                            case ".mp3":
                                // Создаем файл .mp3 для записи аудио
                                string fileNameAudioMp3 = "audio_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp3";
                                writer = new WaveFileWriter(Path.Combine(savePath, fileNameAudioMp3), waveIn.WaveFormat);
                                break;
                        }
                    }

                    TimeLable.Visibility = Visibility.Visible;
                    durationTextBlock.Visibility = Visibility.Visible;

                    // Начать запись
                    waveIn.StartRecording();

                    // Начать отслеживание времени записи
                    recordingTime = TimeSpan.Zero;
                    timer = new Timer();
                    timer.Interval = TimeSpan.FromSeconds(1);
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
                            case "360p":
                                bitrate = 5000000;
                                break;
                            case "720p":
                                bitrate = 10000000;
                                break;
                            case "1080p":
                                bitrate = 20000000;
                                break;
                        }
                    }

                    // Запускаем захват рабочего стола
                    screenCapture = new ScreenCaptureStream(Screen.PrimaryScreen.Bounds, fps);
                    screenCapture.NewFrame += ScreenCapture_NewFrame;
                    screenCapture.Start();

                    // Начинаем запись видео
                    videoWriter = new VideoFileWriter();
                    string fileName = "video_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
                    videoWriter.Open(Path.Combine(savePath, fileName), width, height, fps, VideoCodec.MPEG4, bitrate);

                    isRecording = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("Выберите путь для сохранения!");
                }
            }
        }

        private void ScreenCapture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (isRecording && videoWriter != null && videoWriter.IsOpen)
            {
                Bitmap frame = eventArgs.Frame;
                videoWriter.WriteVideoFrame(frame);
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            recordingTime = recordingTime.Add(TimeSpan.FromSeconds(1));
            durationTextBlock.Text = recordingTime.ToString(@"mm\:ss");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (waveIn != null)
            {
                // Остановить запись и очистить ресурсы
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
            System.Windows.MessageBox.Show("Приложение для записи/захвата экрана было создано с целью импортозамещения для дипломной работы.\nПриложение с понятным интерфейсом и простой настройкой.\nВы можете просто запустить программу и за пару кликов уже начать вести запись!\nНадеюсь, что вам помогла эта программа!", "О приложении",MessageBoxButton.OK);
        }

        private void DeveloperButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Здравствуй, меня зовут Айдар.\nЯ студент 4 курса с направления Информационных Систем и Технологий.\n\nЕсли ты нашёл какие-то проблемы в программе, то можешь написать мне на почту: novo1233@mail.ru\n\nСпасибо, что пользуешься данным продуктом!","О разработчике", MessageBoxButton.OK);
        }
    }
}