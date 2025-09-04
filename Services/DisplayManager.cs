

using MonitorPresetManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static MonitorPresetManager.Services.NativeMethods;

namespace MonitorPresetManager.Services
{
    public class DisplayManager : IDisplayService
    {
        private readonly ILogger _logger;

        public DisplayManager(ILogger logger)
        {
            _logger = logger;
        }

        private static string GetDisplayChangeErrorMessage(int result)
        {
            return ((DISP_CHANGE)result) switch
            {
                DISP_CHANGE.SUCCESSFUL => "Success",
                DISP_CHANGE.RESTART => "Restart required",
                DISP_CHANGE.FAILED => "The display driver failed the specified graphics mode",
                DISP_CHANGE.BADMODE => "The graphics mode is not supported",
                DISP_CHANGE.NOTUPDATED => "Unable to write settings to the registry",
                DISP_CHANGE.BADFLAGS => "An invalid set of flags was passed in",
                DISP_CHANGE.BADPARAM => "An invalid parameter was passed in",
                DISP_CHANGE.BADDUALVIEW => "The settings change was unsuccessful because the system is DualView capable",
                _ => $"Unknown error code: {result}"
            };
        }

        public async Task<List<MonitorInfo>> GetCurrentMonitorConfigurationAsync()
        {
            return await Task.Run(() => GetCurrentMonitorConfiguration());
        }

        public async Task<bool> ApplyMonitorConfigurationAsync(DisplayPreset preset)
        {
            return await Task.Run(() => ApplyMonitorConfiguration(preset));
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidatePresetAsync(DisplayPreset preset)
        {
            return await Task.Run(() => ValidatePreset(preset));
        }

        private List<MonitorInfo> GetCurrentMonitorConfiguration()
        {
            var monitors = new List<MonitorInfo>();
            
            try
            {
                uint deviceIndex = 0;
                var displayDevice = new DISPLAY_DEVICE();
                displayDevice.cb = Marshal.SizeOf(displayDevice);

                while (EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0))
                {
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

        private MonitorInfo? GetMonitorInfo(DISPLAY_DEVICE displayDevice)
        {
            try
            {
                var devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                if (!EnumDisplaySettings(displayDevice.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    return null;
                }

                var monitorDevice = new DISPLAY_DEVICE();
                monitorDevice.cb = Marshal.SizeOf(monitorDevice);
                if (!EnumDisplayDevices(displayDevice.DeviceName, 0, ref monitorDevice, 0))
                {
                    _logger.LogWarning($"Could not find monitor device for adapter {displayDevice.DeviceName}.");
                }

                return new MonitorInfo
                {
                    DeviceName = displayDevice.DeviceName,
                    PersistentId = monitorDevice.DeviceID, 
                    Width = devMode.dmPelsWidth,
                    Height = devMode.dmPelsHeight,
                    PositionX = devMode.dmPositionX,
                    PositionY = devMode.dmPositionY,
                    IsPrimary = (displayDevice.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0,
                    Orientation = ConvertToDisplayOrientation(devMode.dmDisplayOrientation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting monitor info for {displayDevice.DeviceName}.", ex);
                return null;
            }
        }

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

        private bool ApplyMonitorConfiguration(DisplayPreset preset)
        {
            if (preset?.Monitors == null || preset.Monitors.Count == 0)
            {
                return false;
            }

            var validation = ValidatePreset(preset);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Cannot apply preset: {validation.ErrorMessage}");
            }

            var currentConfig = GetCurrentMonitorConfiguration();
            var currentConfigMap = currentConfig.ToDictionary(m => m.PersistentId);

            var mappedMonitors = new List<(MonitorInfo presetMonitor, MonitorInfo targetMonitor)>();
            foreach(var presetMonitor in preset.Monitors)
            {
                if (currentConfigMap.TryGetValue(presetMonitor.PersistentId, out var currentMonitor))
                {
                    var targetMonitor = new MonitorInfo
                    {
                        PersistentId = presetMonitor.PersistentId,
                        Width = presetMonitor.Width,
                        Height = presetMonitor.Height,
                        PositionX = presetMonitor.PositionX,
                        PositionY = presetMonitor.PositionY,
                        IsPrimary = presetMonitor.IsPrimary,
                        Orientation = presetMonitor.Orientation,
                        DeviceName = currentMonitor.DeviceName
                    };
                    mappedMonitors.Add((presetMonitor, targetMonitor));
                }
                else
                {
                    throw new InvalidOperationException($"Monitor with ID '{presetMonitor.PersistentId}' from the preset is not connected to the system.");
                }
            }

            var primaryMappedMonitorTuple = mappedMonitors.FirstOrDefault(m => m.targetMonitor.IsPrimary);
            if (primaryMappedMonitorTuple.targetMonitor == null)
            {
                throw new InvalidOperationException("Preset does not have a primary monitor.");
            }
            var primaryMappedMonitor = primaryMappedMonitorTuple.targetMonitor;

            try
            {
                foreach (var (_, targetMonitor) in mappedMonitors.Where(m => !m.targetMonitor.IsPrimary))
                {
                    var devMode = new DEVMODE();
                    devMode.dmSize = (short)Marshal.SizeOf(devMode);
                    if (EnumDisplaySettings(targetMonitor.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                    {
                        devMode.dmPelsWidth = 0;
                        devMode.dmPelsHeight = 0;
                        devMode.dmFields = 0x00080000 | 0x00100000; // DM_PELSWIDTH | DM_PELSHEIGHT
                        _logger.LogInfo($"Disabling non-primary monitor {targetMonitor.DeviceName}.");
                        ChangeDisplaySettingsEx(targetMonitor.DeviceName, ref devMode, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET), IntPtr.Zero);
                    }
                }

                var primaryDevMode = new DEVMODE();
                primaryDevMode.dmSize = (short)Marshal.SizeOf(primaryDevMode);
                if (!EnumDisplaySettings(primaryMappedMonitor.DeviceName, ENUM_CURRENT_SETTINGS, ref primaryDevMode))
                {
                    throw new InvalidOperationException($"Failed to get current settings for primary monitor {primaryMappedMonitor.DeviceName}");
                }

                primaryDevMode.dmPelsWidth = primaryMappedMonitor.Width;
                primaryDevMode.dmPelsHeight = primaryMappedMonitor.Height;
                primaryDevMode.dmPositionX = 0;
                primaryDevMode.dmPositionY = 0;
                primaryDevMode.dmDisplayOrientation = ConvertFromDisplayOrientation(primaryMappedMonitor.Orientation);
                primaryDevMode.dmFields = 0x00080000 | 0x00100000 | 0x00000020 | 0x00000080; // DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION
                
                _logger.LogInfo($"Setting primary display settings for {primaryMappedMonitor.DeviceName}.");
                int primaryApplyResult = ChangeDisplaySettingsEx(primaryMappedMonitor.DeviceName, ref primaryDevMode, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_SET_PRIMARY | ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET), IntPtr.Zero);
                if (primaryApplyResult != (int)DISP_CHANGE.SUCCESSFUL)
                {
                    string errorMsg = GetDisplayChangeErrorMessage(primaryApplyResult);
                    throw new InvalidOperationException($"Failed to set primary monitor {primaryMappedMonitor.DeviceName}: {errorMsg}");
                }

                int originalPrimaryX = primaryMappedMonitor.PositionX;
                int originalPrimaryY = primaryMappedMonitor.PositionY;
                foreach (var (_, targetMonitor) in mappedMonitors.Where(m => !m.targetMonitor.IsPrimary))
                {
                    var devMode = new DEVMODE();
                    devMode.dmSize = (short)Marshal.SizeOf(devMode);
                    if (EnumDisplaySettings(targetMonitor.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                    {
                        devMode.dmPelsWidth = targetMonitor.Width;
                        devMode.dmPelsHeight = targetMonitor.Height;
                        devMode.dmPositionX = targetMonitor.PositionX - originalPrimaryX;
                        devMode.dmPositionY = targetMonitor.PositionY - originalPrimaryY;
                        devMode.dmDisplayOrientation = ConvertFromDisplayOrientation(targetMonitor.Orientation);
                        devMode.dmFields = 0x00080000 | 0x00100000 | 0x00000020 | 0x00000080; // DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION
                        
                        _logger.LogInfo($"Enabling non-primary display settings for {targetMonitor.DeviceName}.");
                        int applyResult = ChangeDisplaySettingsEx(targetMonitor.DeviceName, ref devMode, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET), IntPtr.Zero);
                        if (applyResult != (int)DISP_CHANGE.SUCCESSFUL)
                        {
                            string errorMsg = GetDisplayChangeErrorMessage(applyResult);
                            throw new InvalidOperationException($"Display settings apply failed for {targetMonitor.DeviceName}: {errorMsg}");
                        }
                    }
                }

                _logger.LogInfo("Applying all display changes.");
                int finalResult = ChangeDisplaySettings(IntPtr.Zero, 0);
                if (finalResult != (int)DISP_CHANGE.SUCCESSFUL)
                {
                    string errorMsg = GetDisplayChangeErrorMessage(finalResult);
                    throw new InvalidOperationException($"Failed to apply final display settings: {errorMsg}");
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogInfo("Rolling back display configuration.");
                    RollbackConfiguration(currentConfig);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError("Failed to rollback display configuration.", rollbackEx);
                }
                
                throw new InvalidOperationException($"Failed to apply monitor configuration: {ex.Message}", ex);
            }
        }

        private bool ApplyMonitorSettings(MonitorInfo monitorConfig)
        {
            try
            {
                var devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                if (!EnumDisplaySettings(monitorConfig.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    throw new InvalidOperationException($"Failed to get current settings for {monitorConfig.DeviceName}");
                }

                devMode.dmPelsWidth = monitorConfig.Width;
                devMode.dmPelsHeight = monitorConfig.Height;
                devMode.dmPositionX = monitorConfig.PositionX;
                devMode.dmPositionY = monitorConfig.PositionY;
                devMode.dmDisplayOrientation = ConvertFromDisplayOrientation(monitorConfig.Orientation);

                const int DM_PELSWIDTH = 0x00080000;
                const int DM_PELSHEIGHT = 0x00100000;
                const int DM_POSITION = 0x00000020;
                const int DM_DISPLAYORIENTATION = 0x00000080;
                
                devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION;

                _logger.LogInfo($"Testing display settings for {monitorConfig.DeviceName}. Width: {devMode.dmPelsWidth}, Height: {devMode.dmPelsHeight}, PosX: {devMode.dmPositionX}, PosY: {devMode.dmPositionY}, Orientation: {devMode.dmDisplayOrientation}");
                int result = ChangeDisplaySettingsEx(monitorConfig.DeviceName, ref devMode, IntPtr.Zero, (uint)ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                if (result != (int)DISP_CHANGE.SUCCESSFUL)
                {
                    string errorMsg = GetDisplayChangeErrorMessage(result);
                    throw new InvalidOperationException($"Display settings test failed for {monitorConfig.DeviceName}: {errorMsg}");
                }

                _logger.LogInfo($"Applying display settings for {monitorConfig.DeviceName}.");
                result = ChangeDisplaySettingsEx(monitorConfig.DeviceName, ref devMode, IntPtr.Zero, (uint)ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                if (result != (int)DISP_CHANGE.SUCCESSFUL)
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
                }
            }
        }

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
                var currentMonitors = GetCurrentMonitorConfiguration();
                var currentPersistentIds = currentMonitors.Select(m => m.PersistentId).ToHashSet();

                foreach (var presetMonitor in preset.Monitors)
                {
                    if (string.IsNullOrWhiteSpace(presetMonitor.PersistentId))
                    {
                        return (false, $"Monitor configuration for '{presetMonitor.DeviceName}' is missing a persistent ID.");
                    }

                    if (!currentPersistentIds.Contains(presetMonitor.PersistentId))
                    {
                        return (false, $"Monitor with ID '{presetMonitor.PersistentId}' not found in current system");
                    }

                    if (presetMonitor.Width <= 0 || presetMonitor.Height <= 0)
                    {
                        return (false, $"Invalid resolution for monitor '{presetMonitor.DeviceName}': {presetMonitor.Width}x{presetMonitor.Height}");
                    }

                    if (!Enum.IsDefined(typeof(DisplayOrientation), presetMonitor.Orientation))
                    {
                        return (false, $"Invalid orientation for monitor '{presetMonitor.DeviceName}': {presetMonitor.Orientation}");
                    }
                }

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

        private void SetPrimaryMonitor(string deviceName)
        {
            try
            {
                var monitors = GetCurrentMonitorConfiguration();
                var monitor = monitors.FirstOrDefault(m => m.DeviceName == deviceName);

                if (monitor == null) return;

                var devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);

                if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    devMode.dmPelsWidth = monitor.Width;
                    devMode.dmPelsHeight = monitor.Height;
                    devMode.dmPositionX = 0;
                    devMode.dmPositionY = 0;
                    devMode.dmDisplayOrientation = ConvertFromDisplayOrientation(monitor.Orientation);

                    const int DM_PELSWIDTH = 0x00080000;
                    const int DM_PELSHEIGHT = 0x00100000;
                    const int DM_POSITION = 0x00000020;
                    const int DM_DISPLAYORIENTATION = 0x00000080;

                    devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION;

                    ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET), IntPtr.Zero);
                }
            }
            catch
            {
            }
        }

        private void RefreshDesktop()
        {
            try
            {
                ChangeDisplaySettings(IntPtr.Zero, 0);
                
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            catch
            {
            }
        }
    }
}


