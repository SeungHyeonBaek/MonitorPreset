namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for file storage operations
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Save object to file as JSON
        /// </summary>
        /// <typeparam name="T">Type of object to save</typeparam>
        /// <param name="filePath">Path to save file</param>
        /// <param name="data">Object to save</param>
        /// <returns>Task representing the async operation</returns>
        Task SaveAsync<T>(string filePath, T data);

        /// <summary>
        /// Load object from JSON file
        /// </summary>
        /// <typeparam name="T">Type of object to load</typeparam>
        /// <param name="filePath">Path to load file from</param>
        /// <returns>Loaded object or default if file doesn't exist</returns>
        Task<T?> LoadAsync<T>(string filePath);

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if file exists</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Delete file if it exists
        /// </summary>
        /// <param name="filePath">Path to delete</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteAsync(string filePath);

        /// <summary>
        /// Get all files in directory with specified extension
        /// </summary>
        /// <param name="directoryPath">Directory to search</param>
        /// <param name="extension">File extension (e.g., "*.json")</param>
        /// <returns>Array of file paths</returns>
        string[] GetFiles(string directoryPath, string extension);

        /// <summary>
        /// Ensure directory exists, create if it doesn't
        /// </summary>
        /// <param name="directoryPath">Directory path to ensure</param>
        void EnsureDirectoryExists(string directoryPath);
    }
}