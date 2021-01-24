using Core.Models;
using System;
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

    }
}