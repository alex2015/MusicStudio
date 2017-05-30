using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Target);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void btnOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = MusicStudio.Resources.OpenFileDialogFilter
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string pathfilename in openFileDialog.FileNames)
                {
                    var fileName = System.IO.Path.GetFileName(pathfilename);
                    playList.Items.Add(fileName);

                    Vars.Files.Add(fileName);
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var q1 = playList.SelectedItems;
            var q2 = playList.SelectedIndex;
            var q3 = playList.SelectedItem;

            var q4 = playList.SelectedValue;
            var q5 = playList.SelectedValuePath;



            //player = new PlayerWrapper(playList.SelectedItem.ToString(), PlayerWrapper.UriType.Common | PlayerWrapper.UriType.LocalFile);
            //player.Play();



            if (playList.SelectedItem != null && !string.IsNullOrWhiteSpace(playList.SelectedItem.ToString()))
            {
                BassLike.Play(playList.SelectedItem.ToString(), BassLike.Volume);
                lblCurrent.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                lblLength.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();

                slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);

                timer.Start();
            }

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            //player.Pause();


            BassLike.Stop();
            timer.Stop();
            slTime.Value = 0;
            lblCurrent.Content = "00:00:00";
        }

        private void Target(object sender, EventArgs eventArgs)
        {
            lblCurrent.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
            slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
        }

        private void TimerSliderScroll()
        {
            // вместо 10 вписать позицию слайдера
            BassLike.SetPosOfScroll(BassLike.Stream, 10);
        }

        private void VolumeSliderScroll()
        {
            // вместо 10 вписать позицию слайдера
            BassLike.SetVolumeStream(BassLike.Stream, 10);
        }
    }
}
