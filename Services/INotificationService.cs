namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for notification services
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Show an information notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        void ShowInfo(string title, string message, int duration = 3000);

        /// <summary>
        /// Show a success notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        void ShowSuccess(string title, string message, int duration = 3000);

        /// <summary>
        /// Show a warning notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        void ShowWarning(string title, string message, int duration = 4000);

        /// <summary>
        /// Show an error notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        void ShowError(string title, string message, int duration = 5000);

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultButton">Default button (Yes/No)</param>
        /// <returns>True if user clicked Yes, false if No</returns>
        bool ShowConfirmation(string title, string message, bool defaultButton = false);

        /// <summary>
        /// Show an input dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultValue">Default input value</param>
        /// <returns>User input or null if cancelled</returns>
        string? ShowInputDialog(string title, string message, string defaultValue = "");

        /// <summary>
        /// Show a message box with custom buttons
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="buttons">Button configuration</param>
        /// <param name="icon">Icon type</param>
        /// <returns>Dialog result</returns>
        DialogResult ShowMessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon);

        /// <summary>
        /// Enable or disable notifications
        /// </summary>
        /// <param name="enabled">Whether notifications are enabled</param>
        void SetNotificationsEnabled(bool enabled);

        /// <summary>
        /// Check if notifications are enabled
        /// </summary>
        /// <returns>True if notifications are enabled</returns>
        bool AreNotificationsEnabled();
    }
}