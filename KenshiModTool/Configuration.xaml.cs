using Core;
using System.Windows;

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

            if (LoadService.config == null)
            {
                MessageBox.Show("could not load configs, strange.... if this persists delete config.json.");
                this.Close();
            }
            txtSteamPath.Text = LoadService.config.SteamModsPath;
            txtMasterlist.Text = LoadService.config.MasterlistSource;
            TxtGamePath.Text = LoadService.config.GamePath;
            chk_updatesAutomatically.IsChecked = LoadService.config.CheckForUpdatesAutomatically;
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            LoadService.config.SteamModsPath = txtSteamPath.Text;
            LoadService.config.MasterlistSource = txtMasterlist.Text;
            LoadService.config.GamePath = TxtGamePath.Text;
            LoadService.config.CheckForUpdatesAutomatically = chk_updatesAutomatically.IsChecked ?? true;

            LoadService.SaveConfig();
        }
    }
}