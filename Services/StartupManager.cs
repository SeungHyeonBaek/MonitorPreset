using Microsoft.Win32;
using System.Reflection;
using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Manager for Windows startup registration using registry
    /// </summary>
    public class StartupManager : IStartupManager
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string ApplicationName = "MonitorPresetManager";
        
        private readonly string _applicationPath;
        private readonly string _applicationName;

        /// <summary>
        /// Initialize StartupManager
        /// </summary>
        public StartupManager()
        {
            // Get the current executable path
            _applicationPath = Assembly.GetExecutingAssembly().Location;
            _applicationName = ApplicationName;
            
            // If running from dotnet, get the actual executable path
            if (_applicationPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var exePath = _applicationPath.Replace(".dll", ".exe");
                if (File.Exists(exePath))
                {
                    _applicationPath = exePath;
                }
                else
                {
                    // Fallback to dotnet command
                    _applicationPath = $"dotnet \"{_applicationPath}\"";
                }
            }
        }

        /// <summary>
        /// Check if the application is registered to start with Windows
        /// </summary>
        /// <returns>True if registered for startup</returns>
        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null)
                {
                    return false;
                }

                var value = key.GetValue(_applicationName);
                return value != null && !string.IsNullOrEmpty(value.ToString());
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Enable the application to start with Windows
        /// </summary>
        /// <param name="startMinimized">Whether to start minimized to tray</param>
        /// <returns>True if registration was successful</returns>
        public bool EnableStartup(bool startMinimized = true)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    return false;
                }

                var commandLine = BuildCommandLine(startMinimized);
                key.SetValue(_applicationName, commandLine);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Disable the application from starting with Windows
        /// </summary>
        /// <returns>True if unregistration was successful</returns>
        public bool DisableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    return true; // Key doesn't exist, so startup is already disabled
                }

                var value = key.GetValue(_applicationName);
                if (value != null)
                {
                    key.DeleteValue(_applicationName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the current startup command line arguments
        /// </summary>
        /// <returns>Command line arguments or null if not registered</returns>
        public string? GetStartupArguments()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null)
                {
                    return null;
                }

                var value = key.GetValue(_applicationName);
                return value?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Update startup settings (enable/disable and arguments)
        /// </summary>
        /// <param name="enabled">Whether startup should be enabled</param>
        /// <param name="startMinimized">Whether to start minimized to tray</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateStartupSettings(bool enabled, bool startMinimized = true)
        {
            if (enabled)
            {
                return EnableStartup(startMinimized);
            }
            else
            {
                return DisableStartup();
            }
        }

        /// <summary>
        /// Build command line for startup registration
        /// </summary>
        /// <param name="startMinimized">Whether to start minimized</param>
        /// <returns>Command line string</returns>
        private string BuildCommandLine(bool startMinimized)
        {
            var commandLine = $"\"{_applicationPath}\"";
            
            if (startMinimized)
            {
                commandLine += " --minimized";
            }
            
            return commandLine;
        }

        /// <summary>
        /// Check if the current startup registration matches the expected settings
        /// </summary>
        /// <param name="startMinimized">Expected minimized setting</param>
        /// <returns>True if settings match</returns>
        public bool IsStartupConfigurationCurrent(bool startMinimized)
        {
            var currentArgs = GetStartupArguments();
            if (currentArgs == null)
            {
                return false;
            }

            var expectedArgs = BuildCommandLine(startMinimized);
            return string.Equals(currentArgs, expectedArgs, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get startup status information
        /// </summary>
        /// <returns>Startup status information</returns>
        public StartupStatus GetStartupStatus()
        {
            var isEnabled = IsStartupEnabled();
            var arguments = GetStartupArguments();
            var startMinimized = arguments?.Contains("--minimized") == true;

            return new StartupStatus
            {
                IsEnabled = isEnabled,
                StartMinimized = startMinimized,
                CommandLine = arguments
            };
        }
    }


}