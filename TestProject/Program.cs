using Core;
using Core.Kenshi_Data.Enums;
using Newtonsoft.Json;
using System;
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
            //bootingupMods();
            conflict();

            var t = ModMetadataReader.LoadMetadata(@"C:\Program Files (x86)\Steam\steamapps\common\Kenshi\Mods\AidKitsMoreCharges\AidKitsMoreCharges.mod");

            var readStream = new FileStream(@"C:\Program Files (x86)\Steam\steamapps\common\Kenshi\Mods\AidKitsMoreCharges\AidKitsMoreCharges.mod", FileMode.Open);
            BinaryReader file = new BinaryReader(readStream);
            int num1 = file.ReadInt32();

            for (int index = 0; index < num1; ++index)
            {
                file.ReadInt32();
                //itemType type = (itemType)file.ReadInt32();
                int num2 = file.ReadInt32();
                string name = readString(file);
                Console.WriteLine(name);
                Console.WriteLine("---------------");
                //string str = fileVersion >= 7 ? GameData.readString(file) : num2.ToString() + "-" + fileName;

                //GameData.Item obj = getItem(str);

                //bool newItem = obj == null;
                //if (obj == null)
                //{
                //    obj = new GameData.Item(type, str);
                //    this.items.Add(str, obj);
                //}
                //if (obj.type != type)
                //    Console.WriteLine("err");
                //int num3 = obj.load(file, name, mode, fileVersion, fileName, newItem) ? 1 : 0;
                //if (obj.getState() == State.REMOVED)
                //{
                //    obj.refreshState();
                //    if (mode == ModMode.BASE || obj.getState() == State.OWNED && !false)
                //        this.items.Remove(obj.stringID);
                //    else
                //        obj.flagDeleted();
                //}
                //if (num3 == 0 & skipMissing) { }
                //this.items.Remove(obj.stringID);
            }
            string readString(BinaryReader file)
            {
                int count = file.ReadInt32();

                if (count <= 0)
                    return string.Empty;
                var arr = new byte[count];
                file.Read(arr, 0, count);
                return Encoding.UTF8.GetString(arr, 0, count);
            }
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

            cm.LoadBaseChanges(baseGameData);

            baseGameData = null;

            foreach (var mod in ordered)
            {
                Console.WriteLine($"{mod.DisplayName} Loading...");
                var gd = new GameData();

                cm.LoadMods(mod.FilePath, ModMode.ACTIVE, gd);

                cm.ListOfGameData.Add(gd);
            }

            cm.LoadChanges();

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds / 1000 + " Seconds Elapsed");

            if (!Directory.Exists("reports"))
                Directory.CreateDirectory("reports");

            Console.WriteLine("writing reports");
            var list = new Task[] {
                Task.Run(() => { File.WriteAllText(filename, JsonConvert.SerializeObject(cm.conflictIndex)); }),
                Task.Run(() => { File.WriteAllText(detailsFilename, JsonConvert.SerializeObject(cm.DetailIndex)); }),
                Task.Run(() => {
                    foreach (var item in cm.listOfTags)
                    {
                        File.WriteAllText($"reports/{item.Key}", JsonConvert.SerializeObject(item.Value.Select(c=>c.ToString())));
                    }
                })
            };

            Task.WaitAll(list);
        }
    }
}