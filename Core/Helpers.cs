using Core.Kenshi_Data.Enums;
using Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Core
{
    public static class Helpers
    {
        public static string SHA256CheckSum(this string filePath)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }

        public static IEnumerable<Mod> Filter(this IEnumerable<Mod> List, bool showRegularMods, bool showSteamMods)
        {
            if (showRegularMods && showSteamMods) return List;

            if (showRegularMods)
                return List.Where(c => c.Source == SourceEnum.GameFolder);

            if (showSteamMods)
                return List.Where(c => c.Source == SourceEnum.Steam);

            return List;
        }

        public static int Percent(this int val, int qty) => val * 100 / qty;

        public static string GetCurrentApplicationPath() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static object sync = new object();
        public static Dictionary<string, ModListChanges> conflictIndex = new Dictionary<string, ModListChanges>();
        public static Dictionary<string, DetailChanges> DetailIndex = new Dictionary<string, DetailChanges>();

        public static void AddToList(string key, ItemType type, string name, GameChange change)
        {
            if (Constants.BaseMods.Contains(change.ModName))
                return;

            var modname = change.ModName;
            var hash = $"{type}{name}{key}".GetHashCode().ToString("x2");

            if (!conflictIndex.ContainsKey(hash))
            {
                conflictIndex.Add(hash, new ModListChanges(new List<string>() { change.ModName }, new List<GameChange>() { change }));
            }
            else
            {
                if (!conflictIndex[hash].Mod.Any(q => q == modname)) conflictIndex[hash].Mod.Add(change.ModName);
                conflictIndex[hash].ChangeList.Add(change);
            }

            if (!DetailIndex.ContainsKey(hash))
                DetailIndex.Add(hash, new DetailChanges(type, name, key));

        }

    }
}