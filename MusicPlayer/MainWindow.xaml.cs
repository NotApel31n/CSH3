using System;
using System.Collections.Generic;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using static System.Net.WebRequestMethods;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Playlist playlist = new Playlist();
        private bool stopUpdating = false;
        private string folder = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void randomButton_Click(object sender, EventArgs e)
        {
            ToggleRandomMode();
        }

        private void ToggleRandomMode()
        {
            playlist.IsRandom = !playlist.IsRandom;
            randomButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/csharpicons/random_{playlist.IsRandom}.png"));

            UpdatePlaylist(playlist.IsRandom);
            if (MusicList.Items.Count > 0)
            {
                MusicList.SelectedIndex = 0;
            }
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if (MusicList.Items.Count == 0)
            {
                return;
            }
            if (MusicList.SelectedIndex == 0)
            {
                MusicList.SelectedIndex = MusicList.Items.Count - 1;
            }
            else
            {
                MusicList.SelectedIndex -= 1;
            }
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            TogglePlayback();
        }

        private void TogglePlayback()
        {
            if (playlist.IsPaused)
            {
                media.Play();
            }
            else
            {
                media.Pause();
            }
            playlist.IsPaused = !playlist.IsPaused;
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (MusicList.Items.Count == 0)
            {
                return;
            }
            if (MusicList.SelectedIndex == MusicList.Items.Count - 1)
            {
                MusicList.SelectedIndex = 0;
            }
            else
            {
                MusicList.SelectedIndex += 1;
            }
        }

        private void repeatButton_Click(object sender, EventArgs e)
        {
            ToggleRepeatMode();
        }

        private void ToggleRepeatMode()
        {
            playlist.IsRandom = !playlist.IsRandom;
            repeatButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/csharpicons/repeat_{playlist.IsRepeat}.png"));
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            trackSlider.Maximum = media.NaturalDuration.TimeSpan.Ticks;
            musicImage.Source = playlist.Tracks[MusicList.SelectedIndex].Image.Source;

            Thread gg = new Thread(delegate () { UpdateSlider(); });
            stopUpdating = false;
            gg.Start();
        }

        private void UpdateSlider()
        {
            while (true)
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        int playerIndex = MusicList.SelectedIndex;
                        if (playerIndex != MusicList.SelectedIndex)
                        {
                            return;
                        }
                        if (media.NaturalDuration.HasTimeSpan && media.Position.Ticks == media.NaturalDuration.TimeSpan.Ticks)
                        {
                            if (!playlist.IsRepeat)
                            {
                                if (MusicList.SelectedIndex == MusicList.Items.Count - 1)
                                {
                                    MusicList.SelectedIndex = 0;
                                }
                                else
                                {
                                    MusicList.SelectedIndex += 1;
                                }
                            }
                            else
                            {
                                playMusic();
                            }
                        }
                        trackPosition.Text = media.Position.ToString();
                        try
                        {
                            trackMaximum.Text = media.NaturalDuration.TimeSpan.ToString();
                        }
                        catch
                        {
                            trackMaximum.Text = new TimeSpan().ToString();
                        }
                        trackSlider.Value = media.Position.Ticks;
                    });
                }
                catch (TaskCanceledException e)
                {
                    Environment.Exit(0);
                }
                catch
                {
                    return;
                }
                Thread.Sleep(100);
            }
        }

        private void playMusic()
        {
            media.Source = new Uri(playlist.Tracks[MusicList.SelectedIndex].Path);
            media.Play();
            media.Volume = 1;
        }

        private void MusicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicList.SelectedIndex == -1) return;
            playMusic();
        }

        private void trackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Position = new TimeSpan(Convert.ToInt64(trackSlider.Value));
        }

        private void UpdatePlaylist(bool randomize = false)
        {
            MusicList.Items.Clear();
            playlist.Tracks.Clear();
            List<string> files = new List<string>();
            foreach (string fileName in Directory.GetFiles(folder))
            {
                if (fileName.EndsWith("mp3"))
                {
                    files.Add(fileName);
                }
            }
            if (randomize)
            {
                Random rnd = new Random();
                files = files.OrderBy(x => rnd.Next()).ToList();
            }
            playlist.LoadTracks(files);
            foreach (Track track in playlist.Tracks)
            {
                MusicList.Items.Add(track.Name);
            }
            media.Stop();
        }

        private void UploadMusic(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog { IsFolderPicker = true };
            CommonFileDialogResult opened = dlg.ShowDialog();
            if (opened != CommonFileDialogResult.Ok)
            {
                return;
            }
            List<string> files = new List<string>();
            folder = dlg.FileName;
            UpdatePlaylist();
            if (MusicList.Items.Count > 0)
            {
                MusicList.SelectedIndex = 0;
            }
        }
    }
}
