using Core;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace KenshiModTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Mod> ItemsList = new ObservableCollection<Mod>();
        public MainWindow()
        {

            InitializeComponent();


            LoadService.Setup();



            foreach (var mod in LoadService.GetListOfMods())
                ItemsList.Add(mod);
            listBox.ItemsSource = ItemsList.OrderBy(q => q.Order);

        }

        private void RibbonCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        public void SetNewOrder(Mod current, int New)
        {
            Dictionary<Guid, Tuple<int, bool>> order = new Dictionary<Guid, Tuple<int, bool>> {
                {current.UniqueIdentifier, new Tuple<int, bool>(New,true)}
            };
            foreach (var item in ItemsList.Where(q => q.Order != -1 && q.Order >= New && q.UniqueIdentifier != current.UniqueIdentifier).OrderBy(c => c.Order))
                order.Add(item.UniqueIdentifier, new Tuple<int, bool>(item.Order, false));



            var i = New + 1;
            foreach (var item in order.Where(q => !q.Value.Item2).OrderBy(c => c.Value.Item1))
            {
                order[item.Key] = new Tuple<int, bool>(i, true);
                i++;
            }

            foreach (var item in order.Where(q => q.Value.Item2))
            {
                ItemsList.FirstOrDefault(m => m.UniqueIdentifier == item.Key).Order = item.Value.Item1;
            }

            listBox.ItemsSource = ItemsList.OrderBy(q => q.Order);

        }

        private void RibbonTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                RibbonTextBox lbi = (sender as RibbonTextBox);
                if (int.TryParse(lbi.Text, out var value))
                {
                    var mod = ((Mod)lbi.DataContext);
                    SetNewOrder(mod, value);
                }
                else
                {
                    MessageBox.Show("you should use numbers only!");
                    lbi.Text = (lbi.DataContext as Mod).Order.ToString();
                }

            }
        }


        private void CheckModPage_Click(object sender, RoutedEventArgs e)
        {

            foreach (var mod in listBox.SelectedItems)
            {

                if (!string.IsNullOrEmpty(((Mod)mod).Url))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = ((Mod)mod).Url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }

        }

        private void SaveModList_Click(object sender, RoutedEventArgs e)
        {
            File.Copy(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), System.IO.Path.Combine(LoadService.config.GamePath, "data", "mods.cfg.backup"));
            
            File.WriteAllLines(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), ItemsList.Where(m => m.Active).OrderBy(q => q.Order).Select(m => Path.GetFileName(m.Name)));
        }

        private void BtnGameFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(LoadService.config.GamePath, "Game folder not configured correctly.");
        }


        private void GameModFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(System.IO.Path.Combine(LoadService.config.GamePath, "Mods"), "Game folder not configured correctly.");
        }

        private void BtnSteamFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(LoadService.config.SteamModsPath, "steam folder not configured correctly.");
        }
        private static void OpenFolder(string folder, string NotFoundMessage)
        {
            if (Directory.Exists(folder))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show(NotFoundMessage);
            }
        }

        private void UpdateCheckBox(object sender, RoutedEventArgs e)
        {
            RibbonCheckBox lbi = (sender as RibbonCheckBox);

            var mod = ((Mod)lbi.DataContext);
            if (mod.Order == -1)
            {
                SetNewOrder(mod, 0);
            }

        }
    }
}
