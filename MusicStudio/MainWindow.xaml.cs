using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using Player;

namespace MusicStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private PlayerWrapper player;
        private BassWrapper player;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            player = new BassWrapper();
            player.InitBass();
            InitTimer();
            ItemsControl();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void ItemsControl()
        {
            playList.ItemsSource = PlayerInfo.filesInfo;
        }

        private async void btnOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = MusicStudio.Resources.OpenFileDialogFilter
            };

            if (openFileDialog.ShowDialog() == true)
            {
                parseFilePathsStart(openFileDialog.FileNames);
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //player = new PlayerWrapper(playList.SelectedItem.ToString(), PlayerWrapper.UriType.Common | PlayerWrapper.UriType.LocalFile);
            //player.Play();

            var selectItem = (TrackModel) playList.SelectedItem;

            if (selectItem != null && !string.IsNullOrWhiteSpace(selectItem.PathFileName))
            {
                PlayerInfo.currentTrackNumber = playList.SelectedIndex;

                player.Play(selectItem.PathFileName);
                setTimeSteamInfo();

                timer.Start();
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            //player.Pause();
            player.Stop();
            timer.Stop();
            slTime.Value = 0;
            lblCurrent.Content = "00:00:00";
        }

        private void Timer_Tick(object sender, EventArgs eventArgs)
        {
            lblCurrent.Content = TimeSpan.FromSeconds(player.GetPosOfStream()).ToString();
            slTime.Value = player.GetPosOfStream();

            if (player.ToNextTrack(PlayerInfo.filesInfo.Count > PlayerInfo.currentTrackNumber + 1))
            {
                playList.SelectedIndex = PlayerInfo.currentTrackNumber;
                setTimeSteamInfo();
            }

            if (player.EndPlaylist)
            {
                btnStop_Click(this, new RoutedEventArgs());
                playList.SelectedIndex = PlayerInfo.currentTrackNumber = 0;
                player.EndPlaylist = false;
            }
        }

        private void SlTime_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            player.SetPosOfScroll(Convert.ToInt32(((Slider) e.Source).Value));
        }

        private void SlVol_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            player.SetVolumeStream(Convert.ToInt32(((Slider) e.Source).Value));
        }

        private async void PlayList_OnDrop(object sender, DragEventArgs e)
        {
            var pathfileNames = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (pathfileNames != null)
            {
                pathfileNames = pathfileNames.Where(i => player.SupportedAudioFileFormats.Contains(System.IO.Path.GetExtension(i))).ToArray();

                if (pathfileNames.Any())
                {
                    parseFilePathsStart(pathfileNames);
                }
            }
        }

        private async void parseFilePathsStart(string[] pathfileNames)
        {
            progressBar.Visibility = Visibility.Visible;
            await Task.Run(() => parseFilePaths(pathfileNames));
            progressBar.Visibility = Visibility.Hidden;
            progressBar.Value = 0;
        }

        private void parseFilePaths(string[] pathfileNames)
        {
            var length = pathfileNames.Length;

            for (int p = 0; p < length; p++)
            {
                var pathfileName = pathfileNames[p];

                if (PlayerInfo.filesInfo.All(i => i.PathFileName != pathfileName))
                {
                    var tm = new TrackModel(pathfileName);

                    var p1 = p + 1;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PlayerInfo.filesInfo.Add(tm);
                        progressBar.Value = p1 * 100.0 / length;
                    });
                }
            }
        }

        private void setTimeSteamInfo()
        {
            var currentPosStream = player.GetPosOfStream();
            var timeLengthStream = player.GetTimeOfStream();

            lblCurrent.Content = TimeSpan.FromSeconds(currentPosStream).ToString();
            lblLength.Content = TimeSpan.FromSeconds(timeLengthStream).ToString();

            slTime.Maximum = timeLengthStream;
            slTime.Value = currentPosStream;
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            playList.Items.Filter = i => ((TrackModel) i).ToString().ToUpperInvariant().Contains(((TextBox) e.Source).Text.ToUpperInvariant());
        }

        private void PlayList_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var listBox = (ListBox) sender;
                var selectIndex = listBox.SelectedIndex;
                PlayerInfo.filesInfo.Remove((TrackModel)listBox.SelectedItem);
                listBox.SelectedIndex = Math.Min(selectIndex, PlayerInfo.filesInfo.Count - 1);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            player.Dispose();
            player = null;
        }
    }
}
