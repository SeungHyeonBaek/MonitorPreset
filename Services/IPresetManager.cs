using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for preset management operations
    /// </summary>
    public interface IPresetManager
    {
        /// <summary>
        /// Save a new preset or update existing one
        /// </summary>
        /// <param name="preset">Preset to save</param>
        /// <param name="overwrite">Whether to overwrite if preset with same name exists</param>
        /// <returns>Task representing the async operation</returns>
        Task SavePresetAsync(DisplayPreset preset, bool overwrite = false);

        /// <summary>
        /// Load a preset by name
        /// </summary>
        /// <param name="presetName">Name of preset to load</param>
        /// <returns>Loaded preset or null if not found</returns>
        Task<DisplayPreset?> LoadPresetAsync(string presetName);

        /// <summary>
        /// Get all available presets
        /// </summary>
        /// <returns>List of all presets</returns>
        Task<List<DisplayPreset>> GetAllPresetsAsync();

        /// <summary>
        /// Get preset names only (for quick listing)
        /// </summary>
        /// <returns>List of preset names</returns>
        Task<List<string>> GetPresetNamesAsync();

        /// <summary>
        /// Delete a preset by name
        /// </summary>
        /// <param name="presetName">Name of preset to delete</param>
        /// <returns>True if preset was deleted, false if not found</returns>
        Task<bool> DeletePresetAsync(string presetName);

        /// <summary>
        /// Rename a preset
        /// </summary>
        /// <param name="oldName">Current name of preset</param>
        /// <param name="newName">New name for preset</param>
        /// <param name="overwrite">Whether to overwrite if preset with new name exists</param>
        /// <returns>True if rename was successful</returns>
        Task<bool> RenamePresetAsync(string oldName, string newName, bool overwrite = false);

        /// <summary>
        /// Check if a preset with given name exists
        /// </summary>
        /// <param name="presetName">Name to check</param>
        /// <returns>True if preset exists</returns>
        Task<bool> PresetExistsAsync(string presetName);

        /// <summary>
        /// Get preset file path for given name
        /// </summary>
        /// <param name="presetName">Preset name</param>
        /// <returns>Full file path</returns>
        string GetPresetFilePath(string presetName);

        /// <summary>
        /// Validate preset name for file system compatibility
        /// </summary>
        /// <param name="presetName">Name to validate</param>
        /// <returns>True if name is valid</returns>
        bool IsValidPresetName(string presetName);

        /// <summary>
        /// Get a preset by its assigned hotkey
        /// </summary>
        /// <param name="modifiers">Hotkey modifiers</param>
        /// <param name="key">Hotkey key</param>
        /// <returns>The preset with the matching hotkey, or null if not found</returns>
        Task<DisplayPreset?> GetPresetByHotkeyAsync(HotkeyModifiers modifiers, Keys key);

    /// <summary>
    /// Export all presets into a single JSON file
    /// </summary>
    Task<int> ExportAllPresetsAsync(string filePath);

    /// <summary>
    /// Import presets from a JSON file
    /// </summary>
    Task<(int imported, int skipped, List<string> errors)> ImportPresetsAsync(string filePath, bool overwrite = false);
    }
}