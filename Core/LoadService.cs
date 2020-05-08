using Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Core
{
    public static class LoadService
    {
        public static KenshiToolConfig config { get; set; }

        public static void Setup()
        {
            if (File.Exists("error.log"))
                File.Delete("error.log");
            if (File.Exists("config.json"))
            {
                config = JsonConvert.DeserializeObject<KenshiToolConfig>(File.ReadAllText("config.json"));
            }
            else
            {
                config = new KenshiToolConfig
                {
                    GamePath = "",
                    SteamModsPath = "",
                    SteamPageUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=",
                    NexusPageUrl = "https://www.nexusmods.com/kenshi/search/?gsearch="
                };
            }

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

            //get removed mods.
            //var all = currentMods.Where(c => !list.Any(e => Path.GetFileName(e.Name) == c));
            //list.AddRange(all.Select(n => new Mod { Source = SourceEnum.Other, Name = n, Active = true }));
            return list;
        }

        private static IEnumerable<Mod> LoadFolderMods(List<string> currentMods)
        {
            return LoadMod(currentMods, Path.Combine(LoadService.config.GamePath, "Mods"));
        }

        private static IEnumerable<Mod> LoadSteamMods(List<string> currentMods)
        {
            if (LoadService.config.SteamModsPath == "NONE") return new List<Mod>();

            return LoadMod(currentMods, LoadService.config.SteamModsPath);
        }

        private static IEnumerable<Mod> LoadMod(List<string> currentMods, string path)
        {
            var listInfo = new List<Mod>();

            if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetDirectories(path))
                {
                    var mod = new Mod
                    {
                        FilePath = Directory.GetFiles(item, "*.mod").FirstOrDefault()
                    };

                    try
                    {
                        ReadAndSetInfo(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.FilePath)}.info"), mod);
                    }
                    catch (Exception ex)
                    {

                        File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - Count't load metadata.{Environment.NewLine}");
                        File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - The mod {mod.FilePath} may be corrupted. {Environment.NewLine}");

                        File.AppendAllText(Constants.Errorfile, $"{ex.Message}");
                        File.AppendAllText(Constants.Errorfile, $"{ex.StackTrace}");
                    }

                    Func<string, bool> predicate = f => f == Path.GetFileName(mod.FilePath);
                    mod.Active = currentMods.Any(predicate);
                    Metadata metadata = ModMetadataReader.LoadMetadata(mod.FilePath);

                    if (metadata is null)
                    {
                        File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - Count't load metadata.{Environment.NewLine}");
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

                    listInfo.Add(mod);
                }
            }
            else
            {
                File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - Count't read folder: {path} .{Environment.NewLine}");
                File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - When report this error, first your config, you may delete config.json.{Environment.NewLine}");
            }
            return listInfo;
        }

        private static List<string> getCurrentActiveMods()
        {
            try
            {
                return File.ReadAllLines(Path.Combine(config.GamePath, "data", "mods.cfg")).Select(c => c.Trim()).ToList();
            }
            catch (Exception ex)
            {
                File.AppendAllText(Constants.Errorfile, $"{DateTime.Now} - Count't read mods.cfg, check your config, you may delete as well, the folder will be requested again.{Environment.NewLine}");
                File.AppendAllText(Constants.Errorfile, $"{ex.Message}{Environment.NewLine}");
                File.AppendAllText(Constants.Errorfile, $"{ex.StackTrace}{Environment.NewLine}");
                return new List<string>();
            }
        }

        private static void ReadAndSetInfo(string filename, Mod mod)
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
            else
            {
                mod.Source = SourceEnum.GameFolder;
            }
        }
    }
}