using Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        public static IEnumerable<Mod> OrderBy(this ObservableCollection<Mod> List, EnumOrder order)
        {
            switch (order)
            {
                case EnumOrder.Name:
                    return List.OrderBy(q => q.DisplayName);
                case EnumOrder.Type:
                    return List.OrderBy(q => q.Source);
                default:
                    return List.OrderBy(q => q.Order);
            }
        }
        public static IEnumerable<Mod> Filter(this IEnumerable<Mod> List, bool showRegularMods, bool showSteamMods)
        {

            if (showRegularMods && showSteamMods) return List;

            if (showRegularMods)
                return List.Where(c => c.Source == SourceEnum.GameFolder);

            if (showRegularMods)
                return List.Where(c => c.Source == SourceEnum.Steam);

            return List;

        }

    }
}
