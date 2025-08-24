namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Represents the current startup status of the application
    /// </summary>
    public class StartupStatus
    {
        /// <summary>
        /// Whether the application is registered to start with Windows
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Whether the application is configured to start minimized to tray
        /// </summary>
        public bool StartMinimized { get; set; }

        /// <summary>
        /// The command line registered in the startup registry
        /// </summary>
        public string? CommandLine { get; set; }

        /// <summary>
        /// Get a user-friendly description of the startup status
        /// </summary>
        /// <returns>Status description</returns>
        public string GetDescription()
        {
            if (!IsEnabled)
            {
                return "Startup is disabled";
            }

            if (StartMinimized)
            {
                return "Startup is enabled (minimized to tray)";
            }

            return "Startup is enabled (normal window)";
        }

        /// <summary>
        /// Check if the startup configuration is valid
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid()
        {
            if (!IsEnabled)
            {
                return true; // Disabled is always valid
            }

            return !string.IsNullOrWhiteSpace(CommandLine);
        }
    }
}