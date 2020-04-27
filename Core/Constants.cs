using System.Collections.Generic;

namespace Core
{
    public static class Constants
    {
        public static List<string> SkippableMods = new List<string>{
                "gamedata.base", "rebirth.mod", "newwworld.mod","dialogue.mod"
            };
        public const string modChangesFileName = "modChanges.json";
        public const string DetailChangesFileName = "detailChanges.json";
    }
}