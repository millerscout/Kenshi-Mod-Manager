using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public static class SteamWorkshop
    {
        public static object syncObject = new object();

        public static PublishedFileId_t FID(this ulong id) => new PublishedFileId_t(id);

        public static bool Init()
        {
            if (!File.Exists("steam_appid.txt")) File.WriteAllText("steam_appid.txt", "233860");

            return SteamAPI.Init();
        }

        public static void ShutDown()
        {
            SteamAPI.Shutdown();
        }

        public static IEnumerable<ulong> getAllModIds()
        {
            Init();
            var amount = SteamUGC.GetNumSubscribedItems();
            var ids = new PublishedFileId_t[amount];
            SteamUGC.GetSubscribedItems(ids, amount);

            return ids.Select(t => t.m_PublishedFileId);
        }

        public static void Subscribe(ulong id)
        {
            Init();
            SteamUGC.SubscribeItem(id.FID());
        }

        public static void Unsubscribe(ulong id)
        {
            Init();
            SteamUGC.UnsubscribeItem(id.FID());
        }
    }
}