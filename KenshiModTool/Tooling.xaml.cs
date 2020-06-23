using Core;
using Core.Models;
using KenshiModTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

                MainGrid.ShowGridLines = false;
                lblSearchInfo.Content = "";

                LoadModList();
            }
            catch (Exception ex)
            {
                File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} -  {ex.Message}.{Environment.NewLine}");
                File.AppendAllText(Constants.Errorfile, $"{ex.StackTrace} {Environment.NewLine}");
            }
        }

        private void UpdateListBox()
        {
            foreach (var mod in ModList)
            {
                mod.Color = SearchList.Any(s => s.UniqueIdentifier == mod.UniqueIdentifier) ? ModColors.SearchColor : "";
            }


            ListBox.ItemsSource = ModList;

            if (SearchList.Length > 0)
                ListBox.ScrollIntoView(SearchList[currentIndexSearch]);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
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
        private void BtnClearEmptyFolders_Click(object sender, RoutedEventArgs e)
        {
            LoadService.FolderCleanUp(LoadService.config.SteamModsPath);
            LoadService.FolderCleanUp(LoadService.config.ModFolder);
        }

        private void BtnRemoveSymbLinks_Click(object sender, RoutedEventArgs e)
        {
            var symblist = ModList
               .Where(c => c.Source == SourceEnum.Steam && c.HasSymbLink)
               .Select(c => System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(c.FilePath)));
            foreach (var folder in symblist)
            {
                LoadService.DeleteFolder(folder);
            }


            LoadModList();
        }

        private void BtnCreateSymbLinks_Click(object sender, RoutedEventArgs e)
        {
            var symblist = ModList
                .Where(c => c.Source == SourceEnum.Steam && !c.HasSymbLink)
                .Select(c => new Tuple<string, string>(
                    System.IO.Path.Combine(LoadService.config.ModFolder, System.IO.Path.GetFileNameWithoutExtension(c.FilePath)),
                    System.IO.Path.GetDirectoryName(c.FilePath)
                    )
                );
            LoadService.CreateSymbLink(symblist);

            LoadModList();
        }

        private void BtnLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            CommonService.StartGame();
        }

        private void btnReloadMods_Click(object sender, RoutedEventArgs e)
        {
            currentIndexSearch = 0;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new ModFolder[0];
            }

            LoadModList();
        }

        private void LoadModList()
        {
            ModList.Clear();

            foreach (var mod in LoadService.GetListOfMods())
                ModList.Add(new ModFolder(mod));

            UpdateListBox();
        }

        private void BtnGameFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.GamePath, () => MessageBox.Show("Game folder not configured correctly."));
        }

        private void GameModFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.ModFolder, () => MessageBox.Show("Game folder not configured correctly."));
        }

        private void BtnSteamFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.SteamModsPath, () => MessageBox.Show("steam folder not configured correctly."));
        }
        private void OpenModFolder_Click(object sender, RoutedEventArgs e)
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
    }
}
