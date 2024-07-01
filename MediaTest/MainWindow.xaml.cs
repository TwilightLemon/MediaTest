using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Media;
using Windows.Media.Control;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MediaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        SMTCCreator? _smtcCreator = null;
        SMTCListener _smtcListener = null;
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _smtcListener = await SMTCListener.CreateInstance();
            _smtcListener.MediaPropertiesChanged += _smtcListener_MediaPropertiesChanged;
            _smtcListener.PlaybackInfoChanged += _smtcListener_PlaybackInfoChanged;
            _smtcListener.SessionExited += _smtcListener_SessionExited;
        }

        private void _smtcListener_SessionExited(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusTb.Text = "Session Exited";
            });
        }

        private void _smtcListener_PlaybackInfoChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var info = _smtcListener.GetPlaybackStatus();
                if (info == null) return;
                StatusTb.Text = info.ToString();
            });
        }

        private void _smtcListener_MediaPropertiesChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var info = await _smtcListener.GetMediaInfoAsync();
                if (info == null) return;
                TitleTb.Text = info.Title;
                ArtistTb.Text = info.Artist;
                AlbumTitleTb.Text = info.AlbumTitle;
                if (info.Thumbnail != null)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = (await info.Thumbnail.OpenReadAsync()).AsStream();
                    img.EndInit();
                    ThumbnailImg.Source = img;
                }
            });
        }

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            await _smtcListener.Previous();
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            await _smtcListener.PlayOrPause();
        }

        private async void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            await _smtcListener.Next();
        }
        bool EnableAppSMTC = false;
        private void SMTCBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EnableAppSMTC)
            {
                smtc.Text = "off";
                _smtcCreator?.Dispose();
                _smtcCreator = null;
                EnableAppSMTC = false;
            }
            else
            {
                _smtcCreator ??= new SMTCCreator("MediaTest");
                _smtcCreator.SetMediaStatus(SMTCMediaStatus.Playing);
                _smtcCreator.Info.SetAlbumTitle("AlbumTitle")
                    .SetArtist("Taylor Swift")
                    .SetTitle("Dancing With Our Hands Tied")
                    .SetThumbnail("https://y.qq.com/music/photo_new/T002R300x300M000003OK4yP2MBOip_1.jpg?max_age=2592000")
                    .Update();
                _smtcCreator.PlayOrPause += _smtcCreator_PlayOrPause;
                _smtcCreator.Previous += _smtcCreator_Previous;
                _smtcCreator.Next += _smtcCreator_Next;
                smtc.Text = "on";
                EnableAppSMTC = true;
            }
        }

        private void _smtcCreator_PlayOrPause(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => status.Text = "PlayOrPause");
        }
        private void _smtcCreator_Previous(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => status.Text = "Previous");
        }
        private void _smtcCreator_Next(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => status.Text = "Next");
        }
    }
}