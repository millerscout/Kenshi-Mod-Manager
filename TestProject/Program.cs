using Core;
using Core.Kenshi_Data.Enums;
using Core.Models;
using MMDHelpers.CSharp.PerformanceChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    internal class Program
    {

        public static void Main(string[] args)
        {

            Ruler.StartMeasuring(false);
            Dictionary<string, ModListChanges> ConflictIndex = new Dictionary<string, ModListChanges>();
            Dictionary<string, DetailChanges> DetailIndex = new Dictionary<string, DetailChanges>();
            LoadService.Setup();
            var ModList = new List<Mod>(1000);
            foreach (var mod in LoadService.GetListOfMods())
            {
                //if (mod.FileName.Contains("test"))
                //{
                mod.setActive(true);
                //}
                //else {
                //    mod.setActive(false);
                //}

                ModList.Add(mod);
            }

            if (ModList.Where(c => c.Active).Count() == 0)
            {
                //MessageBox.Show("you should have mods to check conflicts");
                return;
            }
            var current = 0;
            var length = 0;

            //ExecuteWorker((object sender, DoWorkEventArgs args) =>
            //{

            var cm = new ConflictManager();

            var ordered = ModList.Where(c => c.Active).OrderBy(c => c.Order);
            length = ordered.Count() * 2;
            var baseGameData = new GameData();

            foreach (var item in Constants.BaseMods)
            {
                cm.LoadMods(Path.Combine(LoadService.config.GamePath, "data", item), ModMode.BASE, baseGameData);
            }

            foreach (var mod in ordered)
            {
                cm.LoadMods(mod.FilePath, ModMode.ACTIVE, baseGameData);
                current++;
                //(sender as BackgroundWorker).ReportProgress(current.Percent(length));

            }

            baseGameData.resolveAllReferences();

            baseGameData = null;

            //foreach (var mod in ordered)
            //{
            //    Console.WriteLine($"{mod.DisplayName} Loading...");
            //    var gd = new GameData();

            //    cm.LoadMods(mod.FilePath, ModMode.ACTIVE, gd);

            //    //cm.ListOfGameData.Add(gd);
            //    current++;
            //    //(sender as BackgroundWorker).ReportProgress(current.Percent(length));
            //}

            Helpers.UpdateDatabase();
            //cm.LoadChanges();

            //ConflictIndex = Helpers.conflictIndex;
            //DetailIndex = Helpers.DetailIndex;

            //Parallel.ForEach(ConflictIndex.Keys, (key) =>
            //{

            //    if (ConflictIndex[key].Mod.Count == 1) return;
            //    foreach (var modName in ConflictIndex[key].Mod)
            //    {
            //        var mod = ModList.FirstOrDefault(c => c.FileName.GetHashCode() == modName.GetHashCode());
            //        if (mod != null && !mod.Conflicts.Any(q => q == key))
            //        {
            //            mod.Conflicts.Push(key);
            //        }
            //        current++;
            //        //(sender as BackgroundWorker).ReportProgress(current.Percent(length));
            //    }
            //});

            //    UpdateListView();

            //});
            Ruler.StopMeasuring();
            Ruler.Show(true);

        }

        public static void bootingupMods()
        {
            ///Booting Mods.
            ///
            LoadService.Setup();
            var mods = LoadService.GetListOfMods();

            var rules = RuleService.GetRules();

            File.WriteAllText("rules.json", JsonConvert.SerializeObject(rules));
            var orderedList = RuleService.OrderMods(mods);
        }

        public static void conflict()
        {
            var stopwatch = Stopwatch.StartNew();

            var filename = "changes.json";
            var detailsFilename = "detail.json";
            //if (args.Length > 0)
            //{
            //    filename = args[0];
            //    if (args.Length > 1)
            //        detailsFilename = args[1];

            //}
            ///Booting Mods.
            ///
            LoadService.Setup();

            var mods = LoadService.GetListOfMods();

            var orderedList = RuleService.OrderMods(mods);

            var changes = new Dictionary<string, List<Dictionary<string, string>>>();
            var cm = new ConflictManager();

            var ordered = mods.OrderBy(c => c.Order).ToList();
            var baseGameData = new GameData();

            foreach (var item in new string[6]{
                                      "gamedata.base",
                                      "Newwworld.mod",
                                      "Dialogue.mod",
                                      "Vitali.mod",
                                      "Nizu.mod",
                                      "rebirth.mod"
                                    })
            {
                cm.LoadMods(Path.Combine(LoadService.config.GamePath, "data", item), ModMode.BASE, baseGameData);
            }

            foreach (var mod in ordered)
            {
                cm.LoadMods(mod.FilePath, ModMode.ACTIVE, baseGameData);
            }

            baseGameData.resolveAllReferences();

            //cm.LoadBaseChanges(baseGameData);

            baseGameData = null;

            foreach (var mod in ordered)
            {
                Console.WriteLine($"{mod.DisplayName} Loading...");
                var gd = new GameData();

                cm.LoadMods(mod.FilePath, ModMode.ACTIVE, gd);

                //cm.ListOfGameData.Add(gd);
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds / 1000 + " Seconds Elapsed");

            if (!Directory.Exists("reports"))
                Directory.CreateDirectory("reports");

            Console.WriteLine("writing reports");
            var list = new Task[] {
                //Task.Run(() => { File.WriteAllText(filename, JsonConvert.SerializeObject(Helpers.conflictIndex)); }),
                //Task.Run(() => { File.WriteAllText(detailsFilename, JsonConvert.SerializeObject(Helpers.DetailIndex)); })
            };

            Task.WaitAll(list);
        }
    }
}