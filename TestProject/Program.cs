using Core;
using GuidelineCore;

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

            var orderedList = RuleService.OrderMods(mods);
        }
    }
}