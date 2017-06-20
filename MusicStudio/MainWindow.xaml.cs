﻿using System;
using System.Collections.Generic;
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
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            BassLike.InitBass();
            InitTimer();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
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
                await Task.Run(() => parseFilePaths(openFileDialog.FileNames));
                playList.ItemsSource = Vars.filesInfo;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //player = new PlayerWrapper(playList.SelectedItem.ToString(), PlayerWrapper.UriType.Common | PlayerWrapper.UriType.LocalFile);
            //player.Play();

            var selectItem = (TagModel) playList.SelectedItem;

            if (selectItem != null && !string.IsNullOrWhiteSpace(selectItem.PathFileName))
            {
                Vars.currentTrackNumber = playList.SelectedIndex;

                BassLike.Play(selectItem.PathFileName);
                setTimeSteamInfo();

                timer.Start();
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            BassLike.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            //player.Pause();
            BassLike.Stop();
            timer.Stop();
            slTime.Value = 0;
            lblCurrent.Content = "00:00:00";
        }

        private void Timer_Tick(object sender, EventArgs eventArgs)
        {
            lblCurrent.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream()).ToString();
            slTime.Value = BassLike.GetPosOfStream();

            if (BassLike.ToNextTrack(Vars.filesInfo.Count > Vars.currentTrackNumber + 1))
            {
                playList.SelectedIndex = Vars.currentTrackNumber;
                setTimeSteamInfo();
            }

            if (BassLike.EndPlaylist)
            {
                btnStop_Click(this, new RoutedEventArgs());
                playList.SelectedIndex = Vars.currentTrackNumber = 0;
                BassLike.EndPlaylist = false;
            }
        }

        private void SlTime_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BassLike.SetPosOfScroll(Convert.ToInt32(((Slider) e.Source).Value));
        }

        private void SlVol_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            BassLike.SetVolumeStream(Convert.ToInt32(((Slider) e.Source).Value));
        }

        private async void PlayList_OnDrop(object sender, DragEventArgs e)
        {
            await Task.Run(() => parseFilePaths((string[]) e.Data.GetData(DataFormats.FileDrop)));
            playList.ItemsSource = Vars.filesInfo;
        }

        private void parseFilePaths(IEnumerable<string> pathfileNames)
        {
            foreach (string pathfilename in pathfileNames)
            {
                var tm = new TagModel(pathfilename);

                if (Vars.filesInfo.All(i => i.PathFileName != pathfilename))
                {
                    Vars.filesInfo.Add(tm);
                }
            }
        }

        private void setTimeSteamInfo()
        {
            var currentPosStream = BassLike.GetPosOfStream();
            var timeLengthStream = BassLike.GetTimeOfStream();

            lblCurrent.Content = TimeSpan.FromSeconds(currentPosStream).ToString();
            lblLength.Content = TimeSpan.FromSeconds(timeLengthStream).ToString();

            slTime.Maximum = timeLengthStream;
            slTime.Value = currentPosStream;
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            playList.Items.Filter = i => ((TagModel) i).ToString().ToUpperInvariant().Contains(((TextBox) e.Source).Text.ToUpperInvariant());
        }
    }
}
