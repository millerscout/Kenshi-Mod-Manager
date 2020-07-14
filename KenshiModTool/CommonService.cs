using Core;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace KenshiModTool
{
    public static class CommonService
    {
        internal static void StartGame()
        {
            ProcessStartInfo psi;
            if (LoadService.config.SteamModsPath != "NONE")
            {
                psi = new ProcessStartInfo
                {
                    FileName = "steam://rungameid/233860",
                    UseShellExecute = true
                };
            }
            else
            {
                psi = new ProcessStartInfo
                {
                    FileName = Path.Combine(LoadService.config.GamePath, "kenshi_x64.exe"),
                    WorkingDirectory = LoadService.config.GamePath

                };
            }
            Process.Start(psi);
        }

        internal static void StartFCS()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Path.Combine(LoadService.config.GamePath, "forgotten construction set.exe"),
                WorkingDirectory = LoadService.config.GamePath

            };
            Process.Start(psi);
        }
        internal static void OpenFolder(string path, Action action)
        {
            if (Directory.Exists(path))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                action();
            }

        }

    }
}
