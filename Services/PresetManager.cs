using MonitorPresetManager.Models;
using System.Text.RegularExpressions;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Manager for display preset operations
    /// </summary>
    public class PresetManager : IPresetManager
    {
        private readonly IFileStorage _fileStorage;
        private readonly string _presetsDirectory;
        private const string PresetFileExtension = ".json";

        // Invalid filename characters for Windows
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly Regex ValidNameRegex = new Regex(@"^[a-zA-Z0-9\s\-_()[\]{}]+$", RegexOptions.Compiled);

        /// <summary>
        /// Initialize PresetManager with file storage service
        /// </summary>
        /// <param name="fileStorage">File storage service</param>
        public PresetManager(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            
            // Set presets directory in user's AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _presetsDirectory = Path.Combine(appDataPath, "MonitorPresetManager", "Presets");
            
            // Ensure presets directory exists
            _fileStorage.EnsureDirectoryExists(_presetsDirectory);
        }

        /// <summary>
        /// Save a new preset or update existing one
        /// </summary>
        /// <param name="preset">Preset to save</param>
        /// <param name="overwrite">Whether to overwrite if preset with same name exists</param>
        /// <returns>Task representing the async operation</returns>
        public async Task SavePresetAsync(DisplayPreset preset, bool overwrite = false)
        {
            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            if (string.IsNullOrWhiteSpace(preset.Name))
            {
                throw new ArgumentException("Preset name cannot be null or empty", nameof(preset));
            }

            if (!IsValidPresetName(preset.Name))
            {
                throw new ArgumentException($"Invalid preset name: '{preset.Name}'. Name contains invalid characters.", nameof(preset));
            }

            var filePath = GetPresetFilePath(preset.Name);

            // Check if preset already exists
            if (!overwrite && await PresetExistsAsync(preset.Name))
            {
                throw new InvalidOperationException($"Preset '{preset.Name}' already exists. Use overwrite parameter to replace it.");
            }

            try
            {
                // Update metadata
                if (preset.CreatedDate == default)
                {
                    preset.CreatedDate = DateTime.Now;
                }
                preset.ModifiedDate = DateTime.Now;

                // Validate preset data
                ValidatePreset(preset);

                // Save to file
                await _fileStorage.SaveAsync(filePath, preset);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save preset '{preset.Name}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load a preset by name
        /// </summary>
        /// <param name="presetName">Name of preset to load</param>
        /// <returns>Loaded preset or null if not found</returns>
        public async Task<DisplayPreset?> LoadPresetAsync(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                throw new ArgumentException("Preset name cannot be null or empty", nameof(presetName));
            }

            var filePath = GetPresetFilePath(presetName);

            try
            {
                return await _fileStorage.LoadAsync<DisplayPreset>(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load preset '{presetName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all available presets
        /// </summary>
        /// <returns>List of all presets</returns>
        public async Task<List<DisplayPreset>> GetAllPresetsAsync()
        {
            try
            {
                var presets = new List<DisplayPreset>();
                var files = _fileStorage.GetFiles(_presetsDirectory, $"*{PresetFileExtension}");

                foreach (var filePath in files)
                {
                    try
                    {
                        var preset = await _fileStorage.LoadAsync<DisplayPreset>(filePath);
                        if (preset != null)
                        {
                            presets.Add(preset);
                        }
                    }
                    catch
                    {
                        // Skip corrupted files
                        continue;
                    }
                }

                // Sort by name
                return presets.OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get all presets: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get preset names only (for quick listing)
        /// </summary>
        /// <returns>List of preset names</returns>
        public Task<List<string>> GetPresetNamesAsync()
        {
            try
            {
                var names = new List<string>();
                var files = _fileStorage.GetFiles(_presetsDirectory, $"*{PresetFileExtension}");

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        names.Add(fileName);
                    }
                }

                return Task.FromResult(names.OrderBy(n => n).ToList());
            }
            catch (Exception ex)
            {
                return Task.FromException<List<string>>(new InvalidOperationException($"Failed to get preset names: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Delete a preset by name
        /// </summary>
        /// <param name="presetName">Name of preset to delete</param>
        /// <returns>True if preset was deleted, false if not found</returns>
        public async Task<bool> DeletePresetAsync(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                throw new ArgumentException("Preset name cannot be null or empty", nameof(presetName));
            }

            var filePath = GetPresetFilePath(presetName);

            if (!_fileStorage.FileExists(filePath))
            {
                return false;
            }

            try
            {
                await _fileStorage.DeleteAsync(filePath);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete preset '{presetName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Rename a preset
        /// </summary>
        /// <param name="oldName">Current name of preset</param>
        /// <param name="newName">New name for preset</param>
        /// <param name="overwrite">Whether to overwrite if preset with new name exists</param>
        /// <returns>True if rename was successful</returns>
        public async Task<bool> RenamePresetAsync(string oldName, string newName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(oldName))
            {
                throw new ArgumentException("Old preset name cannot be null or empty", nameof(oldName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New preset name cannot be null or empty", nameof(newName));
            }

            if (!IsValidPresetName(newName))
            {
                throw new ArgumentException($"Invalid new preset name: '{newName}'. Name contains invalid characters.", nameof(newName));
            }

            // Check if old preset exists
            if (!await PresetExistsAsync(oldName))
            {
                return false;
            }

            // Check if new name already exists
            if (!overwrite && await PresetExistsAsync(newName))
            {
                throw new InvalidOperationException($"Preset '{newName}' already exists. Use overwrite parameter to replace it.");
            }

            try
            {
                // Load the preset
                var preset = await LoadPresetAsync(oldName);
                if (preset == null)
                {
                    return false;
                }

                // Update the name and save with new name
                preset.Name = newName;
                preset.ModifiedDate = DateTime.Now;
                await SavePresetAsync(preset, overwrite);

                // Delete the old file
                await DeletePresetAsync(oldName);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to rename preset from '{oldName}' to '{newName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if a preset with given name exists
        /// </summary>
        /// <param name="presetName">Name to check</param>
        /// <returns>True if preset exists</returns>
        public Task<bool> PresetExistsAsync(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                return Task.FromResult(false);
            }

            var filePath = GetPresetFilePath(presetName);
            return Task.FromResult(_fileStorage.FileExists(filePath));
        }

        /// <summary>
        /// Get preset file path for given name
        /// </summary>
        /// <param name="presetName">Preset name</param>
        /// <returns>Full file path</returns>
        public string GetPresetFilePath(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                throw new ArgumentException("Preset name cannot be null or empty", nameof(presetName));
            }

            var fileName = presetName + PresetFileExtension;
            return Path.Combine(_presetsDirectory, fileName);
        }

        /// <summary>
        /// Validate preset name for file system compatibility
        /// </summary>
        /// <param name="presetName">Name to validate</param>
        /// <returns>True if name is valid</returns>
        public bool IsValidPresetName(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                return false;
            }

            // Check length (Windows has 255 char limit for filenames)
            if (presetName.Length > 200) // Leave room for extension and path
            {
                return false;
            }

            // Check for invalid characters
            if (presetName.IndexOfAny(InvalidFileNameChars) >= 0)
            {
                return false;
            }

            // Check for reserved names
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            if (reservedNames.Contains(presetName.ToUpperInvariant()))
            {
                return false;
            }

            // Use regex for additional validation (alphanumeric, spaces, common symbols)
            return ValidNameRegex.IsMatch(presetName);
        }

        /// <summary>
        /// Get a preset by its assigned hotkey
        /// </summary>
        /// <param name="modifiers">Hotkey modifiers</param>
        /// <param name="key">Hotkey key</param>
        /// <returns>The preset with the matching hotkey, or null if not found</returns>
        public async Task<DisplayPreset?> GetPresetByHotkeyAsync(HotkeyModifiers modifiers, Keys key)
        {
            var hotkeyString = HotkeyHelper.GetHotkeyString(modifiers, key);

            var allPresets = await GetAllPresetsAsync();

            return allPresets.FirstOrDefault(p => p.HotKey == hotkeyString);
        }

        /// <summary>
        /// Validate preset data before saving
        /// </summary>
        /// <param name="preset">Preset to validate</param>
        private static void ValidatePreset(DisplayPreset preset)
        {
            if (preset.Monitors == null || preset.Monitors.Count == 0)
            {
                throw new ArgumentException("Preset must contain at least one monitor configuration");
            }

            var primaryCount = preset.Monitors.Count(m => m.IsPrimary);
            if (primaryCount != 1)
            {
                throw new ArgumentException($"Preset must have exactly one primary monitor, found {primaryCount}");
            }

            foreach (var monitor in preset.Monitors)
            {
                if (string.IsNullOrWhiteSpace(monitor.DeviceName))
                {
                    throw new ArgumentException("Monitor device name cannot be null or empty");
                }

                if (monitor.Width <= 0 || monitor.Height <= 0)
                {
                    throw new ArgumentException($"Monitor '{monitor.DeviceName}' has invalid resolution: {monitor.Width}x{monitor.Height}");
                }
            }
        }
    }
}