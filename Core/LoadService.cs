using Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core
{
    public static class LoadService
    {
        public static KenshiToolConfig config { get; set; }

        public static void Setup()
        {
            if (File.Exists(Constants.Errorfile))
                File.Delete(Constants.Errorfile);
            if (File.Exists(Constants.ConfigFile))
            {
                config = JsonConvert.DeserializeObject<KenshiToolConfig>(File.ReadAllText(Constants.ConfigFile));
            }
            else
            {
                config = new KenshiToolConfig
                {
                    GamePath = "",
                    SteamModsPath = "",
                    SteamPageUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=",
                    NexusPageUrl = "https://www.nexusmods.com/kenshi/search/?gsearch=",
                    MasterlistSource = "millerscout/kmm-masterlist",
                    MaxLogFiles = 5
                };
            }

            var version = RuleService.GetLatestVersion();

            if (!string.IsNullOrEmpty(version) && version != config.MasterlistVersion)
            {
                config.MasterlistVersion = version;
                RuleService.UpdateMasterFile();
            }
            if (File.Exists(Constants.Logfile)) File.Delete(Constants.Logfile);
        }

        public static void SaveConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
        }

        public static IEnumerable<Mod> GetListOfMods()
        {
            var list = new List<Mod>();

            var currentMods = LoadService.getCurrentActiveMods();

            list.AddRange(LoadSteamMods(currentMods));
            list.AddRange(LoadFolderMods(currentMods));

            var qtdByType = string.Join(", ",
                list.GroupBy(c => c.Source).Select(q => $"{q.Key} - {q.Count()}")
            );

            var appendLog = new List<string> {
                $"{DateTime.Now} - Loaded: {list.Count()} Mods",
                $"{DateTime.Now} - {qtdByType}",
            };
            appendLog.Add($"{DateTime.Now} - Detailed List:");
            appendLog.AddRange(list.Select(item => $"{DateTime.Now} - {item.Source} - {item.FilePath}"));

            File.AppendAllLines(Path.Combine( Directory.GetCurrentDirectory(),  Constants.Logfile), appendLog);
            return list;
        }

        public static IEnumerable<Mod> LoadFolderMods(List<string> currentMods)
        {
            return LoadMod(currentMods, Path.Combine(LoadService.config.GamePath, "Mods"));
        }

        public static IEnumerable<Mod> LoadSteamMods(List<string> currentMods)
        {
            if (LoadService.config.SteamModsPath == "NONE") return new List<Mod>();

            return LoadMod(currentMods, LoadService.config.SteamModsPath);
        }

        public static bool IsSymbolic(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            if (pathInfo.Attributes.HasFlag(FileAttributes.Archive))
            {
                return new FileInfo(Path.GetDirectoryName(path)).Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            try
            {
                if (Directory.GetFiles(path).Length > 0)
                {
                    /// valid path
                };
            }
            catch (Exception ex)
            {
                DeleteFolder(path);
                return true;
            }

            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path);
        }

        public static void CreateSymbLink(IEnumerable<Tuple<string, string>> symblist)
        {
            var sync = new object();
            var infoLogList = new ConcurrentBag<string>();
            var errorLogList = new ConcurrentBag<string>();
            Parallel.ForEach(symblist, (symb) =>
            {
                try
                {
                    if (Directory.Exists(symb.Item1))
                    {
                        infoLogList.Add($"{DateTime.Now} - Folder already exist, maybe you installed manually the mod on kenshi's folder? if not you may delete.");
                        return;
                    }
                    Murphy.SymbolicLink.SymbolicLink.create(symb.Item2, symb.Item1);
                }
                catch (Exception ex)
                {
                    infoLogList.Add($"{DateTime.Now} - Failed to link folder.");
                    infoLogList.Add($"{DateTime.Now} - Source: {symb.Item2} >> Target: {symb.Item1}.");
                    infoLogList.Add($"{ex.Message}");
                    infoLogList.Add($"{ex.StackTrace}");
                }
            });

            if (infoLogList.Count > 0)
                File.AppendAllLines(Path.Combine( Directory.GetCurrentDirectory(),  Constants.Logfile), infoLogList);
            if (errorLogList.Count > 0)
                File.AppendAllLines(Constants.Errorfile, errorLogList);
        }

        public static void FolderCleanUp(string path)
        {
            if (!Directory.Exists(path)) return;
            foreach (var directory in Directory.GetDirectories(path))
            {
                FolderCleanUp(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    File.AppendAllText(Constants.Logfile, $"{DateTime.Now} - This Directory was deleted, because it was empty: {directory}.{Environment.NewLine}");
                    DeleteFolder(directory);
                }
            }
        }

        public static IEnumerable<Mod> LoadMod(List<string> currentMods, string path)
        {
            var listMods = new List<Mod>();

            if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetDirectories(path))
                {
                    if (IsSymbolic(item)) continue;

                    var modName = Directory.GetFiles(item, "*.mod").FirstOrDefault() ?? item;
                    var mod = new Mod
                    {
                        FilePath = modName
                    };

                    try
                    {
                        ReadAndSetInfo(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.FilePath)}.info"), mod);
                    }
                    catch (Exception ex)
                    {

                        Logging.WriteError("Count't load metadata.", $"The mod {mod.FilePath} may be corrupted.");
                        Logging.WriteError(ex);
                    }

                    Func<string, bool> predicate = f => f == Path.GetFileName(mod.FilePath);
                    mod.Active = currentMods.Any(predicate);

                    Metadata metadata = new Metadata { };
                    try
                    {
                        metadata = ModMetadataReader.LoadMetadata(mod.FilePath);
                    }
                    catch (Exception ex)
                    {

                        metadata = new Metadata { Description = $"This mod couldn't be loaded, maybe is corrupted or a empty folder, check the error.log and the mod folder {item}" };
                        Logging.WriteError($"Check the folder: {item}");
                    }

                    if (metadata is null)
                    {
                        metadata = new Metadata { Description = $"This mod couldn't be loaded, maybe is corrupted or a empty folder, check the error.log  and the mod folder {item}" };
                        Logging.WriteError($"Count't load metadata from path: {item}.");
                    }
                    else
                    {
                        mod.Dependencies = metadata.Dependencies;
                        mod.Description = metadata.Description;
                        mod.References = metadata.Referenced;
                        mod.Version = metadata.Version.ToString();
                        mod.Author = metadata.Author;
                    }
                    mod.Order = currentMods.IndexOf(Path.GetFileName(mod.FilePath));

                    if (File.Exists($"reports/{mod.FileName}"))
                    {
                        mod.TypesChanged = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($"reports/{mod.FileName}"));
                    }

                    listMods.Add(mod);
                }
            }
            else
            {
                Logging.WriteError($"Count't read folder: {path} .");
                Logging.WriteError($"When report this error, you may delete config.json and try again.");
            }
            return listMods;
        }

        public static List<string> getCurrentActiveMods()
        {
            try
            {
                return File.ReadAllLines(Path.Combine(config.GamePath, "data", "mods.cfg")).Select(c => c.Trim()).ToList();
            }
            catch (Exception ex)
            {
                Logging.WriteError("Count't read mods.cfg, check your config, you may delete as well, the folder will be requested again.");
                Logging.WriteError(ex);
                return new List<string>();
            }
        }

        public static void ReadAndSetInfo(string filename, Mod mod)
        {
            if (File.Exists(filename))
            {
                XDocument xdoc = XDocument.Load(filename);

                var tags = from modData in xdoc.Descendants("ModData")
                           select new
                           {
                               Children = modData.Descendants("tags")
                           };

                mod.Categories = tags.SelectMany(t => t.Children.Descendants("string")).Select(q => q.Value);
                mod.Id = xdoc.Descendants("id").FirstOrDefault().Value;
                mod.Source = SourceEnum.Steam;
            }

            mod.Source = mod.FilePath.Contains(LoadService.config.GamePath)
                ? SourceEnum.GameFolder :
                SourceEnum.Steam;
        }
    }
}