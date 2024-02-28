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

        public static Dictionary<string, int> category2orderMap = new Dictionary<string, int>();

        public static readonly Dictionary<string, string> category2ruleType = new Dictionary<string, string>{
            {"GUI", "Ui"},
            {"Graphical", "Graphics"},
            {"Races", "RaceEdits"},
            {"Factions", "FactionEdits"},
            {"Buildings", "Buildings"},
            {"Clothing/Armour", "Armor/Weapons"},
            {"Items/Weapons", "Armor/Weapons"},
            {"Gameplay", "Overhauls & Big additions/world changes"},
        };

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
                Logging.Write(Constants.Errorfile, $"Count't verify latest version.");
                Logging.Write(Constants.Errorfile, ex);

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
                Logging.Write(Constants.Errorfile, "Count't update masterlist to latest version.");
                Logging.Write(Constants.Errorfile, ex);
                return "";
            }
        }

        public static IEnumerable<Mod> OrderMods(IEnumerable<Mod> mods)
        {
            if (ruleList.Count == 0)
                ruleList = GetRules();

            category2orderMap = getCategory2OrderMap(ruleList);

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

            // Fine, lots of mods is not set in rule set, there will force order them sequencely.
            // A better order rule for unspecified mods process by their categories
            int PRESET_BIG_ORDER_NUMBER = 999999;

            foreach (var item in mods.Where(c => !c.OrderedAutomatically))
            {
                int expected_order = PRESET_BIG_ORDER_NUMBER;
                foreach (var c in item.Categories)
                {
                    if (category2orderMap.ContainsKey(c))
                    {
                        expected_order = Math.Min(category2orderMap[c], expected_order);
                    }
                }

                if (expected_order != PRESET_BIG_ORDER_NUMBER)
                {
                    ruleList.FirstOrDefault(c => c.Order == expected_order).ModsOrdered.Add(item);
                    item.OrderedAutomatically = true;
                }
                else
                {
                    Logging.Write(Constants.Logfile, String.Format("mod {0} with categories {1} has no target order.", item.DisplayName, item.DisplayCategories));
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

        private static Dictionary<string, int> getCategory2OrderMap(List<Rules> rules)
        {
            var rule2order = new Dictionary<string, int>();
            foreach (var rule in rules)
            {
                string[] categories = rule.Name.Split(',');
                foreach (string category in categories)
                {
                    rule2order[category.Trim()] = rule.Order;
                }
            }

            var category2order = new Dictionary<string, int>();

            foreach (KeyValuePair<string, string> kv in category2ruleType)
            {
                if (!rule2order.ContainsKey(kv.Value))
                {
                    Logging.Write(Constants.Errorfile, String.Format("rule type {0} not found.", kv.Value));
                    continue;
                }

                category2order.Add(kv.Key, rule2order[kv.Value]);
            }

            //category2order.Add("Translation", 20);

            return category2order;
        }
    }
}