using Newtonsoft.Json;
using System.Text;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// File storage service for JSON serialization and file operations
    /// </summary>
    public class FileStorage : IFileStorage
    {
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Initialize FileStorage with JSON settings
        /// </summary>
        public FileStorage()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        /// <summary>
        /// Save object to file as JSON
        /// </summary>
        /// <typeparam name="T">Type of object to save</typeparam>
        /// <param name="filePath">Path to save file</param>
        /// <param name="data">Object to save</param>
        /// <returns>Task representing the async operation</returns>
        public async Task SaveAsync<T>(string filePath, T data)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    EnsureDirectoryExists(directory);
                }

                // Serialize to JSON
                var json = JsonConvert.SerializeObject(data, _jsonSettings);

                // Write to file with UTF-8 encoding
                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save file '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load object from JSON file
        /// </summary>
        /// <typeparam name="T">Type of object to load</typeparam>
        /// <param name="filePath">Path to load file from</param>
        /// <returns>Loaded object or default if file doesn't exist</returns>
        public async Task<T?> LoadAsync<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!FileExists(filePath))
            {
                return default(T);
            }

            try
            {
                // Read file content
                var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return default(T);
                }

                // Deserialize from JSON
                return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load file '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if file exists</returns>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete file if it exists
        /// </summary>
        /// <param name="filePath">Path to delete</param>
        /// <returns>Task representing the async operation</returns>
        public async Task DeleteAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            try
            {
                if (FileExists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete file '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all files in directory with specified extension
        /// </summary>
        /// <param name="directoryPath">Directory to search</param>
        /// <param name="extension">File extension (e.g., "*.json")</param>
        /// <returns>Array of file paths</returns>
        public string[] GetFiles(string directoryPath, string extension)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("Extension cannot be null or empty", nameof(extension));
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return Array.Empty<string>();
                }

                return Directory.GetFiles(directoryPath, extension);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get files from directory '{directoryPath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ensure directory exists, create if it doesn't
        /// </summary>
        /// <param name="directoryPath">Directory path to ensure</param>
        public void EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create directory '{directoryPath}': {ex.Message}", ex);
            }
        }
    }
}