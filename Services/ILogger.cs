namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for logging services
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Log an information message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        void LogInfo(string message, string? category = null);

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        void LogWarning(string message, string? category = null);

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Optional exception details</param>
        /// <param name="category">Optional category for the log entry</param>
        void LogError(string message, Exception? exception = null, string? category = null);

        /// <summary>
        /// Log a debug message (only in debug builds)
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        void LogDebug(string message, string? category = null);

        /// <summary>
        /// Clear old log files to prevent disk space issues
        /// </summary>
        /// <param name="maxAgeInDays">Maximum age of log files to keep</param>
        void CleanupOldLogs(int maxAgeInDays = 30);

        /// <summary>
        /// Get the current log file path
        /// </summary>
        /// <returns>Path to the current log file</returns>
        string GetLogFilePath();
    }
}