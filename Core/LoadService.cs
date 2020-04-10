using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        }

        public static List<string> getCurrentActiveMods()
        {
            return File.ReadAllLines(Path.Combine(config.GamePath, "data", "mods.cfg")).Select(c => c.Trim()).ToList();
        }
    }
}
