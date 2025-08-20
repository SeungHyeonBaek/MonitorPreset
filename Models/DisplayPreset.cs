using System.Text.Json.Serialization;

namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Represents a saved display configuration preset
    /// </summary>
    public class DisplayPreset
    {
        /// <summary>
        /// Name of the preset
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Date when the preset was created
        /// </summary>
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the preset was last modified
        /// </summary>
        [JsonPropertyName("modifiedDate")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Hotkey combination assigned to this preset
        /// </summary>
        [JsonPropertyName("hotkey")]
        public string HotKey { get; set; } = string.Empty;

        /// <summary>
        /// List of monitor configurations in this preset
        /// </summary>
        [JsonPropertyName("monitors")]
        public List<MonitorInfo> Monitors { get; set; } = new List<MonitorInfo>();

        /// <summary>
        /// Description of the preset (optional)
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Check if this preset has a valid configuration
        /// </summary>
        /// <returns>True if preset has at least one monitor</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && Monitors.Count > 0;
        }

        /// <summary>
        /// Get a summary string of the preset configuration
        /// </summary>
        /// <returns>Summary string</returns>
        public string GetSummary()
        {
            if (Monitors.Count == 0)
                return "No monitors configured";

            var primaryMonitor = Monitors.FirstOrDefault(m => m.IsPrimary);
            var totalMonitors = Monitors.Count;
            
            if (primaryMonitor != null)
            {
                return $"{totalMonitors} monitor(s), Primary: {primaryMonitor.Width}x{primaryMonitor.Height}";
            }
            
            return $"{totalMonitors} monitor(s)";
        }

        /// <summary>
        /// Create a copy of this preset with a new name
        /// </summary>
        /// <param name="newName">New name for the copy</param>
        /// <returns>Copy of the preset</returns>
        public DisplayPreset Clone(string newName)
        {
            var clonedMonitors = new List<MonitorInfo>();
            foreach (var monitor in Monitors)
            {
                clonedMonitors.Add(new MonitorInfo
                {
                    DeviceName = monitor.DeviceName,
                    Width = monitor.Width,
                    Height = monitor.Height,
                    PositionX = monitor.PositionX,
                    PositionY = monitor.PositionY,
                    IsPrimary = monitor.IsPrimary,
                    Orientation = monitor.Orientation
                });
            }

            return new DisplayPreset
            {
                Name = newName,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                HotKey = string.Empty, // Don't copy hotkey to avoid conflicts
                Description = Description,
                Monitors = clonedMonitors
            };
        }
    }
}