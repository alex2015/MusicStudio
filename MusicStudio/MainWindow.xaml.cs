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
using MusicStudio.Models;
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
            InitTimer();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
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
                    playList.Items.Add(new ListBoxItemModel
                    {
                        Value = pathfilename,
                        DisplayValue = System.IO.Path.GetFileName(pathfilename)
                    });

                    Vars.Files.Add(pathfilename);
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //player = new PlayerWrapper(playList.SelectedItem.ToString(), PlayerWrapper.UriType.Common | PlayerWrapper.UriType.LocalFile);
            //player.Play();

            var selectItem = (ListBoxItemModel) playList.SelectedItem;

            if (selectItem != null && !string.IsNullOrWhiteSpace(selectItem.Value))
            {
                BassLike.Play(selectItem.Value, BassLike.Volume);
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

        private void Timer_Tick(object sender, EventArgs eventArgs)
        {
            lblCurrent.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
            slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
        }

        private void SlTime_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BassLike.SetPosOfScroll(BassLike.Stream, Convert.ToInt32(((Slider) e.Source).Value));
        }

        private void SlVol_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BassLike.SetVolumeStream(BassLike.Stream, Convert.ToInt32(((Slider) e.Source).Value));
        }
    }
}
