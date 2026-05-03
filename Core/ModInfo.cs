namespace SewerMenu.Core
{
    public static class ModInfo
    {
        public const string Name = "SewerMenu";
        public const string Version = "2.0.1";
        public const string Author = "zampx";
        public const string DownloadLink = "https://www.nexusmods.com/schedule1/mods/1379";
        
        public const string SupportedGameVersion = "0.4.5f2";
        
        public const string BuildDate = "2026-05-03";
        public const string BuildType = "Release";
        
        public static readonly bool EnableDebugLogging = false;
        public static readonly bool EnableExperimentalFeatures = false;
        
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
