using System.Text;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// File-based logger implementation
    /// </summary>
    public class FileLogger : ILogger, IDisposable
    {
        private readonly string _logDirectory;
        private readonly string _logFileName;
        private readonly object _lockObject = new object();
        private bool _disposed;

        /// <summary>
        /// Initialize file logger
        /// </summary>
        /// <param name="logDirectory">Directory to store log files (optional)</param>
        public FileLogger(string? logDirectory = null)
        {
            // Default to application data directory
            _logDirectory = logDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MonitorPresetManager",
                "Logs");

            // Create log directory if it doesn't exist
            Directory.CreateDirectory(_logDirectory);

            // Use date-based log file names
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            _logFileName = $"MonitorPresetManager_{today}.log";

            // Log startup
            LogInfo("Logger initialized", "System");
        }

        /// <summary>
        /// Log an information message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        public void LogInfo(string message, string? category = null)
        {
            WriteLogEntry("INFO", message, category);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        public void LogWarning(string message, string? category = null)
        {
            WriteLogEntry("WARN", message, category);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Optional exception details</param>
        /// <param name="category">Optional category for the log entry</param>
        public void LogError(string message, Exception? exception = null, string? category = null)
        {
            var fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\nException: {exception.GetType().Name}: {exception.Message}";
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    fullMessage += $"\nStack Trace: {exception.StackTrace}";
                }
                
                // Include inner exception if present
                if (exception.InnerException != null)
                {
                    fullMessage += $"\nInner Exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
                }
            }
            
            WriteLogEntry("ERROR", fullMessage, category);
        }

        /// <summary>
        /// Log a debug message (only in debug builds)
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category for the log entry</param>
        public void LogDebug(string message, string? category = null)
        {
#if DEBUG
            WriteLogEntry("DEBUG", message, category);
#endif
        }

        /// <summary>
        /// Clear old log files to prevent disk space issues
        /// </summary>
        /// <param name="maxAgeInDays">Maximum age of log files to keep</param>
        public void CleanupOldLogs(int maxAgeInDays = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-maxAgeInDays);
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                        LogInfo($"Deleted old log file: {Path.GetFileName(logFile)}", "Cleanup");
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't throw exceptions from cleanup - just log if possible
                try
                {
                    WriteLogEntry("ERROR", $"Failed to cleanup old logs: {ex.Message}", "Cleanup");
                }
                catch
                {
                    // If we can't even log the cleanup error, just ignore it
                }
            }
        }

        /// <summary>
        /// Get the current log file path
        /// </summary>
        /// <returns>Path to the current log file</returns>
        public string GetLogFilePath()
        {
            return Path.Combine(_logDirectory, _logFileName);
        }

        /// <summary>
        /// Write a log entry to the file
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Message to log</param>
        /// <param name="category">Optional category</param>
        private void WriteLogEntry(string level, string message, string? category)
        {
            if (_disposed)
                return;

            try
            {
                lock (_lockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var categoryText = string.IsNullOrEmpty(category) ? "" : $" [{category}]";
                    var logEntry = $"[{timestamp}] {level}{categoryText}: {message}";

                    var logFilePath = GetLogFilePath();
                    
                    // Append to log file
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Don't throw exceptions from logging - this could cause infinite loops
                // In a production system, you might want to fall back to event log or console
            }
        }

        /// <summary>
        /// Get log statistics
        /// </summary>
        /// <returns>Log statistics information</returns>
        public LogStatistics GetLogStatistics()
        {
            try
            {
                var logFilePath = GetLogFilePath();
                var stats = new LogStatistics();

                if (File.Exists(logFilePath))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    stats.CurrentLogFileSize = fileInfo.Length;
                    stats.CurrentLogFileCreated = fileInfo.CreationTime;
                    
                    // Count lines for different log levels
                    var lines = File.ReadAllLines(logFilePath);
                    stats.TotalEntries = lines.Length;
                    stats.ErrorCount = lines.Count(line => line.Contains(" ERROR"));
                    stats.WarningCount = lines.Count(line => line.Contains(" WARN"));
                    stats.InfoCount = lines.Count(line => line.Contains(" INFO"));
                    stats.DebugCount = lines.Count(line => line.Contains(" DEBUG"));
                }

                // Count total log files
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                stats.TotalLogFiles = logFiles.Length;

                return stats;
            }
            catch
            {
                return new LogStatistics(); // Return empty stats if we can't read the file
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                LogInfo("Logger shutting down", "System");
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Log statistics information
    /// </summary>
    public class LogStatistics
    {
        /// <summary>
        /// Size of current log file in bytes
        /// </summary>
        public long CurrentLogFileSize { get; set; }

        /// <summary>
        /// When the current log file was created
        /// </summary>
        public DateTime CurrentLogFileCreated { get; set; }

        /// <summary>
        /// Total number of log entries in current file
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// Number of error entries
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Number of warning entries
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Number of info entries
        /// </summary>
        public int InfoCount { get; set; }

        /// <summary>
        /// Number of debug entries
        /// </summary>
        public int DebugCount { get; set; }

        /// <summary>
        /// Total number of log files
        /// </summary>
        public int TotalLogFiles { get; set; }

        /// <summary>
        /// Get a summary string of the statistics
        /// </summary>
        /// <returns>Summary string</returns>
        public string GetSummary()
        {
            return $"Entries: {TotalEntries} (Errors: {ErrorCount}, Warnings: {WarningCount}, Info: {InfoCount}, Debug: {DebugCount}), " +
                   $"File Size: {CurrentLogFileSize / 1024.0:F1} KB, Files: {TotalLogFiles}";
        }
    }
}