//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
//using WebWindows;

//namespace KenshiModTool
//{
//    /// <summary>
//    /// Interaction logic for SelectVersion.xaml
//    /// </summary>
//    public partial class SelectVersion : Window
//    {
//        public SelectVersion()
//        {
//            InitializeComponent();
//        }

//        WebWindow window;
//        private void NewVersion(object sender, RoutedEventArgs e)
//        {
//            this.Close();
// for future versions:
//      https://www.nuget.org/packages/WebWindow
//            window = new WebWindow("Kenshi Mod Tool by MillerScout",(WebWindowOptions options)=>{
//            });
//            var workingArea = window.Monitors.FirstOrDefault().WorkArea;
//            window.Location = new System.Drawing.Point()
//            {
//                X = Convert.ToInt32(Math.Max(workingArea.X, workingArea.X + (workingArea.Width - window.Width) / 2)),
//                Y = Convert.ToInt32(Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - window.Height) / 2))
//            };
//            window.SizeChanged += Window_SizeChanged;


//            window.SetIconFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "icon.ico"));
//            window.NavigateToUrl("http://localhost:5000/index.html");


//            window.WaitForExit();

//        }

//        private void Window_SizeChanged(object sender, System.Drawing.Size e)
//        {
//            window.Height = e.Height <= 600 ? 600 : e.Height;
//            window.Width = e.Width <= 800 ? 800 : e.Width;
//        }

//        private void OldVersion(object sender, RoutedEventArgs e)
//        {
//            var main = new MainWindow();
//            main.Show();

//            this.Close();
//        }
//    }
//}
