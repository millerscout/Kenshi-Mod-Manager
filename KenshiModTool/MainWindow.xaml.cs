using Core;
using Core.Models;
using GuidelineCore;
using Microsoft.Win32;
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
        public Mod[] SearchList { get; set; } = new Mod[0];
        public int currentIndexSearch { get; set; } = 0;
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
            File.Copy(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), System.IO.Path.Combine(LoadService.config.GamePath, "data", "mods.cfg.backup"), true);

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
        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {

            var list = new List<Mod>();
            list.AddRange(ItemsList.Select(c => c));

            var mods = RuleService.OrderMods(list);

            ItemsList = new ObservableCollection<Mod>();

            foreach (var mod in mods.OrderBy(m => m.Order))
                ItemsList.Add(mod);

            listBox.ItemsSource = ItemsList;


        }

        private void saveModProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "mod profile.cfg",
            };
            saveFileDialog.Filter = "Config file (*.cfg)|*.cfg";

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, string.Join(Environment.NewLine, ItemsList.Where(m => m.Active).OrderBy(q => q.Order).Select(m => Path.GetFileName(m.Name))));
        }

        private void btnLoadProfile_Click(object sender, RoutedEventArgs e)
        {



            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                FileName = "mod profile.cfg",
            };
            fileDialog.Filter = "Config file (*.cfg)|*.cfg";

            string[] mods = new string[0];
            if (fileDialog.ShowDialog() == true)
                mods = File.ReadAllLines(fileDialog.FileName);

            foreach (var mod in ItemsList)
            {
                mod.Order = -1;
                mod.Active = false;
            }

            for (int i = 0; i < mods.Length; i++)
            {
                var profileMod = mods[i];
                var mod = ItemsList.FirstOrDefault(c => Path.GetFileName(c.Name) == profileMod);
                mod.Active = true;
                mod.Order = i;


            }

            listBox.ItemsSource = ItemsList.OrderBy(m => m.Order);

        }

        private void ToggleActive(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in listBox.SelectedItems)
            {
                mod.Active = !mod.Active;
                SetNewOrder(mod, mod.Active ? 0 : -1);
            }
        }

        private void DeactiveMods(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in listBox.SelectedItems)
            {
                mod.Active = false;
                SetNewOrder(mod, -1);
            }
        }

        private void ActiveMods(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in listBox.SelectedItems)
            {
                mod.Active = true;
                SetNewOrder(mod, 0);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {



            foreach (var item in ItemsList)
                item.Color = "";

            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new Mod[0];
            }
            else
            {

                var mod = ItemsList.FirstOrDefault(c => c.Name.Contains(txtSearch.Text));


                SearchList = ItemsList.Where(c => c.Name.ToLower().Contains(txtSearch.Text.ToLower())).OrderBy(m => m.Order).ToArray();

                if (SearchList.Length == 0)
                {
                    lblSearchInfo.Content = "Couldn't find any mod with the name.";
                }
                else
                {
                    lblSearchInfo.Content = $"Found:  {currentIndexSearch + 1}/{SearchList.Length}.";
                }


                foreach (var item in SearchList)
                {
                    item.Color = "Red";
                }
            }
            listBox.ItemsSource = ItemsList.OrderBy(c => c.Order);

            if (SearchList.Length > 0)
                listBox.ScrollIntoView(SearchList[currentIndexSearch]);


        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                if (string.IsNullOrEmpty(txtSearch.Text)) return;
                if (SearchList.Length > 0)
                {
                    if (currentIndexSearch == SearchList.Length - 1)
                        currentIndexSearch = 0;
                    else
                        currentIndexSearch++;

                    lblSearchInfo.Content = $"Found:  {currentIndexSearch + 1}/{SearchList.Length}.";

                    listBox.ScrollIntoView(SearchList[currentIndexSearch]);

                }
            }
        }
    }
}
