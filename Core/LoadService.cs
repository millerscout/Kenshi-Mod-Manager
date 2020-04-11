using Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Core
{
    public static class LoadService
    {
        public static KenshiToolConfig config { get; set; }
        public static void Setup()
        {
            if (File.Exists("config.json"))
            {
                config = JsonConvert.DeserializeObject<KenshiToolConfig>(File.ReadAllText("config.json"));
            }
            else{
                config = new KenshiToolConfig
                {
                    GamePath = "",
                    SteamModsPath = "",
                    SteamPageUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=",
                    NexusPageUrl = "https://www.nexusmods.com/kenshi/search/?gsearch="
                };
            }
            

        }
        public static void SaveConfig(KenshiToolConfig config) {
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
            var listInfo = new List<Mod>();
            foreach (var item in Directory.GetDirectories(Path.Combine(LoadService.config.GamePath, "Mods")))
            {
                Console.WriteLine(item);

                var mod = new Mod
                {
                    Name = Directory.GetFiles(item, "*.mod").FirstOrDefault()
                };
#if DEBUG
                Console.WriteLine(Path.GetDirectoryName(mod.Name));
                Console.WriteLine(Path.Combine(Path.GetFullPath(mod.Name), Path.GetFileNameWithoutExtension(mod.Name)));
                Console.WriteLine((Path.Combine(item, $"{Path.GetFileNameWithoutExtension(mod.Name)}")));
#endif

                ReadAndSetInfo(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.Name)}.info"), mod);

                Func<string, bool> predicate = f => f == Path.GetFileName(mod.Name);
                mod.Active = currentMods.Any(predicate);
                mod.Order = currentMods.IndexOf(Path.GetFileName(mod.Name));
                if (!listInfo.Any(m => m.DisplayName == mod.DisplayName))
                    listInfo.Add(mod);

            }
            return listInfo;
        }

        private static IEnumerable<Mod> LoadSteamMods(List<string> currentMods)
        {
            var listInfo = new List<Mod>();
            foreach (var item in Directory.GetDirectories(LoadService.config.SteamModsPath))
            {
                Console.WriteLine(item);

                var mod = new Mod
                {
                    Name = Directory.GetFiles(item, "*.mod").FirstOrDefault()
                };
#if DEBUG
                Console.WriteLine(Path.GetDirectoryName(mod.Name));
                Console.WriteLine(Path.Combine(Path.GetFullPath(mod.Name), Path.GetFileNameWithoutExtension(mod.Name)));
                Console.WriteLine((Path.Combine(item, $"{Path.GetFileNameWithoutExtension(mod.Name)}")));
#endif

                ReadAndSetInfo(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.Name)}.info"), mod);

                Func<string, bool> predicate = f => f == Path.GetFileName(mod.Name);
                mod.Active = currentMods.Any(predicate);
                mod.Order = currentMods.IndexOf(Path.GetFileName(mod.Name));
                listInfo.Add(mod);

            }
            return listInfo;
        }

        private static List<string> getCurrentActiveMods()
        {
            return File.ReadAllLines(Path.Combine(config.GamePath, "data", "mods.cfg")).Select(c => c.Trim()).ToList();
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
