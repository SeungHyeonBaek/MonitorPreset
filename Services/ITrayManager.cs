namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for system tray management
    /// </summary>
    public interface ITrayManager : IDisposable
    {
        /// <summary>
        /// Event fired when user wants to show the main window
        /// </summary>
        event EventHandler? ShowMainWindow;

        /// <summary>
        /// Event fired when user wants to exit the application
        /// </summary>
        event EventHandler? ExitApplication;

        /// <summary>
        /// Event fired when user wants to apply a preset from tray menu
        /// </summary>
        event EventHandler<string>? ApplyPreset;

        /// <summary>
        /// Show the tray icon
        /// </summary>
        void ShowTrayIcon();

        /// <summary>
        /// Hide the tray icon
        /// </summary>
        void HideTrayIcon();

        /// <summary>
        /// Check if tray icon is visible
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Update the preset list in the tray context menu
        /// </summary>
        /// <param name="presetNames">List of preset names</param>
        void UpdatePresetMenu(List<string> presetNames);

        /// <summary>
        /// Show a balloon tip notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="text">Notification text</param>
        /// <param name="icon">Notification icon type</param>
        void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info);

        /// <summary>
        /// Set the tray icon tooltip text
        /// </summary>
        /// <param name="text">Tooltip text</param>
        void SetTooltipText(string text);
    }
}