//using Microsoft.AspNetCore.Hosting;
using Core;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.AppendAllText(Constants.Errorfile, $"{JsonConvert.SerializeObject(e.ExceptionObject)} {Environment.NewLine}");
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} -  {e.Exception.Message}.{Environment.NewLine}");
            File.AppendAllText(Constants.Errorfile, $"{e.Exception.StackTrace} {Environment.NewLine}");

        }


    }

}

