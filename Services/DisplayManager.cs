using System.Runtime.InteropServices;
using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Manager class for display configuration using Windows API
    /// </summary>
    public class DisplayManager : IDisplayService
    {
        #region Win32 API Declarations

        /// <summary>
        /// Retrieves information about the display devices in the current session
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(
            string? lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        /// <summary>
        /// Retrieves information about one of the graphics modes for a display device
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(
            string? deviceName,
            int modeNum,
            ref DEVMODE devMode);

        /// <summary>
        /// Changes the settings of the default display device to the specified graphics mode
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(
            ref DEVMODE devMode,
            uint flags);

        /// <summary>
        /// Changes the settings of the specified display device to the specified graphics mode
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettingsEx(
            string? lpszDeviceName,
            ref DEVMODE lpDevMode,
            IntPtr hwnd,
            uint dwflags,
            IntPtr lParam);

        /// <summary>
        /// Refreshes the desktop by broadcasting a settings change message
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            IntPtr pvParam,
            uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x0014;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        #endregion

        #region Win32 Structures

        /// <summary>
        /// Contains information about a display device
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        /// <summary>
        /// Contains information about the initialization and environment of a printer or a display device
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion

        #region Constants

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int CDS_TEST = 0x02;
        private const int CDS_NORESET = 0x10000000;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_RESTART = 1;
        private const int DISP_CHANGE_FAILED = -1;
        private const int DISP_CHANGE_BADMODE = -2;
        private const int DISP_CHANGE_NOTUPDATED = -3;
        private const int DISP_CHANGE_BADFLAGS = -4;
        private const int DISP_CHANGE_BADPARAM = -5;
        private const int DISP_CHANGE_BADDUALVIEW = -6;
        private const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
        private const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get human-readable error message for display change result codes
        /// </summary>
        /// <param name="result">Result code from ChangeDisplaySettings</param>
        /// <returns>Human-readable error message</returns>
        private static string GetDisplayChangeErrorMessage(int result)
        {
            return result switch
            {
                DISP_CHANGE_SUCCESSFUL => "Success",
                DISP_CHANGE_RESTART => "Restart required",
                DISP_CHANGE_FAILED => "The display driver failed the specified graphics mode",
                DISP_CHANGE_BADMODE => "The graphics mode is not supported",
                DISP_CHANGE_NOTUPDATED => "Unable to write settings to the registry",
                DISP_CHANGE_BADFLAGS => "An invalid set of flags was passed in",
                DISP_CHANGE_BADPARAM => "An invalid parameter was passed in",
                DISP_CHANGE_BADDUALVIEW => "The settings change was unsuccessful because the system is DualView capable",
                _ => $"Unknown error code: {result}"
            };
        }

        #endregion

        #region IDisplayService Implementation

        /// <summary>
        /// Get current monitor configuration
        /// </summary>
        /// <returns>List of current monitor configurations</returns>
        public async Task<List<MonitorInfo>> GetCurrentMonitorConfigurationAsync()
        {
            return await Task.Run(() => GetCurrentMonitorConfiguration());
        }

        /// <summary>
        /// Apply monitor configuration from preset
        /// </summary>
        /// <param name="preset">Preset to apply</param>
        /// <returns>Success status</returns>
        public async Task<bool> ApplyMonitorConfigurationAsync(DisplayPreset preset)
        {
            return await Task.Run(() => ApplyMonitorConfiguration(preset));
        }

        /// <summary>
        /// Validate if a preset can be applied to current system
        /// </summary>
        /// <param name="preset">Preset to validate</param>
        /// <returns>Validation result with error messages if any</returns>
        public async Task<(bool IsValid, string ErrorMessage)> ValidatePresetAsync(DisplayPreset preset)
        {
            return await Task.Run(() => ValidatePreset(preset));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get current monitor configuration (synchronous)
        /// </summary>
        /// <returns>List of current monitor configurations</returns>
        private List<MonitorInfo> GetCurrentMonitorConfiguration()
        {
            var monitors = new List<MonitorInfo>();
            
            try
            {
                uint deviceIndex = 0;
                DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
                displayDevice.cb = Marshal.SizeOf(displayDevice);

                // Enumerate all display devices
                while (EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0))
                {
                    // Only process devices attached to desktop
                    if ((displayDevice.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) != 0)
                    {
                        var monitor = GetMonitorInfo(displayDevice);
                        if (monitor != null)
                        {
                            monitors.Add(monitor);
                        }
                    }

                    deviceIndex++;
                    displayDevice = new DISPLAY_DEVICE();
                    displayDevice.cb = Marshal.SizeOf(displayDevice);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get current monitor configuration: {ex.Message}", ex);
            }

            return monitors;
        }

        /// <summary>
        /// Get monitor information from display device
        /// </summary>
        /// <param name="displayDevice">Display device structure</param>
        /// <returns>Monitor information or null if failed</returns>
        private MonitorInfo? GetMonitorInfo(DISPLAY_DEVICE displayDevice)
        {
            try
            {
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                // Get current display settings
                if (!EnumDisplaySettings(displayDevice.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    return null;
                }

                return new MonitorInfo
                {
                    DeviceName = displayDevice.DeviceName,
                    Width = devMode.dmPelsWidth,
                    Height = devMode.dmPelsHeight,
                    PositionX = devMode.dmPositionX,
                    PositionY = devMode.dmPositionY,
                    IsPrimary = (displayDevice.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0,
                    Orientation = ConvertToDisplayOrientation(devMode.dmDisplayOrientation)
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Convert Windows display orientation to DisplayOrientation enum
        /// </summary>
        /// <param name="windowsOrientation">Windows orientation value</param>
        /// <returns>DisplayOrientation enum value</returns>
        private DisplayOrientation ConvertToDisplayOrientation(int windowsOrientation)
        {
            return windowsOrientation switch
            {
                0 => DisplayOrientation.Landscape,
                1 => DisplayOrientation.Portrait,
                2 => DisplayOrientation.LandscapeFlipped,
                3 => DisplayOrientation.PortraitFlipped,
                _ => DisplayOrientation.Landscape
            };
        }

        /// <summary>
        /// Convert DisplayOrientation enum to Windows display orientation value
        /// </summary>
        /// <param name="orientation">DisplayOrientation enum value</param>
        /// <returns>Windows orientation value</returns>
        private int ConvertFromDisplayOrientation(DisplayOrientation orientation)
        {
            return orientation switch
            {
                DisplayOrientation.Landscape => 0,
                DisplayOrientation.Portrait => 1,
                DisplayOrientation.LandscapeFlipped => 2,
                DisplayOrientation.PortraitFlipped => 3,
                _ => 0
            };
        }

        /// <summary>
        /// Apply monitor configuration from preset (synchronous)
        /// </summary>
        /// <param name="preset">Preset to apply</param>
        /// <returns>Success status</returns>
        private bool ApplyMonitorConfiguration(DisplayPreset preset)
        {
            if (preset?.Monitors == null || preset.Monitors.Count == 0)
            {
                return false;
            }

            // Validate preset before applying
            var validation = ValidatePreset(preset);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Cannot apply preset: {validation.ErrorMessage}");
            }

            // Store current configuration for rollback
            var currentConfig = GetCurrentMonitorConfiguration();

            try
            {
                // Apply configuration for each monitor (non-primary first)
                var nonPrimaryMonitors = preset.Monitors.Where(m => !m.IsPrimary).ToList();
                var primaryMonitor = preset.Monitors.FirstOrDefault(m => m.IsPrimary);

                // Apply non-primary monitors first
                foreach (var monitorConfig in nonPrimaryMonitors)
                {
                    ApplyMonitorSettings(monitorConfig);
                }

                // Apply primary monitor last (if exists)
                if (primaryMonitor != null)
                {
                    ApplyMonitorSettings(primaryMonitor);
                    
                    // Set as primary monitor using a separate call
                    SetPrimaryMonitor(primaryMonitor.DeviceName);
                }

                // Refresh the desktop to ensure changes take effect
                RefreshDesktop();

                return true;
            }
            catch (Exception ex)
            {
                // Rollback on any exception
                try
                {
                    RollbackConfiguration(currentConfig);
                }
                catch
                {
                    // Ignore rollback errors
                }
                
                throw new InvalidOperationException($"Failed to apply monitor configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Apply settings for a single monitor
        /// </summary>
        /// <param name="monitorConfig">Monitor configuration to apply</param>
        /// <returns>Success status</returns>
        private bool ApplyMonitorSettings(MonitorInfo monitorConfig)
        {
            try
            {
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                // Get current settings first
                if (!EnumDisplaySettings(monitorConfig.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    throw new InvalidOperationException($"Failed to get current settings for {monitorConfig.DeviceName}");
                }

                // Update with new settings
                devMode.dmPelsWidth = monitorConfig.Width;
                devMode.dmPelsHeight = monitorConfig.Height;
                devMode.dmPositionX = monitorConfig.PositionX;
                devMode.dmPositionY = monitorConfig.PositionY;
                devMode.dmDisplayOrientation = ConvertFromDisplayOrientation(monitorConfig.Orientation);

                // Set correct fields that we're changing
                const int DM_PELSWIDTH = 0x00080000;
                const int DM_PELSHEIGHT = 0x00100000;
                const int DM_POSITION = 0x00000020;
                const int DM_DISPLAYORIENTATION = 0x00000080;
                
                devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION;

                // Test the change first
                int result = ChangeDisplaySettingsEx(monitorConfig.DeviceName, ref devMode, IntPtr.Zero, CDS_TEST, IntPtr.Zero);
                if (result != DISP_CHANGE_SUCCESSFUL)
                {
                    string errorMsg = GetDisplayChangeErrorMessage(result);
                    throw new InvalidOperationException($"Display settings test failed for {monitorConfig.DeviceName}: {errorMsg}");
                }

                // Apply the change
                result = ChangeDisplaySettingsEx(monitorConfig.DeviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                if (result != DISP_CHANGE_SUCCESSFUL)
                {
                    string errorMsg = GetDisplayChangeErrorMessage(result);
                    throw new InvalidOperationException($"Display settings apply failed for {monitorConfig.DeviceName}: {errorMsg}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply settings for monitor {monitorConfig.DeviceName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Rollback to previous monitor configuration
        /// </summary>
        /// <param name="previousConfig">Previous configuration to restore</param>
        private void RollbackConfiguration(List<MonitorInfo> previousConfig)
        {
            foreach (var monitorConfig in previousConfig)
            {
                try
                {
                    ApplyMonitorSettings(monitorConfig);
                }
                catch
                {
                    // Ignore individual rollback failures
                }
            }
        }

        /// <summary>
        /// Validate if a preset can be applied to current system (synchronous)
        /// </summary>
        /// <param name="preset">Preset to validate</param>
        /// <returns>Validation result with error messages if any</returns>
        private (bool IsValid, string ErrorMessage) ValidatePreset(DisplayPreset preset)
        {
            if (preset == null)
            {
                return (false, "Preset is null");
            }

            if (preset.Monitors == null || preset.Monitors.Count == 0)
            {
                return (false, "Preset contains no monitor configurations");
            }

            try
            {
                // Get current system monitors
                var currentMonitors = GetCurrentMonitorConfiguration();
                var currentDeviceNames = currentMonitors.Select(m => m.DeviceName).ToHashSet();

                // Check if all preset monitors exist in current system
                foreach (var presetMonitor in preset.Monitors)
                {
                    if (string.IsNullOrWhiteSpace(presetMonitor.DeviceName))
                    {
                        return (false, "Monitor configuration contains empty device name");
                    }

                    if (!currentDeviceNames.Contains(presetMonitor.DeviceName))
                    {
                        return (false, $"Monitor '{presetMonitor.DeviceName}' not found in current system");
                    }

                    // Validate resolution
                    if (presetMonitor.Width <= 0 || presetMonitor.Height <= 0)
                    {
                        return (false, $"Invalid resolution for monitor '{presetMonitor.DeviceName}': {presetMonitor.Width}x{presetMonitor.Height}");
                    }

                    // Validate orientation
                    if (!Enum.IsDefined(typeof(DisplayOrientation), presetMonitor.Orientation))
                    {
                        return (false, $"Invalid orientation for monitor '{presetMonitor.DeviceName}': {presetMonitor.Orientation}");
                    }

                    // Test if the configuration can be applied
                    if (!TestMonitorConfiguration(presetMonitor))
                    {
                        return (false, $"Configuration for monitor '{presetMonitor.DeviceName}' is not supported by the hardware");
                    }
                }

                // Validate that exactly one monitor is marked as primary
                var primaryMonitors = preset.Monitors.Where(m => m.IsPrimary).ToList();
                if (primaryMonitors.Count != 1)
                {
                    return (false, $"Preset must have exactly one primary monitor, found {primaryMonitors.Count}");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test if a monitor configuration is supported by the hardware
        /// </summary>
        /// <param name="monitorConfig">Monitor configuration to test</param>
        /// <returns>True if configuration is supported</returns>
        private bool TestMonitorConfiguration(MonitorInfo monitorConfig)
        {
            try
            {
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                // Get current settings first
                if (!EnumDisplaySettings(monitorConfig.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    return false;
                }

                // Update with new settings
                devMode.dmPelsWidth = monitorConfig.Width;
                devMode.dmPelsHeight = monitorConfig.Height;
                devMode.dmPositionX = monitorConfig.PositionX;
                devMode.dmPositionY = monitorConfig.PositionY;
                devMode.dmDisplayOrientation = ConvertFromDisplayOrientation(monitorConfig.Orientation);

                // Set correct fields that we're changing
                const int DM_PELSWIDTH = 0x00080000;
                const int DM_PELSHEIGHT = 0x00100000;
                const int DM_POSITION = 0x00000020;
                const int DM_DISPLAYORIENTATION = 0x00000080;
                
                devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION;

                // Test the change without applying it
                int result = ChangeDisplaySettingsEx(monitorConfig.DeviceName, ref devMode, IntPtr.Zero, CDS_TEST, IntPtr.Zero);
                return result == DISP_CHANGE_SUCCESSFUL;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set the specified monitor as the primary monitor
        /// </summary>
        /// <param name="deviceName">Device name of the monitor to set as primary</param>
        private void SetPrimaryMonitor(string deviceName)
        {
            try
            {
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                // Get current settings
                if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    // Set position to (0,0) for primary monitor
                    devMode.dmPositionX = 0;
                    devMode.dmPositionY = 0;
                    
                    const int DM_POSITION = 0x00000020;
                    devMode.dmFields = DM_POSITION;

                    // Apply the primary monitor setting
                    ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY | CDS_NORESET, IntPtr.Zero);
                }
            }
            catch
            {
                // Ignore errors in primary monitor setting
            }
        }

        /// <summary>
        /// Refresh the desktop to ensure display changes take effect
        /// </summary>
        private void RefreshDesktop()
        {
            try
            {
                // Apply all pending changes
                ChangeDisplaySettings(IntPtr.Zero, 0);
                
                // Refresh desktop wallpaper to force a redraw
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            catch
            {
                // Ignore refresh errors
            }
        }

        /// <summary>
        /// Overload for ChangeDisplaySettings without DEVMODE parameter
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(IntPtr lpDevMode, uint dwFlags);

        #endregion
    }
}