namespace SewerMenu.Core
{
    public static class ModInfo
    {
        public const string Name = "SewerMenu";
        public const string Version = "1.0.6";
        public const string Author = "zampx";
        public const string DownloadLink = "https://www.nexusmods.com/schedule1/mods/1379";
        
        public const string SupportedGameVersion = "0.4.2f9";
        
        public const string BuildDate = "2026-01-01";
        public const string BuildType = "Release";
        
        public const bool EnableDebugLogging = true;
        public const bool EnableExperimentalFeatures = false;
        
        public static string GetVersionString()
        {
            return $"{Name} v{Version}";
        }
        
        public static string GetFullInfo()
        {
            return $"{Name} v{Version} by {Author} | Built: {BuildDate} | Game: {SupportedGameVersion}";
        }
    }
}
