using Core.Models;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public static class RuleService
    {
        public static List<Rules> ruleList = new List<Rules>();

        public static string GetLatestVersion()
        {
            try
            {
                LatestReleaseModelGithub latest = $"http://api.github.com/repos/{LoadService.config.MasterlistSource}/releases/latest"
                    .WithHeader("User-Agent", "kmm-tool")
                    .GetAsync().ReceiveJson<LatestReleaseModelGithub>().GetAwaiter().GetResult();

                return latest.TagName;
            }
            catch (Exception ex)
            {
                Logging.WriteError($"Count't verify latest version.");
                Logging.WriteError(ex);

                return "";
            }
        }

        public static List<Rules> GetRules()
        {
            var rules = "";
            if (!File.Exists(Constants.MasterlistFile))
            {
                rules = UpdateMasterFile();
            }
            else
            {
                rules = File.ReadAllText(Constants.MasterlistFile);
            }
            if (string.IsNullOrEmpty(rules)) return BaseRules.Get;

            return JsonConvert.DeserializeObject<List<Rules>>(rules);
        }

        public static string UpdateMasterFile()
        {
            try
            {
                var content = $"https://raw.githubusercontent.com/{LoadService.config.MasterlistSource}/{LoadService.config.MasterlistVersion}/masterlist.json".GetStringAsync().GetAwaiter().GetResult();

                File.WriteAllText(Constants.MasterlistFile, content);
                return content;
            }
            catch (Exception ex)
            {
                Logging.WriteError("Count't update masterlist to latest version.");
                Logging.WriteError(ex);
                return "";
            }
        }

        public static IEnumerable<Mod> OrderMods(IEnumerable<Mod> mods)
        {
            if (ruleList.Count == 0)
                ruleList = GetRules();

            foreach (var rule in ruleList)
            {
                foreach (var orderedMod in rule.Mod)
                {
                    var mod = mods.FirstOrDefault(c => Path.GetFileName(c.FilePath).Contains(orderedMod));
                    if (mod == null) continue;
                    if (Path.GetFileName(mod.FilePath).Contains(orderedMod))
                    {
                        mod.OrderedAutomatically = true;

                        removeModFromOtherList(mod, rule.Order);

                        if (!rule.ModsOrdered.Any(q => q.UniqueIdentifier == mod.UniqueIdentifier))
                            rule.ModsOrdered.Add(mod);
                        continue;
                    }
                }
            }

            foreach (var item in mods.Where(c => !c.OrderedAutomatically))
            {
                ruleList.FirstOrDefault(c => c.Order == 10).ModsOrdered.Add(item);
                Console.WriteLine(Path.GetFileName(item.FilePath));
            }

            var ordered = new List<Mod>();

            foreach (var rule in ruleList)
            {
                var index = rule.InitialRange;

                foreach (var mod in rule.ModsOrdered)
                {
                    mod.Order = index;
                    index++;
                }
                ordered.AddRange(rule.ModsOrdered.OrderBy(o => o.Order));
            }

            var i = 0;
            foreach (var item in ordered.OrderBy(o => o.Order))
            {
                item.Order = i;
                i++;
            }

            foreach (var rule in ruleList)
            {
                rule.ModsOrdered.Clear();
            }

            var required = ordered.Where(c => c.AllDependencies.Any()).ToList();

            foreach (var item in required)
            {
                var oldOrder = item.Order;

                var dependencies = ordered.Where(c =>
                    item.AllDependencies.Any(q => q.IndexOf(c.FileName, StringComparison.CurrentCultureIgnoreCase) >= 0)
                );

                if (!dependencies.Any()) continue;
                if (oldOrder > dependencies.Max(c => c.Order)) continue;

                ordered.Remove(ordered.FirstOrDefault(c => c.UniqueIdentifier == item.UniqueIdentifier));

                ordered.InsertRange(
                    ordered.IndexOf(dependencies.OrderBy(c => c.Order).Last()) + 1
                , new List<Mod> { item });

                for (int ii = 0; ii < ordered.Count; ii++)
                {
                    ordered.ElementAt(ii).Order = ii;
                }
            }
            return ordered.OrderBy(o => ordered);

            void removeModFromOtherList(Mod mod, int order)
            {
                foreach (var rule in ruleList.Where(c => c.Order < order))
                    rule.ModsOrdered.Remove(mod);
            }
        }
    }
}