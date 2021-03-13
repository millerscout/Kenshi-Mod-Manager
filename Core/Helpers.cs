using Core.Kenshi_Data.Enums;
using Core.Models;
using MMDHelpers.CSharp.LocalData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Core
{
    public static class Helpers
    {
        public static string SHA256CheckSum(this string filePath)
        {
            using (SHA256 SHA256 = System.Security.Cryptography.SHA256.Create())
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

        static SQLiteParameter[][] List = new SQLiteParameter[500][];
        static int lastIndex = 0;
        public static void AddToList(int id, ItemType type, string key, string name, string empty, string flag2, State state)
        {
            if (lastIndex == 500)
            {
                UpdateDatabase();
            }


            List[lastIndex] = new SQLiteParameter[7] {
                new SQLiteParameter("ModId", id),
                    new SQLiteParameter("Type", type),
                    new SQLiteParameter("Section", key),
                    new SQLiteParameter("Key", name),
                    new SQLiteParameter("OldVal", empty),
                    new SQLiteParameter("NewVal", flag2),
                    new SQLiteParameter("State", state)
            };

            lastIndex++;



        }

        public static void UpdateDatabase()
        {
            var db = new DataService();
            var query = @"INSERT INTO ModChange (
                                            ModId,
                                            Type,
                                            Section,
                                            [Key],
                                            OldVal,
                                            NewVal,
                                            State
                                        )
                                        VALUES (
                                            :ModId,
                                            :Type,
                                            :Section,
                                            :Key,
                                            :OldVal,
                                            :NewVal,
                                            :State
                                        )";

            using (var connection = new SQLiteConnection($"Data Source={DataService.DbLocation}; Version=3;"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var command = connection.CreateCommand())
                {
                    command.Prepare();
                    command.CommandText = query;

                    foreach (var item in List.Take(lastIndex))
                    {
                        command.Parameters.AddRange(item);

                        //if (timeout > 0) command.CommandTimeout = timeout;

                        /*executed += */
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            //db.InsertBatch(, List.Take(lastIndex));
            lastIndex = 0;
        }
    }
}