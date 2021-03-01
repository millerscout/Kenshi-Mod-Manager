using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Core.Models
{
    public struct ModListChanges
    {
        public ModListChanges(List<string> mod, List<GameChange> changeList)
        {
            Mod = mod;
            ChangeList = changeList;
        }

        public List<string> Mod { get; }
        public List<GameChange> ChangeList { get; }
    }
}