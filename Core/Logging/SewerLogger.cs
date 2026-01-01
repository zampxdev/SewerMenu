using System;
using MelonLoader;

namespace SewerMenu.Core.Logging
{
    /// <summary>
    /// Centralized logging wrapper for SewerMenu.
    /// Provides consistent formatting and log level control.
    /// </summary>
    public static class SewerLogger
    {
        private static MelonLogger.Instance _logger;
        
        /// <summary>
        /// Initializes the logger with the mod's MelonLogger instance.
        /// </summary>
        public static void Initialize(MelonLogger.Instance logger)
        {
            _logger = logger;
            Info("SewerLogger initialized");
        }
        
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public static void Info(string message)
        {
            _logger?.Msg(message);
        }
        
        /// <summary>
        /// Logs a formatted informational message.
        /// </summary>
        public static void Info(string format, params object[] args)
        {
            _logger?.Msg(string.Format(format, args));
        }
        
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warning(string message)
        {
            _logger?.Warning(message);
        }
        
        /// <summary>
        /// Logs a formatted warning message.
        /// </summary>
        public static void Warning(string format, params object[] args)
        {
            _logger?.Warning(string.Format(format, args));
        }
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void Error(string message)
        {
            _logger?.Error(message);
        }
        
        /// <summary>
        /// Logs an error with exception details.
        /// </summary>
        public static void Error(string message, Exception ex)
        {
            _logger?.Error($"{message}: {ex.Message}");
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Error(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        public static void Error(string format, params object[] args)
        {
            _logger?.Error(string.Format(format, args));
        }
        
        /// <summary>
        /// Logs a debug message (only if debug logging is enabled).
        /// </summary>
        public static void Debug(string message)
        {
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Msg($"[DEBUG] {message}");
            }
        }
        
        /// <summary>
        /// Logs a formatted debug message.
        /// </summary>
        public static void Debug(string format, params object[] args)
        {
            if (ModInfo.EnableDebugLogging)
            {
                _logger?.Msg($"[DEBUG] {string.Format(format, args)}");
            }
        }
        
        /// <summary>
        /// Logs a feature-specific message.
        /// </summary>
        public static void Feature(string featureName, string message)
        {
            _logger?.Msg($"[{featureName}] {message}");
        }
        
        /// <summary>
        /// Logs a success message with visual indicator.
        /// </summary>
        public static void Success(string message)
        {
            _logger?.Msg($"[OK] {message}");
        }
        
        /// <summary>
        /// Logs when a feature is toggled.
        /// </summary>
        public static void FeatureToggled(string featureName, bool enabled)
        {
            string status = enabled ? "ENABLED" : "DISABLED";
            _logger?.Msg($"[{featureName}] {status}");
        }
    }
}
