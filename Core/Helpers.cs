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
        public static ConcurrentDictionary<string, ModListChanges> conflictIndex = new ConcurrentDictionary<string, ModListChanges>();
        public static ConcurrentDictionary<string, DetailChanges> DetailIndex = new ConcurrentDictionary<string, DetailChanges>();
        public static Dictionary<string, List<ItemType>> listOfTags = new Dictionary<string, List<ItemType>>();

        public static void AddToList(string key, ItemType type, string name, GameChange change)
        {
            Func<List<GameChange>> ObjectC = () => new List<GameChange>() { change };

            var hash = new Random($"{type}{name}{key}".GetHashCode()).Next().ToString();

            lock (sync)
            {
                if (listOfTags.ContainsKey(change.ModName))
                {
                    if (!listOfTags[change.ModName].Any(c => c == type))
                        listOfTags[change.ModName].Add(type);
                }
                else
                {
                    listOfTags.Add(change.ModName, new List<ItemType> { type });
                }
            }

            conflictIndex.AddOrUpdate(hash,
              addValue: new ModListChanges { Mod = new ConcurrentStack<string>(new List<string> { change.ModName }), ChangeList = new ConcurrentStack<GameChange>(new ConcurrentStack<GameChange>(ObjectC())) },
              updateValueFactory: (val, value) =>
              {
                  var current = conflictIndex.GetOrAdd(hash, value);

                  if (!current.Mod.Any(q => q == change.ModName))
                      current.Mod.Push(change.ModName);

                  current.ChangeList.Push(change);
                  return current;
              });

            DetailIndex.AddOrUpdate(hash,
              addValue: new DetailChanges() { Name = name, PropertyKey = key, Type = type.ToString() },
              updateValueFactory: (val, value) =>
              {
                  return DetailIndex.GetOrAdd(hash, value);
              });
        }

    }
}