using MMDHelpers.CSharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Core
{
    public static class Logging
    {
        private static readonly object sync = new object();
        public static void Write(string filename, params string[] texts)
        {
            if (texts.Length == 0) return;
            lock (sync)
            {
                filename
                    .ToCurrentPath()
                    .WriteToFile(new List<string> { string.Join("", texts.Select(c => $"{DateTime.Now} - {c} {Environment.NewLine}")) });

            }
        }

        public static void Write(string filename, IEnumerable<string> texts)
        {
            if (texts.Count() == 0) return;
            lock (sync)
            {
                filename
                    .ToCurrentPath()
                    .WriteToFile(new List<string> { string.Join("", texts.Select(c => $"{DateTime.Now} - {c} {Environment.NewLine}")) });

            }
        }

        public static void Write(string filename, Exception ex)
        {
            lock (sync)
            {
                filename
                    .ToCurrentPath()
                    .WriteToFile(new List<string> { $"{DateTime.Now} -  {ex.Message}.{Environment.NewLine}", $"{ex.StackTrace} {Environment.NewLine}" });
            }

        }


    }
}
