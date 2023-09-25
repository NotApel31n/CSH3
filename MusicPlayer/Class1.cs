using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using TagLib;

namespace MusicPlayer
{
    public class Track
    {
        public string Name { get; }
        public string Path { get; }
        public Image Image { get; }

        public Track(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Image = GetImage();
        }

        private Image GetImage()
        {
            try
            {
                var file = TagLib.File.Create(Path);
                var im = new Image
                {
                    Height = 30,
                    Width = 30,
                    Stretch = Stretch.Fill
                };

                if (file.Tag.Pictures.Length > 0)
                {
                    var pic = file.Tag.Pictures[0];
                    var ms = new MemoryStream(pic.Data.Data);
                    ms.Seek(0, SeekOrigin.Begin);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    im.Source = bitmap;
                }
                else
                {
                    im.Source = new BitmapImage(new Uri($"pack://application:,,,/csharpicons/musicdefault.png"));
                }

                return im;
            }
            catch
            {
                var im = new Image
                {
                    Source = new BitmapImage(new Uri($"pack://application:,,,/csharpicons/musicdefault.png")),
                    Height = 30,
                    Width = 30,
                    Stretch = Stretch.Fill
                };
                return im;
            }
        }
    }

    public class Playlist
    {
        public bool IsPaused { get; set; } = false;
        public bool IsRandom { get; set; } = false;
        public bool IsRepeat { get; set; } = false;
        public List<Track> Tracks { get; } = new List<Track>();

        public void LoadTracks(List<string> tracks = null)
        {
            if (tracks != null)
            {
                foreach (var track in tracks)
                {
                    Tracks.Add(new Track(track));
                }
            }
        }
    }

}
