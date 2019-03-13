using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;
namespace vlc
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragged = false;
        public ObservableCollection<Video> VideoQueue = new ObservableCollection<Video>();
        public int currentIndex = 0;
        private bool Started = false;
        public FileStream CurrentFileStream;
        public bool Repeat = false;
        public MainWindow()
        {
            InitializeComponent();
            VideoQueueList.ItemsSource = VideoQueue;
            VideoQueueList.SelectionChanged += VideoQueueList_SelectionChanged;
        }

        private void VideoQueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(VideoQueueList.SelectedIndex);
            if (VideoQueueList.SelectedIndex != currentIndex && VideoQueueList.SelectedIndex != -1)
            {
                currentIndex = VideoQueueList.SelectedIndex;
                PlayNewVideo();
            }
            
        }

        public void StartPlaying()
        {
            var vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };
            MyControl.MediaPlayer.VlcLibDirectory = vlcLibDirectory;
            MyControl.MediaPlayer.EndInit();
            // Load libvlc libraries and initializes stuff. It is important that the options (if you want to pass any) and lib directory are given before calling this method.
            if (VideoQueue[currentIndex].isURL)
            {
                MyControl.MediaPlayer.Play(VideoQueue[currentIndex].path);
            }
            else
            {
                if (CurrentFileStream != null) CurrentFileStream.Close();
                CurrentFileStream = new FileStream(VideoQueue[currentIndex].path, FileMode.Open);
                MyControl.MediaPlayer.Play(CurrentFileStream);
            }
            MyControl.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MyControl.MediaPlayer.EndReached += MediaPlayer_EndReached;
        }
        private void MediaPlayer_EndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            ChangeVideo(true);
        }
        private void ChangeVideo(bool next)
        {
            if (!Repeat)
            {
                if (next)
                {
                    if (currentIndex < VideoQueue.Count - 1)
                    {
                        currentIndex++;
                    }
                    else
                    {
                        currentIndex = 0;
                    }
                }
                else if (currentIndex > 0)
                {
                    currentIndex--;
                }
                else
                {
                    currentIndex = VideoQueue.Count - 1;
                }
            }
            PlayNewVideo();
        }
        public void PlayNewVideo()
        {
            if(VideoQueue.Count > 0)
            {
                if (VideoQueue[currentIndex].isURL)
                {
                    ThreadPool.QueueUserWorkItem((f) => MyControl.MediaPlayer.Play(VideoQueue[currentIndex].path));
                }
                else
                {
                    if (CurrentFileStream != null) CurrentFileStream.Close();
                    CurrentFileStream = new FileStream(VideoQueue[currentIndex].path, FileMode.Open);
                    ThreadPool.QueueUserWorkItem((f) => MyControl.MediaPlayer.Play(CurrentFileStream));
                    //MyControl.MediaPlayer.Audio.
                }
            }
            else
            {
                //MyControl.MediaPlayer.;
            }
          
        }
        private void MediaPlayer_TimeChanged(object sender, VlcMediaPlayerTimeChangedEventArgs e)
        {
            if (!isDragged)
            {
                long VideoEndTime = MyControl.MediaPlayer.VlcMediaPlayer.Length;
                long EndMinutes = VideoEndTime / 1000 / 60;
                long EndSeconds = VideoEndTime / 1000 % 60;

                long VideoCurrentTime = MyControl.MediaPlayer.VlcMediaPlayer.Time;
                long CurrentMinutes = VideoCurrentTime / 1000 / 60;
                long CurrentSeconds = VideoCurrentTime / 1000 % 60;
                this.Dispatcher.Invoke(() =>
                {
                    TimeSlider.Maximum = VideoEndTime;
                    TimeSlider.Minimum = 0;
                    CurrentTime.Content = CurrentMinutes > 10 ? CurrentMinutes.ToString() : "0" + CurrentMinutes.ToString() + ":" + (CurrentSeconds > 10 ? CurrentSeconds.ToString() : "0" + CurrentSeconds.ToString());
                    EndTime.Content = EndMinutes > 10 ? EndMinutes.ToString() : "0" + EndMinutes.ToString() + ":" + (EndSeconds > 10 ? EndSeconds.ToString() : "0" + EndSeconds.ToString());
                    TimeSlider.Value = e.NewTime;
                });
            }
          
        }
        public void ChangeTime(long SecondsToAdd)
        {
            long ChangedTime = MyControl.MediaPlayer.Time + (SecondsToAdd * 1000);
            if(ChangedTime < 0)
            {
                ChangedTime = 0;
            }
            if(ChangedTime >= MyControl.MediaPlayer.Length)
            {
                ChangedTime = MyControl.MediaPlayer.Length;
            }
            MyControl.MediaPlayer.Time = ChangedTime;
        }
        private void MediaPlayer_MediaChanged(object sender, VlcMediaPlayerMediaChangedEventArgs e)
        {
            Debug.WriteLine("media chagned");
        }

        public void ToggleVideo()
        {
            if (MyControl.MediaPlayer.IsPlaying)
            {
                MyControl.MediaPlayer.Pause();
            }
            else
            {
                MyControl.MediaPlayer.Play();
            }
           
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleVideo();
        }

        private void TimeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MyControl.MediaPlayer.Time = (long)Math.Round(TimeSlider.Value);
            isDragged = false;
        }

        private void TimeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragged = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChangeVideo(true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ChangeVideo(false);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ChangeTime(30);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ChangeTime(-5);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            VideoQueue.Add(new Video(VideoInput.Text,true));
            if (!Started)
            {
                Started = true;
                StartPlaying();
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                VideoQueue.Add(new Video(@dlg.FileName,false));
           //     Debug.WriteLine(dlg.FileName);
                if (!Started)
                {
                    Started = true;
                    StartPlaying();
                }
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Repeat = !Repeat;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(currentIndex);
            VideoQueue.RemoveAt(currentIndex);
            if (currentIndex > 0)
            {
                //  Debug.WriteLine("NEGR" + currentIndex);
                currentIndex -= 1;
            }
            ChangeVideo(false);
        }
    }
}
