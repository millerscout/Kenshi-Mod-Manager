using AutoUpdaterDotNET;
using Core;
using Core.Models;
using KenshiModTool.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        public ConcurrentDictionary<string, ModListChanges> ConflictIndex = new ConcurrentDictionary<string, ModListChanges>();
        public ConcurrentDictionary<string, DetailChanges> DetailIndex = new ConcurrentDictionary<string, DetailChanges>();
        public bool ShowConflicts { get; set; } = false;

        public System.Timers.Timer updateTimer = new System.Timers.Timer(TimeSpan.FromHours(12).TotalMilliseconds);


        public MainWindow()
        {
            try
            {
                InitializeComponent();
                AutoUpdater.DownloadPath = Environment.CurrentDirectory;

                this.Title = $"[v{Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}] - {this.Title}";

                if (SystemParameters.PrimaryScreenWidth >= 1000) this.Width = 1000;
                else if (SystemParameters.PrimaryScreenWidth >= 750) this.Width = 750;

                MainGrid.ShowGridLines = false;
                lblSearchInfo.Content = "";
                RtbDetail.Document.Blocks.Clear();
                lsView.Items.Clear();

                Style itemContainerStyle = lsView.ItemContainerStyle;
                itemContainerStyle.Setters.Add(new Setter(ListViewItem.AllowDropProperty, true));
                itemContainerStyle.Setters.Add(new EventSetter(ListViewItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewDragAndDrop)));
                itemContainerStyle.Setters.Add(new EventSetter(ListViewItem.DropEvent, new DragEventHandler(SetDropAction)));
                lsView.ContextMenuOpening += LsView_ContextMenuOpening;
                LoadService.Setup();

                AskGamePathIfRequired();
                AskSteamPathIfRequired();
                LoadService.SaveConfig();

                LoadModList();

                if (ModList.Count > 0)
                {
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), $"{Constants.BackupSubscribeList}*").OrderBy(c => c);
                    if (files.Count() > LoadService.config.MaxLogFiles)
                        File.Delete(files.FirstOrDefault());

                    File.WriteAllText($"{Constants.BackupSubscribeList}{DateTime.Now:yyyyMMddHHmmss}.info", string.Join(Environment.NewLine, ModList.Where(c => c.Source == SourceEnum.Steam).Select(q => q.Id)));
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var source = ((GridView)lsView.View).Columns[2].Header as GridViewColumnHeader;
                    SortHeaderClick(source, null);

                    if (LoadService.config.CheckForUpdatesAutomatically)
                    {
                        AutoUpdater.Start(Constants.UpdateListUrl);
                        updateTimer.Start();
                    }

                }), System.Windows.Threading.DispatcherPriority.ContextIdle, null);

                updateTimer.Elapsed += UpdateTimer_Elapsed;
            }
            catch (Exception ex)
            {
                File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} -  {ex.Message}.{Environment.NewLine}");
                File.AppendAllText(Constants.Errorfile, $"{ex.StackTrace} {Environment.NewLine}");
            }
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            updateTimer.Stop();
            AutoUpdater.Start(Constants.UpdateListUrl);
            updateTimer.Start();
        }

        private void LsView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lsView.SelectedItems.Count > 0)
            {
                var q = lsView.ContextMenu;
                foreach (Mod mod in lsView.SelectedItems)
                {
                    if (mod.Source == SourceEnum.Steam)
                    {
                        MenuUnSubs.Visibility = Visibility.Visible;
                        MenuUnSubs.Height = Double.NaN;
                        return;
                    }
                }
            }
            MenuUnSubs.Visibility = Visibility.Hidden;
            MenuUnSubs.Height = Double.NaN;
        }

        private void PreviewDragAndDrop(object sender, MouseButtonEventArgs e)
        {
            var target = ((MouseDevice)e.Device).Target;
            if (sender is ListViewItem && target is TextBlock && (target as TextBlock).Name == "Handle")
            {
                ListViewItem draggedItem = sender as ListViewItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void SetDropAction(object sender, DragEventArgs e)
        {
            Mod droppedData = e.Data.GetData(typeof(Mod)) as Mod;
            Mod target = ((ListViewItem)(sender)).DataContext as Mod;

            int oldIndex = lsView.Items.IndexOf(droppedData);
            int targetIdx = lsView.Items.IndexOf(target);
            if (oldIndex == targetIdx)
            {
                lsView.SelectedItem = target;
            }
            else
            {
                SetNewOrder(droppedData, targetIdx < 0 ? 0 : target.Order);
            }
        }

        #region Environment Functions

        private void AskGamePathIfRequired()
        {
            if (string.IsNullOrEmpty(LoadService.config.GamePath))
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.InitialDirectory = (string)Registry.GetValue(
                    Constants.SteamRegistryKey,
                    "InstallPath",
                    Constants.DefaultSteamDirectory
                ) + "\\steamapps\\common\\Kenshi";
                dialog.Title = Directory.Exists(dialog.InitialDirectory) ? "Is this Kenshi Folder?" : "What is the game folder?";
                CommonFileDialogResult result = dialog.ShowDialog();

                LoadService.config.GamePath = dialog.FileName;
            }
        }

        private void AskSteamPathIfRequired()
        {
            if (LoadService.config.SteamModsPath == "NONE")
            {
                LoadService.config.SteamModsPath = "NONE";
                return;
            }

            if (string.IsNullOrEmpty(LoadService.config.SteamModsPath))
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you using Steam Version?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var dialog = new CommonOpenFileDialog();
                    dialog.IsFolderPicker = true;
                    dialog.Title = Directory.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\233860") ? "Is this Kenshi Mod Folder (STEAM) P.s. 233860 is the id from kenshi ?" : "You need to select Kenshi Steam Folder, it's your steam folder + \"Steam\\steamapps\\workshop\\content\\233860";
                    dialog.InitialDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\233860";
                    CommonFileDialogResult result = dialog.ShowDialog();

                    try
                    {
                        LoadService.config.SteamModsPath = dialog.FileName;
                    }
                    catch (Exception)
                    {
                        LoadService.config.SteamModsPath = "NONE";
                    }
                }
                else
                    LoadService.config.SteamModsPath = "NONE";
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

                SearchList = ModList.Where(c => c.FilePath.Contains(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) || c.DisplayName.Contains(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) || c.Id == txtSearch.Text).OrderBy(m => m.Order).ToArray();

                if (SearchList.Length == 0)
                {
                    lblSearchInfo.Content = "Couldn't find any mod with the name.";
                }
                else
                {
                    lblSearchInfo.Content = $"Found:  {currentIndexSearch + 1}/{SearchList.Length}.";
                }
            }

            UpdateListView();
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

                    lsView.ScrollIntoView(SearchList[currentIndexSearch]);
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

            UpdateListView();
        }

        private void UpdateListView()
        {
            foreach (var mod in ModList)
            {
                mod.Color = SearchList.Any(s => s.UniqueIdentifier == mod.UniqueIdentifier) ? ModColors.SearchColor : "";

                if (mod.AllDependencies.Any())
                {
                    if (mod.Active)
                    {
                        bool HasError = false;
                        foreach (var item in mod.AllDependencies)
                        {
                            if (!ModList.Any(c => c.Active && c.Order < mod.Order && c.FileName.Contains(item, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                HasError = true;
                                break;
                            }
                        }

                        if (HasError)
                        {
                            mod.Color = ModColors.RequisiteNotFoundColor;
                        }
                    }
                }

                if (mod.Conflicts.Count > 0)
                {
                    mod.Color = ModColors.HasConflictsColor;
                }

                if (!Path.HasExtension(mod.FileName))
                {
                    mod.Color = ModColors.SomeErrorWhileReadingMetadataColor;
                }
            }

            this.Dispatcher.Invoke(() =>
            {
                lsView.ItemsSource = new List<object>();
                lsView.ItemsSource = ModList
                    .Filter(ShowRegularMods.IsChecked ?? false, ShowSteamMods.IsChecked ?? false);

                if (SearchList.Length > 0)
                    lsView.ScrollIntoView(SearchList[currentIndexSearch]);

                ListView_SelectionChanged(this, null);
            });
        }

        public void SetNewOrder(Mod current, int New, bool ignoreUpdateList = false)
        {
            if (New < 0) New = 0;
            Dictionary<Guid, Tuple<int, bool>> order = new Dictionary<Guid, Tuple<int, bool>> {
                {current.UniqueIdentifier, new Tuple<int, bool>(New,true)}
            };
            foreach (var item in ModList.Where(q => q.Order != -1 && q.UniqueIdentifier != current.UniqueIdentifier).OrderBy(c => c.Order))
                order.Add(item.UniqueIdentifier, new Tuple<int, bool>(item.Order, false));

            var i = 0;
            foreach (var item in order.Where(q => !q.Value.Item2).OrderBy(c => c.Value.Item1))
            {
                if (i == New) i++;
                order[item.Key] = new Tuple<int, bool>(i, true);
                i++;
            }

            foreach (var item in order.Where(q => q.Value.Item2))
            {
                ModList.FirstOrDefault(m => m.UniqueIdentifier == item.Key).Order = item.Value.Item1;
            }

            var curList = ModList.Where(c => c.Active && c.UniqueIdentifier != current.UniqueIdentifier);
            var max = curList.Count() > 0 ? curList.Max(q => q.Order) : 0;

            if (New > max)
            {
                ModList.FirstOrDefault(m => m.UniqueIdentifier == current.UniqueIdentifier).Order = max + 1;
            }
            if (!ignoreUpdateList)
                UpdateListView();
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

            UpdateListView();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lsView.SelectedItems.Count > 0)
            {
                var list = new List<string>();
                Mod mod = lsView.SelectedItems[0] as Mod;
                FlowDocument document = new FlowDocument();

                Paragraph paragraph = new Paragraph();

                if (mod.Dependencies != null && mod.Dependencies.Count > 0)
                    WriteRequisites("Dependencies: ", mod.Dependencies);
                if (mod.References != null && mod.References.Count > 0)
                    WriteRequisites("References: ", mod.References);

                if (mod.Conflicts.Count > 0)
                {
                    Write("******************   Ordered By Priority   **********************************", "");
                    Write("Conflicts:", "");
                    Write("*****************   Save mod order and click check conflicts again   *******", "");

                    foreach (var key in mod.Conflicts)
                    {
                        paragraph.Inlines.Add($"{DetailIndex[key].Type}:{Environment.NewLine}");
                        paragraph.Inlines.Add($"{DetailIndex[key].Name}:{Environment.NewLine}");
                        paragraph.Inlines.Add($"{DetailIndex[key].PropertyKey}:{Environment.NewLine}");

                        for (int i = 0; i < ConflictIndex[key].ChangeList.Count; i++)
                        {
                            var item = ConflictIndex[key].ChangeList.ElementAt(i);

                            var isRemoved = item.State == "REMOVED";
                            var isOwned = item.State == "OWNED";
                            var priority = i == ConflictIndex[key].ChangeList.Count - 1 && !isRemoved ? " <<<< This Value will be used" : "";
                            var value = isRemoved ? "" : $"- Value: {item.Value}";
                            paragraph.Inlines.Add($"{item.State} {value} - Mod: {item.ModName} {priority} {Environment.NewLine}");
                        }
                    }
                    Write("*****************************************************************************", "");
                }
                Write("Author:", mod.Author);
                Write("Version:", mod.Version);
                WriteUrl("FilePath:", mod.FilePath, true);
                WriteUrl("Url:", mod.Url);
                Write("Description:", mod.Description);

                RtbDetail.Document.Blocks.Clear();

                RtbDetail.Document = document;

                void WriteRequisites(string title, List<string> requisite)
                {
                    if (!requisite.Any()) return;

                    paragraph.Inlines.Add(new Bold(new Run(title)));

                    for (int i = 0; i < requisite.Count; i++)
                    {
                        var req = requisite[i];

                        var color = ModList.Where(c => c.Active && c.Order < mod.Order).Any(c => c.FileName.Contains(req)) ? Brushes.Green : Brushes.Red;
                        if (!mod.Active) color = Brushes.Black;

                        var text = req ?? "";

                        paragraph.Inlines.Add(new Run($"{text}")
                        {
                            Foreground = color
                        });

                        if ((i + 1) < requisite.Count && requisite.Count > 1) paragraph.Inlines.Add($" ,");
                    }
                    paragraph.Inlines.Add($"{Environment.NewLine}");
                }

                void Write(string title, string Value)
                {
                    Value = Value ?? "";
                    paragraph.Inlines.Add(new Bold(new Run(title)));
                    paragraph.Inlines.Add($" {Value}{Environment.NewLine}");
                }

                void WriteUrl(string title, string Value, bool local = false)
                {
                    Value = Value ?? "";
                    paragraph.Inlines.Add(new Bold(new Run(title)));
                    Hyperlink textLink = new Hyperlink(new Run($" {Value}{Environment.NewLine}"));
                    textLink.NavigateUri = new Uri(local ? Path.GetDirectoryName(Value) : Value);
                    textLink.RequestNavigate += TextLink_RequestNavigate;
                    paragraph.Inlines.Add(textLink);
                }

                document.Blocks.Add(paragraph);
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
            if (File.Exists(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg")))
                File.Copy(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), Path.Combine(LoadService.config.GamePath, "data", "mods.cfg.backup"), true);

            File.WriteAllLines(Path.Combine(LoadService.config.GamePath, "data", "mods.cfg"), ModList.Where(m => m.Active).OrderBy(q => q.Order).Select(m => m.FileName));
        }

        private void BtnGameFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.GamePath, () => MessageBox.Show("Game folder not configured correctly."));
        }

        private void GameModFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(System.IO.Path.Combine(LoadService.config.GamePath, "Mods"), () => MessageBox.Show("Game folder not configured correctly."));
        }

        private void BtnSteamFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonService.OpenFolder(LoadService.config.SteamModsPath, () => MessageBox.Show("steam folder not configured correctly."));
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<Mod>();
            list.AddRange(ModList.Select(c => c));

            var mods = RuleService.OrderMods(list);

            ModList = new ObservableCollection<Mod>();

            foreach (var mod in mods.OrderBy(m => m.Order))
                ModList.Add(mod);

            UpdateListView();
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
                if (mod != null)
                {
                    mod.Active = true;
                    mod.Order = i;
                }
            }

            UpdateListView();
        }

        private void BtnLaunchGameClick(object sender, RoutedEventArgs e)
        {
            CommonService.StartGame();
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
            foreach (var mod in lsView.SelectedItems)
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

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PBStatus.Value = e.ProgressPercentage;
        }

        private BackgroundWorker ListBackgroundWorker = null;

        private void ExecuteWorker(DoWorkEventHandler handler)
        {
            if (ListBackgroundWorker != null)
            {
                MessageBox.Show("Hold on buddy! i'm working on your last request.");
                return;
            }
            ListBackgroundWorker = new BackgroundWorker();
            ListBackgroundWorker.WorkerReportsProgress = true;

            ListBackgroundWorker.DoWork += handler;

            ListBackgroundWorker.ProgressChanged += worker_ProgressChanged;
            ListBackgroundWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            ListBackgroundWorker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ListBackgroundWorker = null;
            this.Dispatcher.Invoke(() =>
            {
                PBStatus.Value = 0;
            });
        }

        private void ToggleActive(object sender, RoutedEventArgs e) =>
            UpdateActiveAndOrder((Mod mod) => mod.Active ? 0 : -1, (Mod mod) => mod.setActive(!mod.Active));

        private void DeactiveMods(object sender, RoutedEventArgs e) =>
            UpdateActiveAndOrder((Mod mod) => -1, (Mod mod) => mod.setActive(false));

        private void ActiveMods(object sender, RoutedEventArgs e) =>
            UpdateActiveAndOrder((Mod mod) => -1, (Mod mod) => mod.setActive(true));

        private void UpdateActiveAndOrder(Func<Mod, int> newOrder, Func<Mod, Mod> Change)
        {
            var length = lsView.SelectedItems.Count;
            Mod[] arr = new Mod[length];
            lsView.SelectedItems.CopyTo(arr, 0);

            ExecuteWorker((object sender, DoWorkEventArgs args) =>
            {
                for (int i = 0; i < length; i++)
                {
                    Mod mod = arr[i];

                    Change(mod);
                    SetNewOrder(mod, newOrder(mod), true);
                    (sender as BackgroundWorker).ReportProgress(i.Percent(length));
                }
                UpdateListView();
            });
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

        private void OpenTooling(object sender, RoutedEventArgs e)
        {
            var tooling = new Tooling();
            tooling.Show();
        }

        private void ShowConflicts_check(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Constants.modChangesFileName))
            {
                MessageBox.Show("You need to Click check conflicts, beware, it'll take a while.");
                return;
            }

            var alreadyLoaded = ConflictIndex.Count > 0 && DetailIndex.Count > 0;
            var list = new Task[] {
            Task.Run(() =>
            {
                if (!alreadyLoaded)
                {
                    var content = File.ReadAllText(Constants.modChangesFileName);

                    if (content.Length > 0)
                        ConflictIndex = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<string, ModListChanges>>(content);
                }
            }),

            Task.Run(() =>
            {
                if (!alreadyLoaded)
                {
                    var content = File.ReadAllText(Constants.DetailChangesFileName);

                    if (content.Length > 0)
                        DetailIndex = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<string, DetailChanges>>(content);
                }
            })};

            Task.WaitAll(list);

            if (!alreadyLoaded)
            {
                Parallel.ForEach(ConflictIndex.Keys, (key) =>
                {
                    if (ConflictIndex[key].Mod.Count == 1) return;
                    foreach (var modName in ConflictIndex[key].Mod)
                    {
                        var mod = ModList.FirstOrDefault(c => c.FileName == modName);
                        if (!mod.Conflicts.Any(q => q == key))
                        {
                            mod.Conflicts.Push(key);
                        }
                    }
                });
            }

            UpdateListView();
        }

        private void ShowTypeChanges(object sender, RoutedEventArgs e)
        {
            UpdateListView();
        }

        private void OpenModFolder_Click(object sender, RoutedEventArgs e)
        {
            bool failure = false;
            foreach (var mod in lsView.SelectedItems)
            {
                CommonService.OpenFolder(System.IO.Path.GetDirectoryName(((Mod)mod).FilePath), () => { failure = true; });
            }

            if (failure)
            {
                MessageBox.Show("Game folder not configured correctly.");
            }
        }

        private void CheckConflicts(object sender, RoutedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = string.IsNullOrEmpty(LoadService.config.ConflictAnalyzerPath) ? "Mod Conflict Manager.exe" : LoadService.config.ConflictAnalyzerPath;
                process.StartInfo.Arguments = $"{ Constants.modChangesFileName} {Constants.DetailChangesFileName}";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.Start();

                process.WaitForExit();
            }
            catch (Exception)
            {
                MessageBox.Show("I'm working on this feature... it's complicated :)");
            }
        }

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private void SortHeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lsView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lsView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        public class SortAdorner : Adorner
        {
            private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            {
                this.Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                TranslateTransform transform = new TranslateTransform
                    (
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2
                    );
                drawingContext.PushTransform(transform);

                Geometry geometry = ascGeometry;
                if (this.Direction == ListSortDirection.Descending)
                    geometry = descGeometry;
                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        }

        private void ToolbarLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;

            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void StartFCS(object sender, RoutedEventArgs e)
        {
            CommonService.StartFCS();
        }

        private void UnSubscribe(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in lsView.SelectedItems)
            {
                if (mod.Source == SourceEnum.Steam)
                {
                    File.AppendAllText("unsubscribedMods", $"{mod.Id}{Environment.NewLine}");
                    SteamWorkshop.Unsubscribe(ulong.Parse(mod.Id));
                }
            }
        }

        private void OpenConfiguration(object sender, RoutedEventArgs e)
        {
            var config = new Configuration();
            config.Show();
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Start(Constants.UpdateListUrl);
        }
    }
}