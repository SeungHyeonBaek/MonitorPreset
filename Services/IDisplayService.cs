using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for display configuration services
    /// </summary>
    public interface IDisplayService
    {
        /// <summary>
        /// Get current monitor configuration
        /// </summary>
        /// <returns>List of current monitor configurations</returns>
        Task<List<MonitorInfo>> GetCurrentMonitorConfigurationAsync();
        
        /// <summary>
        /// Apply monitor configuration from preset
        /// </summary>
        /// <param name="preset">Preset to apply</param>
        /// <returns>Success status</returns>
        Task<bool> ApplyMonitorConfigurationAsync(DisplayPreset preset);
        
        /// <summary>
        /// Validate if a preset can be applied to current system
        /// </summary>
        /// <param name="preset">Preset to validate</param>
        /// <returns>Validation result with error messages if any</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidatePresetAsync(DisplayPreset preset);
    }
}