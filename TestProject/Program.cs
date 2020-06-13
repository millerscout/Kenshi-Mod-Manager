using Core;
using Core.Models;
using Newtonsoft.Json;
using System.IO;

namespace TestProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ///Booting Mods.
            ///
            LoadService.Setup();
            var mods = LoadService.GetListOfMods();

            var rules = RuleService.GetRules();

            File.WriteAllText("rules.json", JsonConvert.SerializeObject(rules));
            var orderedList = RuleService.OrderMods(mods);
        }
    }
}