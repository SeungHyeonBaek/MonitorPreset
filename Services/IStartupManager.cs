using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for Windows startup management
    /// </summary>
    public interface IStartupManager
    {
        /// <summary>
        /// Check if the application is registered to start with Windows
        /// </summary>
        /// <returns>True if registered for startup</returns>
        bool IsStartupEnabled();

        /// <summary>
        /// Enable the application to start with Windows
        /// </summary>
        /// <param name="startMinimized">Whether to start minimized to tray</param>
        /// <returns>True if registration was successful</returns>
        bool EnableStartup(bool startMinimized = true);

        /// <summary>
        /// Disable the application from starting with Windows
        /// </summary>
        /// <returns>True if unregistration was successful</returns>
        bool DisableStartup();

        /// <summary>
        /// Get the current startup command line arguments
        /// </summary>
        /// <returns>Command line arguments or null if not registered</returns>
        string? GetStartupArguments();

        /// <summary>
        /// Update startup settings (enable/disable and arguments)
        /// </summary>
        /// <param name="enabled">Whether startup should be enabled</param>
        /// <param name="startMinimized">Whether to start minimized to tray</param>
        /// <returns>True if update was successful</returns>
        bool UpdateStartupSettings(bool enabled, bool startMinimized = true);

        /// <summary>
        /// Check if the current startup registration matches the expected settings
        /// </summary>
        /// <param name="startMinimized">Expected minimized setting</param>
        /// <returns>True if settings match</returns>
        bool IsStartupConfigurationCurrent(bool startMinimized);

        /// <summary>
        /// Get startup status information
        /// </summary>
        /// <returns>Startup status information</returns>
        StartupStatus GetStartupStatus();
    }
}