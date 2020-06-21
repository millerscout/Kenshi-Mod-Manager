namespace Core
{
    public class KenshiToolConfig
    {
        public string GamePath { get; set; }
        public string ModFolder => System.IO.Path.Combine(LoadService.config.GamePath, "Mods");
        public string SteamModsPath { get; set; }
        public string SteamPageUrl { get; set; }
        public string NexusPageUrl { get; set; }
        public string ConflictAnalyzerPath { get; set; }
        public string MasterlistVersion { get; set; }
        public string MasterlistSource { get; set; }

    }
}