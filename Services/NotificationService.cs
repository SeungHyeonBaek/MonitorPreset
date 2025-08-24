namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Service for handling user notifications and confirmations
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ITrayManager? _trayManager;
        private readonly ILogger? _logger;
        private bool _notificationsEnabled = true;

        /// <summary>
        /// Initialize notification service
        /// </summary>
        /// <param name="trayManager">Tray manager for system tray notifications (optional)</param>
        /// <param name="logger">Logger for logging notifications (optional)</param>
        public NotificationService(ITrayManager? trayManager = null, ILogger? logger = null)
        {
            _trayManager = trayManager;
            _logger = logger;
        }

        /// <summary>
        /// Show an information notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        public void ShowInfo(string title, string message, int duration = 3000)
        {
            if (!_notificationsEnabled) return;

            _logger?.LogInfo($"Info notification: {title} - {message}", "Notification");
            
            if (_trayManager?.IsVisible == true)
            {
                _trayManager.ShowNotification(title, message, ToolTipIcon.Info);
            }
            else
            {
                // Fallback to message box if tray is not available
                ShowMessageBox(title, message, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Show a success notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        public void ShowSuccess(string title, string message, int duration = 3000)
        {
            if (!_notificationsEnabled) return;

            _logger?.LogInfo($"Success notification: {title} - {message}", "Notification");
            
            if (_trayManager?.IsVisible == true)
            {
                _trayManager.ShowNotification(title, message, ToolTipIcon.Info);
            }
            else
            {
                ShowMessageBox(title, message, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Show a warning notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        public void ShowWarning(string title, string message, int duration = 4000)
        {
            if (!_notificationsEnabled) return;

            _logger?.LogWarning($"Warning notification: {title} - {message}", "Notification");
            
            if (_trayManager?.IsVisible == true)
            {
                _trayManager.ShowNotification(title, message, ToolTipIcon.Warning);
            }
            else
            {
                ShowMessageBox(title, message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Show an error notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="duration">Duration in milliseconds (optional)</param>
        public void ShowError(string title, string message, int duration = 5000)
        {
            if (!_notificationsEnabled) return;

            _logger?.LogError($"Error notification: {title} - {message}", null, "Notification");
            
            if (_trayManager?.IsVisible == true)
            {
                _trayManager.ShowNotification(title, message, ToolTipIcon.Error);
            }
            else
            {
                ShowMessageBox(title, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultButton">Default button (Yes/No)</param>
        /// <returns>True if user clicked Yes, false if No</returns>
        public bool ShowConfirmation(string title, string message, bool defaultButton = false)
        {
            _logger?.LogInfo($"Confirmation dialog: {title} - {message}", "Dialog");
            
            var defaultBtn = defaultButton ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2;
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultBtn);
            
            _logger?.LogInfo($"Confirmation result: {result}", "Dialog");
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Show an input dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultValue">Default input value</param>
        /// <returns>User input or null if cancelled</returns>
        public string? ShowInputDialog(string title, string message, string defaultValue = "")
        {
            _logger?.LogInfo($"Input dialog: {title} - {message}", "Dialog");
            
            using (var inputDialog = new InputDialog(title, message, defaultValue))
            {
                var result = inputDialog.ShowDialog();
                var userInput = result == DialogResult.OK ? inputDialog.InputText : null;
                
                _logger?.LogInfo($"Input dialog result: {result}, Input: {userInput ?? "null"}", "Dialog");
                return userInput;
            }
        }

        /// <summary>
        /// Show a message box with custom buttons
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="buttons">Button configuration</param>
        /// <param name="icon">Icon type</param>
        /// <returns>Dialog result</returns>
        public DialogResult ShowMessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _logger?.LogInfo($"Message box: {title} - {message} (Buttons: {buttons}, Icon: {icon})", "Dialog");
            
            var result = MessageBox.Show(message, title, buttons, icon);
            
            _logger?.LogInfo($"Message box result: {result}", "Dialog");
            return result;
        }

        /// <summary>
        /// Enable or disable notifications
        /// </summary>
        /// <param name="enabled">Whether notifications are enabled</param>
        public void SetNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            _logger?.LogInfo($"Notifications {(enabled ? "enabled" : "disabled")}", "Settings");
        }

        /// <summary>
        /// Check if notifications are enabled
        /// </summary>
        /// <returns>True if notifications are enabled</returns>
        public bool AreNotificationsEnabled()
        {
            return _notificationsEnabled;
        }
    }

    /// <summary>
    /// Simple input dialog for getting text input from user
    /// </summary>
    public partial class InputDialog : Form
    {
        private TextBox _textBox = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Label _messageLabel = null!;

        /// <summary>
        /// Get the input text from the dialog
        /// </summary>
        public string InputText => _textBox.Text;

        /// <summary>
        /// Initialize input dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultValue">Default input value</param>
        public InputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            
            this.Text = title;
            _messageLabel.Text = message;
            _textBox.Text = defaultValue;
            
            // Select all text for easy replacement
            _textBox.SelectAll();
        }

        /// <summary>
        /// Initialize dialog components
        /// </summary>
        private void InitializeComponent()
        {
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Message label
            _messageLabel = new Label
            {
                Location = new Point(12, 15),
                Size = new Size(360, 20),
                Text = "Enter value:"
            };

            // Text input
            _textBox = new TextBox
            {
                Location = new Point(12, 40),
                Size = new Size(360, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // OK button
            _okButton = new Button
            {
                Location = new Point(217, 75),
                Size = new Size(75, 23),
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            _okButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            // Cancel button
            _cancelButton = new Button
            {
                Location = new Point(297, 75),
                Size = new Size(75, 23),
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Set default and cancel buttons
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            // Add controls
            this.Controls.AddRange(new Control[] { _messageLabel, _textBox, _okButton, _cancelButton });

            // Focus on text box
            this.Load += (s, e) => _textBox.Focus();
        }
    }
}