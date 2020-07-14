using System;
using System.Collections.Generic;
using System.Text;

namespace KenshiModManDotNet
{
    public class Mod
    {
        public bool Enabled { get; set; }
        public int Order { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }
        public string Categories { get; set; }

        public override string ToString()
        {
            return this.Name.ToString();
        }

        public static List<Mod> GetDefaultMods(){

            List<Mod> mods = new List<Mod>();

            mods.Add(new Mod { Enabled = false, Name = "Test Mod 1", Categories = "Buildings, Faction, Armour", Order = -1, Source = "User" });
            mods.Add(new Mod { Enabled = true,  Name = "Test Mod 2", Categories = "NPC, Buildings", Order = 0, Source = "Steam" });
            mods.Add(new Mod { Enabled = true, Name = "Test Mod 3", Categories = "Buildings, Faction, Armour", Order = 1, Source = "Steam" });
            mods.Add(new Mod { Enabled = false, Name = "Test Mod 4", Categories = "Armour", Order = -1, Source = "User" });
            mods.Add(new Mod { Enabled = true, Name = "Test Mod 5", Categories = "NPC, Faction, Armour", Order = 2, Source = "Steam" });

            return mods;
        }
    }
}
