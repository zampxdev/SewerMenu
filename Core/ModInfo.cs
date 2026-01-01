namespace SewerMenu.Core
{
    /// <summary>
    /// Central location for mod metadata and version information.
    /// </summary>
    public static class ModInfo
    {
        public const string Name = "SewerMenu";
        public const string Version = "1.0.6";
        public const string Author = "zampx";
        public const string DownloadLink = "https://www.nexusmods.com/schedule1/mods/1379";
        
        // Supported game version
        public const string SupportedGameVersion = "0.4.2f9";
        
        // Build info
        public const string BuildDate = "2026-01-01";
        public const string BuildType = "Release";
        
        // Feature flags
        public const bool EnableDebugLogging = true;
        public const bool EnableExperimentalFeatures = false;
        
        /// <summary>
        /// Gets a formatted version string for display.
        /// </summary>
        public static string GetVersionString()
        {
            return $"{Name} v{Version}";
        }
        
        /// <summary>
        /// Gets full mod information for logging.
        /// </summary>
        public static string GetFullInfo()
        {
            return $"{Name} v{Version} by {Author} | Built: {BuildDate} | Game: {SupportedGameVersion}";
        }
    }
}
