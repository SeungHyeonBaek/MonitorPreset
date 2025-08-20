namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Class representing application settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>Whether to auto-start with Windows</summary>
        public bool AutoStart { get; set; } = false;
        
        /// <summary>Whether to minimize to tray when closing window</summary>
        public bool MinimizeToTray { get; set; } = true;
        
        /// <summary>Whether to show notifications</summary>
        public bool ShowNotifications { get; set; } = true;
        
        /// <summary>Name of the last used preset</summary>
        public string LastUsedPreset { get; set; } = string.Empty;
        
        /// <summary>Application version</summary>
        public string Version { get; set; } = "1.0.0";
        
        /// <summary>
        /// Reset to default settings
        /// </summary>
        public void ResetToDefaults()
        {
            AutoStart = false;
            MinimizeToTray = true;
            ShowNotifications = true;
            LastUsedPreset = string.Empty;
            Version = "1.0.0";
        }
        
        /// <summary>
        /// Validate if the settings are valid
        /// </summary>
        /// <returns>Whether the settings are valid</returns>
        public bool IsValid()
        {
            // Check if version information exists
            if (string.IsNullOrWhiteSpace(Version)) return false;
            
            // Check if basic settings are within valid ranges
            return true;
        }
        
        /// <summary>
        /// Create a copy of the settings
        /// </summary>
        /// <returns>Cloned settings</returns>
        public AppSettings Clone()
        {
            return new AppSettings
            {
                AutoStart = AutoStart,
                MinimizeToTray = MinimizeToTray,
                ShowNotifications = ShowNotifications,
                LastUsedPreset = LastUsedPreset,
                Version = Version
            };
        }
    }
}