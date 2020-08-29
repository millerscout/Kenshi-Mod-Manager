using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Core
{
    public static class Logging
    {
        static object sync = new object();
        public static void WriteError(params string[] texts)
        {
            lock (sync)
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), Constants.Errorfile), string.Join("", texts.Select(c => $"{DateTime.Now} - {c} {Environment.NewLine}")));
            }
        }

        public static void WriteError(Exception ex)
        {
            lock (sync)
            {
                File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), Constants.Errorfile), $"{DateTime.Now} -  {ex.Message}.{Environment.NewLine}");
                File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), Constants.Errorfile), $"{ex.StackTrace} {Environment.NewLine}");
            }
        }
    }
}
