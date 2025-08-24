using MonitorPresetManager.Services;
using MonitorPresetManager.Models;

namespace MonitorPresetManager.Forms
{
    /// <summary>
    /// Form for application settings
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly IStartupManager _startupManager;
        private StartupStatus _currentStartupStatus;

        /// <summary>
        /// Initialize settings form
        /// </summary>
        /// <param name="startupManager">Startup manager service</param>
        public SettingsForm(IStartupManager startupManager)
        {
            _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));
            _currentStartupStatus = new StartupStatus();

            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// Initialize form properties and event handlers
        /// </summary>
        private void InitializeForm()
        {
            this.Text = "Settings";
            this.Size = new Size(400, 300);
            this.MinimumSize = new Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Initialize event handlers
            this.Load += SettingsForm_Load;
            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonApply.Click += ButtonApply_Click;
        }

        /// <summary>
        /// Handle form load event
        /// </summary>
        private void SettingsForm_Load(object? sender, EventArgs e)
        {
            LoadCurrentSettings();
            UpdateUI();
        }

        /// <summary>
        /// Load current settings from system
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                _currentStartupStatus = _startupManager.GetStartupStatus();
                
                checkBoxStartWithWindows.Checked = _currentStartupStatus.IsEnabled;
                checkBoxStartMinimized.Checked = _currentStartupStatus.StartMinimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load current settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update UI state based on current settings
        /// </summary>
        private void UpdateUI()
        {
            buttonApply.Enabled = HasChanges();
        }

        /// <summary>
        /// Check if there are unsaved changes
        /// </summary>
        /// <returns>True if there are changes</returns>
        private bool HasChanges()
        {
            return checkBoxStartWithWindows.Checked != _currentStartupStatus.IsEnabled ||
                   (checkBoxStartWithWindows.Checked && checkBoxStartMinimized.Checked != _currentStartupStatus.StartMinimized);
        }

        /// <summary>
        /// Apply current settings
        /// </summary>
        private bool ApplySettings()
        {
            try
            {
                var enabled = checkBoxStartWithWindows.Checked;
                var startMinimized = checkBoxStartMinimized.Checked;

                var success = _startupManager.UpdateStartupSettings(enabled, startMinimized);
                
                if (success)
                {
                    // Reload current status
                    _currentStartupStatus = _startupManager.GetStartupStatus();
                    UpdateUI();
                    
                    return true;
                }
                else
                {
                    MessageBox.Show("Failed to update startup settings. You may need administrator privileges.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle OK button click
        /// </summary>
        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            if (HasChanges())
            {
                if (ApplySettings())
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Handle Cancel button click
        /// </summary>
        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Handle Apply button click
        /// </summary>
        private void ButtonApply_Click(object? sender, EventArgs e)
        {
            if (ApplySettings())
            {
                MessageBox.Show("Settings applied successfully.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion
    }
}