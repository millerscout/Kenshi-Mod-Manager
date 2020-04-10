using Core;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadService.Setup();



            //LoadService.config.GamePath

            var listInfo = new List<Mod>();

            var currentMods = LoadService.getCurrentActiveMods();

            foreach (var item in Directory.GetDirectories(LoadService.config.SteamModsPath).Take(1))
            {
                Console.WriteLine(item);

                var mod = new Mod
                {
                    Name = Directory.GetFiles(item, "*.mod").FirstOrDefault()
                };

                Console.WriteLine(Path.GetDirectoryName(mod.Name));
                Console.WriteLine(Path.Combine(Path.GetFullPath(mod.Name), Path.GetFileNameWithoutExtension(mod.Name)));
                Console.WriteLine((Path.Combine(item, $"{Path.GetFileNameWithoutExtension(mod.Name)}")));

                if (File.Exists(Path.Combine(item, $"{Path.GetFileNameWithoutExtension(mod.Name)}.info")))
                {
                    mod.Categories = Read(Path.Combine(item, $"{Path.GetFileNameWithoutExtension(mod.Name)}.info"));
                }
                else if (File.Exists(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.Name)}.info")))
                {
                    mod.Categories = Read(Path.Combine(item, $"_{Path.GetFileNameWithoutExtension(mod.Name)}.info"));
                }

                Func<string, bool> predicate = f => f == Path.GetFileName(mod.Name);
                mod.Active =  currentMods.Any(predicate);
                mod.Order = currentMods.IndexOf(Path.GetFileName(mod.Name));

            }

        }
        public static IEnumerable<string> Read(string File)
        {
            StringBuilder result = new StringBuilder();

            XDocument xdoc = XDocument.Load(File);

            var tags = from lv1 in xdoc.Descendants("ModData")
                       select new
                       {
                           Children = lv1.Descendants("tags")
                       };

            var q = tags.SelectMany(t => t.Children.Descendants("string"));

            return q.Select(q => q.Value);
        }
    }


    public class Mod
    {
        public int Order { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }

    }
}
