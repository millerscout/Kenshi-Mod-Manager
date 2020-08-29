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
        public const string Errorfile = "error.log";
        public const string Logfile = "info.log";
        public const string BackupSubscribeList = "bkpsubscribedList";
        public const string ConfigFile = "config.json";
        public const string MasterlistFile = "masterlist.json";
        public const string SteamRegistryKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam";
        public const string DefaultSteamDirectory = "C:\\Program Files (x86)\\Steam";

#if RELEASE
#error Should Not compile if there's no updatelist configured. (DO NOT CHANGE THIS)
        ///the automation is tied to publish using Selfcontained OR Standalone Symbols.
#endif
#if SELFCONTAINED
        public const string UpdateListUrl = "https://raw.githubusercontent.com/millerscout/Kenshi-Mod-Manager/master/updatelist-selfcontained.xml";
#endif
#if STANDALONE
        public const string UpdateListUrl = "https://raw.githubusercontent.com/millerscout/Kenshi-Mod-Manager/master/updatelist-standalone.xml";
#endif
#if DEBUG
        public const string UpdateListUrl = "http://localhost:5000/list.xml";
#endif
    }
}