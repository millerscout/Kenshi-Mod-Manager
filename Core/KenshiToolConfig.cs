namespace Core
{
    public class KenshiToolConfig
    {
        public string GamePath { get; set; }
        public string ModFolder => System.IO.Path.Combine(LoadService.config.GamePath, "Mods");
        public string SteamModsPath { get; set; }
        public string SteamPageUrl { get; set; }
        public string NexusPageUrl { get; set; }
        public string MasterlistVersion { get; set; }
        public string MasterlistSource { get; set; }
        public int MaxLogFiles { get; set; } = 5;
        public bool CheckForUpdatesAutomatically { get; set; } = true;
    }
}