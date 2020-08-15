//using Microsoft.AspNetCore.Hosting;
using Core;
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
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            //var builder = WebServer.Builder
            //    .CreateWebHostBuilder();
            //var task = Task.Run(() =>
            //{
            //    builder.Run();
            //});
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} -  {e.Exception.Message}.{Environment.NewLine}");
            File.AppendAllText(Constants.Errorfile, $"{e.Exception.StackTrace} {Environment.NewLine}");

        }


    }

}

