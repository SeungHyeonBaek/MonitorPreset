using MonitorPresetManager.Models;
using MonitorPresetManager.Services;

namespace MonitorPresetManager.Forms
{
    /// <summary>
    /// Form for configuring hotkeys for presets
    /// </summary>
    public partial class HotkeySettingsForm : Form
    {
        private readonly IPresetManager _presetManager;
        private readonly IHotkeyManager _hotkeyManager;
        private readonly ILogger _logger;
        private List<DisplayPreset> _presets;
        private Dictionary<string, int> _presetHotkeyIds;

        /// <summary>
        /// Initialize hotkey settings form
        /// </summary>
        /// <param name="presetManager">Preset manager service</param>
        /// <param name="hotkeyManager">Hotkey manager service</param>
        public HotkeySettingsForm(IPresetManager presetManager, IHotkeyManager hotkeyManager)
        {
            _presetManager = presetManager ?? throw new ArgumentNullException(nameof(presetManager));
            _hotkeyManager = hotkeyManager ?? throw new ArgumentNullException(nameof(hotkeyManager));
            _logger = new FileLogger();
            _presets = new List<DisplayPreset>();
            _presetHotkeyIds = new Dictionary<string, int>();

            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// Initialize form properties and event handlers
        /// </summary>
        private void InitializeForm()
        {
            this.Text = "Hotkey Settings";
            this.Size = new Size(600, 400);
            this.MinimumSize = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Initialize event handlers
            this.Load += HotkeySettingsForm_Load;
            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonAssignHotkey.Click += ButtonAssignHotkey_Click;
            buttonRemoveHotkey.Click += ButtonRemoveHotkey_Click;
            listViewPresets.SelectedIndexChanged += ListViewPresets_SelectedIndexChanged;
            
            // Add event handlers for real-time preview
            checkBoxCtrl.CheckedChanged += HotkeyInput_Changed;
            checkBoxAlt.CheckedChanged += HotkeyInput_Changed;
            checkBoxShift.CheckedChanged += HotkeyInput_Changed;
            checkBoxWin.CheckedChanged += HotkeyInput_Changed;
            comboBoxKey.SelectedIndexChanged += HotkeyInput_Changed;
        }

        /// <summary>
        /// Handle form load event
        /// </summary>
        private async void HotkeySettingsForm_Load(object? sender, EventArgs e)
        {
            await LoadPresets();
            PopulatePresetHotkeyIds();
            UpdateUI();
        }

        /// <summary>
        /// Load all presets
        /// </summary>
        private async Task LoadPresets()
        {
            try
            {
                _presets = await _presetManager.GetAllPresetsAsync();
                
                listViewPresets.Items.Clear();
                
                foreach (var preset in _presets)
                {
                    var item = new ListViewItem(preset.Name);
                    item.SubItems.Add(string.IsNullOrEmpty(preset.HotKey) ? "None" : preset.HotKey);
                    item.Tag = preset;
                    listViewPresets.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load presets: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update UI state based on selection
        /// </summary>
        private void UpdateUI()
        {
            var selectedPreset = GetSelectedPreset();

            if (selectedPreset == null)
            {
                buttonAssignHotkey.Enabled = false;
                buttonRemoveHotkey.Enabled = false;
                labelSelectedPreset.Text = "Selected: None";
                labelCurrentHotkey.Text = "Current Hotkey: None";
                UpdateHotkeyInputControls(null);
                return;
            }

            var hasHotkey = !string.IsNullOrEmpty(selectedPreset.HotKey);

            buttonAssignHotkey.Enabled = true;
            buttonRemoveHotkey.Enabled = hasHotkey;
            labelSelectedPreset.Text = $"Selected: {selectedPreset.Name}";
            labelCurrentHotkey.Text = $"Current Hotkey: {(hasHotkey ? selectedPreset.HotKey : "None")}";
            
            // Update hotkey input controls
            UpdateHotkeyInputControls(selectedPreset.HotKey);
        }

        private void PopulatePresetHotkeyIds()
        {
            _presetHotkeyIds.Clear();
            var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();
            foreach (var preset in _presets)
            {
                if (!string.IsNullOrEmpty(preset.HotKey))
                {
                    foreach (var hotkey in registeredHotkeys)
                    {
                        var hotkeyString = HotkeyHelper.GetHotkeyString(hotkey.Value.Modifiers, hotkey.Value.Key);
                        if (hotkeyString == preset.HotKey)
                        {
                            _presetHotkeyIds[preset.Name] = hotkey.Key;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update hotkey input controls based on current hotkey
        /// </summary>
        private void UpdateHotkeyInputControls(string? currentHotkey)
        {
            // Reset all checkboxes
            checkBoxCtrl.Checked = false;
            checkBoxAlt.Checked = false;
            checkBoxShift.Checked = false;
            checkBoxWin.Checked = false;
            comboBoxKey.SelectedIndex = -1;

            if (!string.IsNullOrEmpty(currentHotkey))
            {
                // Parse current hotkey and update controls
                ParseHotkeyString(currentHotkey);
            }
        }

        /// <summary>
        /// Parse hotkey string and update UI controls
        /// </summary>
        private void ParseHotkeyString(string hotkeyString)
        {
            // Simple parsing - format: "Ctrl+Alt+F1"
            var parts = hotkeyString.Split('+');
            
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                switch (trimmedPart.ToLowerInvariant())
                {
                    case "ctrl":
                        checkBoxCtrl.Checked = true;
                        break;
                    case "alt":
                        checkBoxAlt.Checked = true;
                        break;
                    case "shift":
                        checkBoxShift.Checked = true;
                        break;
                    case "win":
                        checkBoxWin.Checked = true;
                        break;
                    default:
                        // Try to find the key in the combo box
                        for (int i = 0; i < comboBoxKey.Items.Count; i++)
                        {
                            if (comboBoxKey.Items[i]?.ToString() == trimmedPart)
                            {
                                comboBoxKey.SelectedIndex = i;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Get currently selected preset
        /// </summary>
        private DisplayPreset? GetSelectedPreset()
        {
            if (listViewPresets.SelectedItems.Count > 0)
            {
                return listViewPresets.SelectedItems[0].Tag as DisplayPreset;
            }
            return null;
        }

        /// <summary>
        /// Convert hotkey string to HotkeyModifiers enum
        /// </summary>
        private HotkeyModifiers GetHotkeyModifiers()
        {
            var modifiers = HotkeyModifiers.None;

            if (checkBoxCtrl.Checked) modifiers |= HotkeyModifiers.Control;
            if (checkBoxAlt.Checked) modifiers |= HotkeyModifiers.Alt;
            if (checkBoxShift.Checked) modifiers |= HotkeyModifiers.Shift;
            if (checkBoxWin.Checked) modifiers |= HotkeyModifiers.Windows;

            return modifiers;
        }

        /// <summary>
        /// Get selected key
        /// </summary>
        private Keys? GetSelectedKey()
        {
            if (comboBoxKey.SelectedItem != null)
            {
                var keyString = comboBoxKey.SelectedItem.ToString();
                if (Enum.TryParse<Keys>(keyString, out var key))
                {
                    return key;
                }
            }
            return null;
        }

        #region Event Handlers

        /// <summary>
        /// Handle preset selection change
        /// </summary>
        private void ListViewPresets_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handle assign hotkey button click
        /// </summary>
        private async void ButtonAssignHotkey_Click(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset == null)
            {
                return;
            }

            var modifiers = GetHotkeyModifiers();
            var key = GetSelectedKey();

            if (modifiers == HotkeyModifiers.None || key == null)
            {
                MessageBox.Show("Please select at least one modifier key and a main key.", "Invalid Hotkey", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check if hotkey is available
                var currentHotkeyString = HotkeyHelper.GetHotkeyString(modifiers, key.Value);
                _logger.LogInfo($"Attempting to assign hotkey: {currentHotkeyString} to preset {selectedPreset.Name}");
                
                // First check if this hotkey is already used by another preset in our application
                var existingPreset = _presets.FirstOrDefault(p => p.HotKey == currentHotkeyString && p.Name != selectedPreset.Name);
                if (existingPreset != null)
                {
                    _logger.LogWarning($"Hotkey conflict: {currentHotkeyString} already assigned to {existingPreset.Name}");
                    MessageBox.Show($"This hotkey combination is already assigned to preset '{existingPreset.Name}'.", "Hotkey Conflict", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Then check if it's available system-wide
                if (!_hotkeyManager.IsHotkeyAvailable(modifiers, key.Value))
                {
                    _logger.LogWarning($"Hotkey {currentHotkeyString} may be in use by another application.");
                    var result = MessageBox.Show(
                        $"The hotkey combination '{currentHotkeyString}' may be in use by another application.\n\n" +
                        "Would you like to try assigning it anyway? This might work if the conflict is temporary.",
                        "Potential Hotkey Conflict", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // Remove existing hotkey if any
                if (!string.IsNullOrEmpty(selectedPreset.HotKey) && _presetHotkeyIds.ContainsKey(selectedPreset.Name))
                {
                    _logger.LogInfo($"Unregistering old hotkey for {selectedPreset.Name}: {selectedPreset.HotKey}");
                    _hotkeyManager.UnregisterHotkey(_presetHotkeyIds[selectedPreset.Name]);
                    _presetHotkeyIds.Remove(selectedPreset.Name);
                }

                // Register new hotkey
                var hotkeyId = _hotkeyManager.GenerateHotkeyId();
                if (_hotkeyManager.RegisterHotkey(hotkeyId, modifiers, key.Value))
                {
                    selectedPreset.HotKey = currentHotkeyString;
                    _presetHotkeyIds[selectedPreset.Name] = hotkeyId;
                    _logger.LogInfo($"Successfully assigned hotkey {currentHotkeyString} to preset {selectedPreset.Name} with ID {hotkeyId}");

                    // Save preset
                    await _presetManager.SavePresetAsync(selectedPreset, overwrite: true);

                    // Update UI
                    await LoadPresets();
                    UpdateUI();

                    MessageBox.Show($"Hotkey '{currentHotkeyString}' assigned to preset '{selectedPreset.Name}'.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _logger.LogError($"Failed to register hotkey {currentHotkeyString} for preset {selectedPreset.Name}");
                    MessageBox.Show("Failed to register hotkey. It may be in use by another application.", "Registration Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while assigning hotkey to preset {selectedPreset.Name}", ex);
                MessageBox.Show($"Failed to assign hotkey: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle remove hotkey button click
        /// </summary>
        private async void ButtonRemoveHotkey_Click(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset == null || string.IsNullOrEmpty(selectedPreset.HotKey))
            {
                return;
            }

            try
            {
                _logger.LogInfo($"Attempting to remove hotkey from preset {selectedPreset.Name}: {selectedPreset.HotKey}");
                // Unregister hotkey
                if (_presetHotkeyIds.ContainsKey(selectedPreset.Name))
                {
                    _hotkeyManager.UnregisterHotkey(_presetHotkeyIds[selectedPreset.Name]);
                    _presetHotkeyIds.Remove(selectedPreset.Name);
                    _logger.LogInfo($"Successfully unregistered hotkey for preset {selectedPreset.Name}");
                }
                else
                {
                    _logger.LogWarning($"Could not find hotkey ID for preset {selectedPreset.Name} in _presetHotkeyIds.");
                }

                // Remove hotkey from preset
                selectedPreset.HotKey = string.Empty;

                // Save preset
                await _presetManager.SavePresetAsync(selectedPreset, overwrite: true);

                // Update UI
                await LoadPresets();
                UpdateUI();

                MessageBox.Show($"Hotkey removed from preset '{selectedPreset.Name}'.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while removing hotkey from preset {selectedPreset.Name}", ex);
                MessageBox.Show($"Failed to remove hotkey: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle OK button click
        /// </summary>
        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
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
        /// Handle hotkey input change for real-time preview
        /// </summary>
        private void HotkeyInput_Changed(object? sender, EventArgs e)
        {
            UpdateHotkeyPreview();
        }

        /// <summary>
        /// Update hotkey preview label
        /// </summary>
        private void UpdateHotkeyPreview()
        {
            var modifiers = GetHotkeyModifiers();
            var key = GetSelectedKey();
            var hotkeyString = HotkeyHelper.GetHotkeyString(modifiers, key ?? Keys.None);
            
            if (string.IsNullOrEmpty(hotkeyString) || key == null)
            {
                labelHotkeyPreview.Text = "None";
                labelHotkeyPreview.ForeColor = SystemColors.ControlText;
                return;
            }

            labelHotkeyPreview.Text = hotkeyString;

            // Check availability and update color
            if (modifiers != HotkeyModifiers.None && key != null)
            {
                // Check if already used by another preset
                var selectedPreset = GetSelectedPreset();
                var existingPreset = _presets.FirstOrDefault(p => p.HotKey == hotkeyString && p.Name != selectedPreset?.Name);
                
                if (existingPreset != null)
                {
                    labelHotkeyPreview.ForeColor = Color.Red;
                    labelHotkeyPreview.Text += $" (Used by '{existingPreset.Name}')";
                }
                else if (!_hotkeyManager.IsHotkeyAvailable(modifiers, key.Value))
                {
                    labelHotkeyPreview.ForeColor = Color.Orange;
                    labelHotkeyPreview.Text += " (May conflict)";
                }
                else
                {
                    labelHotkeyPreview.ForeColor = Color.Green;
                    labelHotkeyPreview.Text += " (Available)";
                }
            }
            else
            {
                labelHotkeyPreview.ForeColor = SystemColors.ControlText;
            }
        }

        #endregion
    }
}