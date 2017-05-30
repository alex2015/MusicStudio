using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Aac;
using Un4seen.Bass.AddOn.Flac;
using Un4seen.Bass.AddOn.Wma;

namespace MusicStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            BassNet.Registration("alex2015@rambler.ru", "2X3521193738");
            BassNet.UseBrokenLatin1Behavior = true;

            //if (!Un4seen.Bass.Bass.BASS_Init(-1, 48000, (BASSInit)0, IntPtr.Zero) && !Un4seen.Bass.Bass.BASS_Init(0, 48000, (BASSInit)0, IntPtr.Zero))
            //    Console.WriteLine("Unable to initialize BASS");
            //BassAac.LoadMe();
            //BassFlac.LoadMe();
            //BassWma.LoadMe();
            //Un4seen.Bass.Bass.BASS_SetConfig((BASSConfig)11, 30000);
            //Un4seen.Bass.Bass.BASS_SetConfig((BASSConfig)67328, true);
            //Un4seen.Bass.Bass.BASS_SetConfig((BASSConfig)67329, true);
            //Un4seen.Bass.Bass.BASS_SetConfig((BASSConfig)65795, true);







            //GlobalDispatcher.Initialize();
            //App.InitializeApplicationSettings();
            //StatisticLog.LogApplicationStart(this.AppId, ApplicationSettings.Instance.Version);
            //if (ApplicationSettings.Instance.IsFirstRun)
            //{
            //    StatisticLog.ActivateStatisticsService();
            //    this.Logger.Info("Statistics service activated");
            //}

            // ISSUE: method pointer
            //this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler((object)this, __methodptr(App_DispatcherUnhandledException));
            //App.SetCulture();
            //MainView mainView = new MainView();
            //this._mainViewModel = new MainViewModel(mainView);
            //mainView.DataContext = (object)this._mainViewModel;
            //ViewRegistry.Instance.Register((ViewModelBase)(mainView.DataContext as MainViewModel), (Window)mainView);
            //this.SetViewSize(mainView);
            //this.SetViewPositionAndState(mainView);
            //mainView.Show();
            //this.ReleaseStarterSemaphore();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Un4seen.Bass.Bass.BASS_Free();
        }
    }
}
