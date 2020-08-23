using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebWindows;

namespace KenshiModTool
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        public Configuration()
        {
            InitializeComponent();

            txtSteamPath.Text = LoadService.config.SteamModsPath;
            txtMasterlist.Text = LoadService.config.MasterlistSource;
            TxtGamePath.Text = LoadService.config.GamePath;

        }

        private void Save(object sender, RoutedEventArgs e)
        {
            LoadService.config.SteamModsPath = txtSteamPath.Text;
            LoadService.config.MasterlistSource = txtMasterlist.Text;
            LoadService.config.GamePath = TxtGamePath.Text;

            LoadService.SaveConfig();

        }
    }
}
