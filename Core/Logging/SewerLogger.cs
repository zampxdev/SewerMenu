using System;
using MelonLoader;

namespace SewerMenu.Core.Logging
{
    public static class SewerLogger
    {
        private static MelonLogger.Instance _logger;
        
        public static void Initialize(MelonLogger.Instance logger)
        {
            _logger = logger;
            Info("SewerLogger initialized");
        }
        
        public static void Info(string message)
        {
            _logger?.Msg(message);
        }
        
        public static void Info(string format, params object[] args)
        {
            _logger?.Msg(string.Format(format, args));
        }
        
        public static void Warning(string message)
        {
            _logger?.Warning(message);
        }
        
        public static void Warning(string format, params object[] args)
        {
            _logger?.Warning(string.Format(format, args));
        }
        
        public static void Error(string message)
        {
            _logger?.Error(message);
        }
        
        public static void Error(string message, Exception ex)
        {
            _logger?.Error($"{message}: {ex.Message}");
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Error(ex.StackTrace);
            }
        }
        
        public static void Error(string format, params object[] args)
        {
            _logger?.Error(string.Format(format, args));
        }
        
        public static void Debug(string message)
        {
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Msg($"[DEBUG] {message}");
            }
        }
        
        public static void Debug(string format, params object[] args)
        {
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Msg($"[DEBUG] {string.Format(format, args)}");
            }
        }
        
        public static void Feature(string featureName, string message)
        {
            _logger?.Msg($"[{featureName}] {message}");
        }
        
        public static void Success(string message)
        {
            _logger?.Msg($"[OK] {message}");
        }
        
        public static void FeatureToggled(string featureName, bool enabled)
        {
            string status = enabled ? "ENABLED" : "DISABLED";
            _logger?.Msg($"[{featureName}] {status}");
        }
    }
}
