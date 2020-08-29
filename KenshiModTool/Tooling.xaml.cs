using Core;
using Core.Models;
using KenshiModTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KenshiModTool
{
    /// <summary>
    /// Interaction logic for Tooling.xaml
    /// </summary>
    public partial class Tooling : Window
    {
        public ObservableCollection<ModFolder> ModList = new ObservableCollection<ModFolder>();
        public int currentIndexSearch { get; set; } = 0;
        public ModFolder[] SearchList { get; set; } = new ModFolder[0];

        public Tooling()
        {
            try
            {
                InitializeComponent();

                this.Title = $"[v{Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}] - {this.Title}";
                MainGrid.ShowGridLines = false;
                lblSearchInfo.Content = "";

                LoadModList();
            }
            catch (Exception ex)
            {
                Logging.WriteError(ex);
            }
        }

        public void UpdateListBox()
        {
            foreach (var mod in ModList)
                mod.Color = SearchList.Any(s => s.UniqueIdentifier == mod.UniqueIdentifier) ? ModColors.SearchColor : "";

            ListBox.ItemsSource = new List<Object>();
            ListBox.ItemsSource = ModList;

            if (SearchList.Length > 0)
                ListBox.ScrollIntoView(SearchList[currentIndexSearch]);
        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentIndexSearch = 0;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new ModFolder[0];
            }
            else
            {
                var mod = ModList.FirstOrDefault(c => c.FilePath.Contains(txtSearch.Text));

                SearchList = ModList.Where(c => c.FilePath.Contains(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) || c.DisplayName.Contains(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                if (SearchList.Length == 0)
                {
                    lblSearchInfo.Content = "Couldn't find any mod with the name.";
                }
                else
                {
                    lblSearchInfo.Content = $"Found:  {currentIndexSearch + 1}/{SearchList.Length}.";
                }
            }

            UpdateListBox();
        }

        public void TxtSearch_KeyDown(object sender, KeyEventArgs e)
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

                    ListBox.ScrollIntoView(SearchList[currentIndexSearch]);
                }
            }
        }

        public void BtnClearEmptyFolders_Click(object sender, RoutedEventArgs e)
        {
            LoadService.FolderCleanUp(LoadService.config.SteamModsPath);
            LoadService.FolderCleanUp(LoadService.config.ModFolder);
        }

        public void BtnRemoveSymbLinks_Click(object sender, RoutedEventArgs e)
        {
            var symblist = ModList
               .Where(c => c.Source == SourceEnum.Steam && c.HasSymbLink && c.Selected)
               .Select(c => System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(c.FilePath)));
            foreach (var folder in symblist)
            {
                LoadService.DeleteFolder(folder);
            }

            LoadModList();
        }

        public void BtnCreateSymbLinks_Click(object sender, RoutedEventArgs e)
        {
            var symblist = ModList
                .Where(c => c.Source == SourceEnum.Steam && !c.HasSymbLink && c.Selected)
                .Select(c => new Tuple<string, string>(
                        System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(c.FilePath)),
                        System.IO.Path.GetDirectoryName(c.FilePath)
                    )
                );

            if (!symblist.Any()) return;

            var logging = Task.Run(() =>
            {
                var appendLog = new List<string> {
                $"{DateTime.Now} - Trying to Create SymbLinks:"
                };

                appendLog.Add($"{DateTime.Now} - Detailed List:");
                appendLog.AddRange(symblist.Select(item => $"{DateTime.Now} From {item.Item2} To {item.Item1}:"));

                File.AppendAllLines(Path.Combine( Directory.GetCurrentDirectory(),  Constants.Logfile), appendLog);
            });

            LoadService.CreateSymbLink(symblist);

            logging.Wait();
            LoadModList();
        }

        public void BtnLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            CommonService.StartGame();
        }

        public void btnReloadMods_Click(object sender, RoutedEventArgs e)
        {
            currentIndexSearch = 0;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new ModFolder[0];
            }

            LoadModList();
        }

        public void LoadModList()
        {
            ModList.Clear();

            foreach (var mod in LoadService.GetListOfMods())
                ModList.Add(new ModFolder(mod));

            UpdateListBox();
        }

        public void BtnGameFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.GamePath, () => MessageBox.Show("Game folder not configured correctly."));
        }

        public void GameModFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.ModFolder, () => MessageBox.Show("Game folder not configured correctly."));
        }

        public void BtnSteamFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.SteamModsPath, () => MessageBox.Show("steam folder not configured correctly."));
        }

        public void OpenModFolder_Click(object sender, RoutedEventArgs e)
        {
            bool failure = false;
            foreach (var mod in ListBox.SelectedItems)
            {
                CommonService.OpenFolder(System.IO.Path.GetDirectoryName(((ModFolder)mod).FilePath), () => { failure = true; });
            }

            if (failure)
            {
                MessageBox.Show("Game folder not configured correctly.");
            }
        }

        public void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModList.Where(c => !c.Selected)) item.Selected = true;

            UpdateListBox();
        }

        public void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModList) item.Selected = !item.Selected;

            UpdateListBox();
        }

        public void CreateSymbSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox.SelectedItems != null && ListBox.SelectedItems.Count > 0)
            {
                ExecuteSymbLink(ShouldDelete: false);

                LoadModList();
            }
        }

        public void ExecuteSymbLink(bool ShouldDelete = true, bool ShouldAdd = true)
        {
            var symblist = new List<Tuple<string, string>>();
            foreach (ModFolder mod in ListBox.SelectedItems)
            {
                if (mod.HasSymbLink)
                {
                    if (ShouldDelete)
                        LoadService.DeleteFolder(System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(mod.FilePath)));
                }
                else
                {
                    if (ShouldAdd)
                        symblist.Add(new Tuple<string, string>(
                      System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(mod.FilePath)),
                      System.IO.Path.GetDirectoryName(mod.FilePath)
                      ));
                }
            }

            try
            {
                if (!symblist.Any()) return;

                var appendLog = new List<string> {
                            $"{DateTime.Now} - Trying to Create SymbLinks:"
                            };

                appendLog.Add($"{DateTime.Now} - Detailed List:");
                appendLog.AddRange(symblist.Select(item => $"{DateTime.Now} From {item.Item2} To {item.Item1}:"));

                File.AppendAllLines(Path.Combine( Directory.GetCurrentDirectory(),  Constants.Logfile), appendLog);
            }
            catch (Exception ex)
            {
                Logging.WriteError("Failed to write log of symblinks.");
                Logging.WriteError(ex);
                
            }

            LoadService.CreateSymbLink(symblist);
        }

        public void ToggleSymbSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox.SelectedItems != null && ListBox.SelectedItems.Count > 0)
            {
                ExecuteSymbLink();

                LoadModList();
            }
        }

        public void RemoveSymbSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox.SelectedItems != null && ListBox.SelectedItems.Count > 0)
            {
                ExecuteSymbLink(ShouldAdd: false);

                LoadModList();
            }
        }
    }
}