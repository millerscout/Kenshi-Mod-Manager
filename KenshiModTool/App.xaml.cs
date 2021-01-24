//using Microsoft.AspNetCore.Hosting;
using Core;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace KenshiModTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            AppCenter.Start("18ca8610-280d-4fe8-8275-c876357993a4", typeof(Analytics), typeof(Crashes));
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


#if DEBUG
            AppCenter.SetEnabledAsync(false);
#endif
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            Logging.Write(Constants.Errorfile, $"{JsonConvert.SerializeObject(e.ExceptionObject)}");
            return;
        }

        public void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Logging.Write(Constants.Errorfile, $"{e.Exception.Message}");
            Logging.Write(Constants.Errorfile, $"{e.Exception.StackTrace}");
            return;
        }
    }
}