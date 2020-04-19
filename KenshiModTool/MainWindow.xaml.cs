using Core;
using Core.Models;
using GuidelineCore;
using KenshiModTool.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

namespace KenshiModTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Mod> ModList = new ObservableCollection<Mod>();
        public Mod[] SearchList { get; set; } = new Mod[0];
        public int currentIndexSearch { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();

            MainGrid.ShowGridLines = false;
            lblSearchInfo.Content = "";
            RtbDetail.Document.Blocks.Clear();

            LoadService.Setup();

            AskGamePathIfRequired();
            AskSteamPathIfRequired();

            LoadModList();
        }

        #region Environment Functions

        private void AskGamePathIfRequired()
        {
            if (string.IsNullOrEmpty(LoadService.config.GamePath))
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = Directory.Exists("C:\\prograSm files (x86)\\steam\\steamapps\\common\\Kenshi") ? "Is this Kenshi Folder?" : "What is the game folder?";
                dialog.InitialDirectory = "C:\\program files (x86)\\steam\\steamapps\\common\\Kenshi";
                CommonFileDialogResult result = dialog.ShowDialog();

                LoadService.config.GamePath = dialog.FileName;
            }
        }

        private void AskSteamPathIfRequired()
        {
            if (string.IsNullOrEmpty(LoadService.config.SteamModsPath))
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = Directory.Exists("C:\\ProgramS Files (x86)\\Steam\\steamapps\\workshop\\content\\233860") ? "Is this Kenshi Mod Folder (STEAM) P.s. 233860 is the id from kenshi ?" : "You need to select Kenshi Steam Folder, it's your steam folder + \"Steam\\steamapps\\workshop\\content\\233860";
                dialog.InitialDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\233860";
                CommonFileDialogResult result = dialog.ShowDialog();

                LoadService.config.SteamModsPath = dialog.FileName;
            }
        }

        #endregion Environment Functions

        #region Search

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentIndexSearch = 0;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new Mod[0];
            }
            else
            {
                var mod = ModList.FirstOrDefault(c => c.FilePath.Contains(txtSearch.Text));

                SearchList = ModList.Where(c => c.FilePath.ToLower().Contains(txtSearch.Text.ToLower()) || c.Id == txtSearch.Text).OrderBy(m => m.Order).ToArray();

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

        #endregion Search

        #region List Manipulation

        private void LoadModList()
        {
            ModList.Clear();

            foreach (var mod in LoadService.GetListOfMods())
                ModList.Add(mod);

            UpdateListBox();
        }

        private void UpdateListBox()
        {
            foreach (var mod in ModList)
            {
                mod.Color = SearchList.Any(s => s.UniqueIdentifier == mod.UniqueIdentifier) ? ModColors.SearchColor : "";
                var dependencies = mod
                    .Dependencies
                    .Concat(mod.References)
                    .Where(c => !Constants.SkippableMods.Contains(c.ToLower()));
                if (dependencies.Count() > 0)
                {
                    if (!ModList.Where(m => m.Active).Any(c => dependencies.Any(r => r.ToLower().Contains(c.FileName.ToLower()))))
                    {
                        mod.Color = ModColors.RequisiteNotFoundColor;
                    }
                }
            }

            ListBox.ItemsSource = ModList.OrderBy(q => q.Order);

            if (SearchList.Length > 0)
                ListBox.ScrollIntoView(SearchList[currentIndexSearch]);
        }

        public void SetNewOrder(Mod current, int New)
        {
            Dictionary<Guid, Tuple<int, bool>> order = new Dictionary<Guid, Tuple<int, bool>> {
                {current.UniqueIdentifier, new Tuple<int, bool>(New,true)}
            };
            foreach (var item in ModList.Where(q => q.Order != -1 && q.Order >= New && q.UniqueIdentifier != current.UniqueIdentifier).OrderBy(c => c.Order))
                order.Add(item.UniqueIdentifier, new Tuple<int, bool>(item.Order, false));

            var i = New + 1;
            foreach (var item in order.Where(q => !q.Value.Item2).OrderBy(c => c.Value.Item1))
            {
                order[item.Key] = new Tuple<int, bool>(i, true);
                i++;
            }

            foreach (var item in order.Where(q => q.Value.Item2))
            {
                ModList.FirstOrDefault(m => m.UniqueIdentifier == item.Key).Order = item.Value.Item1;
            }

            UpdateListBox();
        }

        private void RibbonTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox lbi = (sender as TextBox);
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

        private void UpdateCheckBox(object sender, RoutedEventArgs e)
        {
            CheckBox lbi = (sender as CheckBox);

            var mod = ((Mod)lbi.DataContext);
            if (mod.Order == -1)
            {
                SetNewOrder(mod, 0);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox.SelectedItems.Count > 0)
            {
                var list = new List<string>();
                Mod mod = ListBox.SelectedItems[0] as Mod;
                FlowDocument document = new FlowDocument();

                Paragraph paragraph = new Paragraph();

                if (mod.Dependencies != null && mod.Dependencies.Count > 0)
                    Write("Dependencies:", string.Join(", ", mod.Dependencies));
                if (mod.References != null && mod.References.Count > 0)
                    Write("References:", string.Join(", ", mod.References));

                Write("Author:", mod.Author);
                Write("Version:", mod.Version);
                WriteUrl("FilePath:", mod.FilePath, true);
                WriteUrl("Url:", mod.Url);
                Write("Description:", mod.Description);

                RtbDetail.Document.Blocks.Clear();

                RtbDetail.Document = document;

                void Write(string title, string Value)
                {
                    Value = Value ?? "";
                    paragraph.Inlines.Add(new Bold(new Run(title)));
                    paragraph.Inlines.Add($" {Value}{Environment.NewLine}");
                    document.Blocks.Add(paragraph);
                }

                void WriteUrl(string title, string Value, bool local = false)
                {
                    Value = Value ?? "";
                    paragraph.Inlines.Add(new Bold(new Run(title)));
                    Hyperlink textLink = new Hyperlink(new Run($" {Value}{Environment.NewLine}"));
                    textLink.NavigateUri = new Uri(local ? Path.GetDirectoryName(Value) : Value);
                    textLink.RequestNavigate += TextLink_RequestNavigate;
                    paragraph.Inlines.Add(textLink);

                    document.Blocks.Add(paragraph);
                }
            }
            else
            {
                RtbDetail.Document.Blocks.Clear();
            }
        }

        #endregion List Manipulation

        #region GUI Buttons.

        private void SaveModList_Click(object sender, RoutedEventArgs e)
        {
            File.Copy(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), Path.Combine(LoadService.config.GamePath, "data", "mods.cfg.backup"), true);

            File.WriteAllLines(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), ModList.Where(m => m.Active).OrderBy(q => q.Order).Select(m => m.FileName));
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

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<Mod>();
            list.AddRange(ModList.Select(c => c));

            var mods = RuleService.OrderMods(list);

            ModList = new ObservableCollection<Mod>();

            foreach (var mod in mods.OrderBy(m => m.Order))
                ModList.Add(mod);

            UpdateListBox();
        }

        private void SaveModProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "mod profile.cfg",
            };
            saveFileDialog.Filter = "Config file (*.cfg)|*.cfg";

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, string.Join(Environment.NewLine, ModList.Where(m => m.Active).OrderBy(q => q.Order).Select(m => m.FileName)));
        }

        private void BtnLoadProfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                FileName = "mod profile.cfg",
            };
            fileDialog.Filter = "Config file (*.cfg)|*.cfg";

            string[] mods = new string[0];
            if (fileDialog.ShowDialog() == true)
                mods = File.ReadAllLines(fileDialog.FileName);

            foreach (var mod in ModList)
            {
                mod.Order = -1;
                mod.Active = false;
            }

            for (int i = 0; i < mods.Length; i++)
            {
                var profileMod = mods[i];
                var mod = ModList.FirstOrDefault(c => c.FileName == profileMod);
                mod.Active = true;
                mod.Order = i;
            }

            UpdateListBox();
        }

        private void BtnLaunchGameClick(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "steam://rungameid/233860",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void BtnRefreshMods_Click(object sender, RoutedEventArgs e)
        {
            currentIndexSearch = 0;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lblSearchInfo.Content = "";
                SearchList = new Mod[0];
            }

            LoadModList();
        }

        #endregion GUI Buttons.

        #region Context Menu actions

        private void CheckModPage_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in ListBox.SelectedItems)
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

        private void ToggleActive(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in ListBox.SelectedItems)
            {
                mod.Active = !mod.Active;
                SetNewOrder(mod, mod.Active ? 0 : -1);
            }
        }

        private void DeactiveMods(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in ListBox.SelectedItems)
            {
                mod.Active = false;
                SetNewOrder(mod, -1);
            }
        }

        private void ActiveMods(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in ListBox.SelectedItems)
            {
                mod.Active = true;
                SetNewOrder(mod, 0);
            }
        }

        #endregion Context Menu actions

        #region Details controls

        private void TextLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        #endregion Details controls
    }
}