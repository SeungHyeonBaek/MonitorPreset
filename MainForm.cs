using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonitorPresetManager.Models;
using MonitorPresetManager.Services;

namespace MonitorPresetManager
{
    /// <summary>
    /// Simple main form for testing basic functionality
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly IDisplayService _displayService;
        private readonly IPresetManager _presetManager;
        private IHotkeyManager _hotkeyManager;
        private readonly ITrayManager _trayManager;
        private readonly ILogger _logger;
        private HotkeyMessageWindow? _hotkeyMessageWindow;
        private Dictionary<string, int> _registeredPresetHotkeyIds; // Stores hotkey IDs for registered presets
        private Services.ActivationMessageWindow? _activationWindow; // Hidden window to receive activation
        private readonly bool _startMinimized;
        
        // UI Controls
        private ListView listViewPresets = null!;
        private Button buttonSavePreset = null!;
        private Button buttonApplyPreset = null!;
        private Button buttonRenamePreset = null!;
        private Button buttonDeletePreset = null!;
        private Label labelDetailsInfo = null!;

        public MainForm(bool startMinimized = false)
        {
            // Initialize services (HotkeyManager will be initialized after handle creation)
            _logger = new FileLogger();
            _displayService = new DisplayManager(_logger);
            _presetManager = new PresetManager(new FileStorage());
            _hotkeyManager = null!; // Will be initialized in Load event
            _trayManager = new TrayManager();
            _registeredPresetHotkeyIds = new Dictionary<string, int>();
            _startMinimized = startMinimized;
            
            InitializeComponent();
            InitializeForm();

            // Create activation receiver window (hidden top-level window)
            _activationWindow = new Services.ActivationMessageWindow(() =>
            {
                try
                {
                    RestoreFromTray();
                }
                catch { /* best-effort */ }
            });

            // If requested, start minimized to tray after first show
            this.Shown += (s, e) =>
            {
                if (_startMinimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                    MinimizeToTray();
                }
            };
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Monitor Preset Manager";
            this.Size = new Size(1024, 565);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 465);
            
            // Create menu strip
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var settingsMenu = new ToolStripMenuItem("Settings");
            var helpMenu = new ToolStripMenuItem("Help");
            
            // File menu items
            var exportMenuItem = new ToolStripMenuItem("Export Presets");
            exportMenuItem.Click += async (s, e) => await ExportPresetsAsync();
            var importMenuItem = new ToolStripMenuItem("Import Presets");
            importMenuItem.Click += async (s, e) => await ImportPresetsAsync();
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) => Application.Exit();
            fileMenu.DropDownItems.Add(exportMenuItem);
            fileMenu.DropDownItems.Add(importMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            // Settings menu items
            var hotkeySettingsMenuItem = new ToolStripMenuItem("Hotkey Settings");
            hotkeySettingsMenuItem.Click += HotkeySettingsMenuItem_Click;
            var generalSettingsMenuItem = new ToolStripMenuItem("General Settings");
            generalSettingsMenuItem.Click += GeneralSettingsMenuItem_Click;
            settingsMenu.DropDownItems.Add(hotkeySettingsMenuItem);
            settingsMenu.DropDownItems.Add(generalSettingsMenuItem);
            
            // Help menu items
            var aboutMenuItem = new ToolStripMenuItem("About");
                        aboutMenuItem.Click += (s, e) => 
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var versionString = version != null ? $"v{version.Major}.{version.Minor}" : "v1.2";
                MessageBox.Show($"Monitor Preset Manager {versionString}\n\nMulti-monitor configuration management tool", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            helpMenu.DropDownItems.Add(aboutMenuItem);
            
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(settingsMenu);
            menuStrip.Items.Add(helpMenu);
            
            // Display Presets label
            var labelDisplayPresets = new Label()
            {
                Text = "Display Presets",
                Location = new Point(20, 35),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            
            // Preset list
            listViewPresets = new ListView()
            {
                Location = new Point(20, 60),
                Size = new Size(640, 400),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            
            listViewPresets.Columns.Add("Name", 150);
            listViewPresets.Columns.Add("Monitors", 80);
            listViewPresets.Columns.Add("Hotkey", 100);
            listViewPresets.Columns.Add("Created", 120);
            
            // Preset Details panel
            var labelPresetDetails = new Label()
            {
                Text = "Preset Details",
                Location = new Point(680, 35),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            
            var presetDetailsPanel = new Panel()
            {
                Location = new Point(680, 60),
                Size = new Size(320, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            
            // Details content (will be updated dynamically)
            labelDetailsInfo = new Label()
            {
                Text = "Select a preset to view details",
                Location = new Point(10, 10),
                Size = new Size(300, 380),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.White
            };
            
            presetDetailsPanel.Controls.Add(labelDetailsInfo);
            
            // Buttons
            buttonSavePreset = new Button()
            {
                Text = "Save Current",
                Location = new Point(20, 470),
                Size = new Size(100, 35)
            };
            
            buttonApplyPreset = new Button()
            {
                Text = "Apply Selected",
                Location = new Point(130, 470),
                Size = new Size(100, 35),
                Enabled = false
            };
            
            buttonRenamePreset = new Button()
            {
                Text = "Rename",
                Location = new Point(240, 470),
                Size = new Size(80, 35),
                Enabled = false
            };
            
            buttonDeletePreset = new Button()
            {
                Text = "Delete",
                Location = new Point(330, 470),
                Size = new Size(80, 35),
                Enabled = false
            };
            
            // Add controls to form
            this.Controls.Add(menuStrip);
            this.Controls.Add(labelDisplayPresets);
            this.Controls.Add(listViewPresets);
            this.Controls.Add(labelPresetDetails);
            this.Controls.Add(presetDetailsPanel);
            this.Controls.Add(buttonSavePreset);
            this.Controls.Add(buttonApplyPreset);
            this.Controls.Add(buttonRenamePreset);
            this.Controls.Add(buttonDeletePreset);
            
            this.MainMenuStrip = menuStrip;
            this.ResumeLayout(false);
        }

        private async Task ExportPresetsAsync()
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Title = "Export Presets",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = $"MonitorPresets_{DateTime.Now:yyyyMMdd}.json",
                    AddExtension = true,
                    OverwritePrompt = true
                };
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var count = await _presetManager.ExportAllPresetsAsync(sfd.FileName);
                MessageBox.Show(this, $"프리셋 {count}개를 내보냈습니다.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"내보내기 실패: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ImportPresetsAsync()
        {
            try
            {
                using var ofd = new OpenFileDialog
                {
                    Title = "Import Presets",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                var overwrite = MessageBox.Show(this, "같은 이름의 프리셋이 있을 경우 덮어쓸까요?", "Import",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                var (imported, skipped, errors) = await _presetManager.ImportPresetsAsync(ofd.FileName, overwrite);
                await LoadPresets();

                var msg = $"가져오기 완료\n- 가져옴: {imported}\n- 건너뜀: {skipped}";
                if (errors.Count > 0)
                {
                    msg += "\n\n오류:\n" + string.Join("\n", errors.Take(10)) + (errors.Count > 10 ? "\n..." : string.Empty);
                }
                MessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"가져오기 실패: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeForm()
        {
            // Initialize tray manager
            InitializeTrayManager();
            
            // Event handlers
            this.Load += SimpleMainForm_Load;
            this.FormClosing += SimpleMainForm_FormClosing;
            this.Resize += SimpleMainForm_Resize;
            
            buttonSavePreset.Click += ButtonSavePreset_Click;
            buttonApplyPreset.Click += ButtonApplyPreset_Click;
            buttonRenamePreset.Click += ButtonRenamePreset_Click;
            buttonDeletePreset.Click += ButtonDeletePreset_Click;
            
            listViewPresets.SelectedIndexChanged += ListViewPresets_SelectedIndexChanged;
            listViewPresets.DoubleClick += ListViewPresets_DoubleClick;
        }

        /// <summary>
        /// Initialize tray manager and set up event handlers
        /// </summary>
        private void InitializeTrayManager()
        {
            // Set up tray manager event handlers
            _trayManager.ShowMainWindow += TrayManager_ShowMainWindow;
            _trayManager.ExitApplication += TrayManager_ExitApplication;
            _trayManager.ApplyPreset += TrayManager_ApplyPreset;
            
            // Show tray icon
            _trayManager.ShowTrayIcon();
            _trayManager.SetTooltipText("Monitor Preset Manager");
        }
        
        private async void SimpleMainForm_Load(object? sender, EventArgs e)
        {
            // Initialize the message-only window for hotkey processing
            _hotkeyMessageWindow = new HotkeyMessageWindow();
            
            // Initialize HotkeyManager with the message-only window's handle
            _hotkeyManager?.Dispose();
            _hotkeyManager = new HotkeyManager(_hotkeyMessageWindow.Handle);
            _hotkeyMessageWindow.SetHotkeyManager(_hotkeyManager); // Pass manager to message window
            _hotkeyManager.HotkeyPressed += HotkeyManager_HotkeyPressed;
            
            await LoadPresets();
            await RegisterSavedHotkeysAsync(); // Register hotkeys from loaded presets
            await UpdateTrayPresetMenu();
        }
        
        /// <summary>
        /// Registers hotkeys for all saved presets.
        /// </summary>
        private async Task RegisterSavedHotkeysAsync()
        {
            _logger.LogInfo("Registering saved hotkeys...");
            // Unregister any previously registered hotkeys to avoid conflicts
            _hotkeyManager.UnregisterAllHotkeys();
            _registeredPresetHotkeyIds.Clear();

            var allPresets = await _presetManager.GetAllPresetsAsync();

            foreach (var preset in allPresets)
            {
                if (!string.IsNullOrEmpty(preset.HotKey))
                {
                    try
                    {
                        var parts = preset.HotKey.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                        HotkeyModifiers modifiers = HotkeyModifiers.None;
                        Keys key = Keys.None;

                        foreach (var part in parts)
                        {
                            switch (part.ToLowerInvariant())
                            {
                                case "ctrl": modifiers |= HotkeyModifiers.Control; break;
                                case "alt": modifiers |= HotkeyModifiers.Alt; break;
                                case "shift": modifiers |= HotkeyModifiers.Shift; break;
                                case "win": modifiers |= HotkeyModifiers.Windows; break;
                                default:
                                    if (Enum.TryParse(part, true, out Keys parsedKey))
                                    {
                                        key = parsedKey;
                                    }
                                    break;
                            }
                        }

                        if (key != Keys.None)
                        {
                            var hotkeyId = _hotkeyManager.GenerateHotkeyId();
                            if (_hotkeyManager.RegisterHotkey(hotkeyId, modifiers, key))
                            {
                                _registeredPresetHotkeyIds[preset.Name] = hotkeyId;
                                _logger.LogInfo($"Successfully re-registered hotkey '{preset.HotKey}' for preset '{preset.Name}' with ID {hotkeyId}.");
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to re-register hotkey '{preset.HotKey}' for preset '{preset.Name}'. It might be in use by another application.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Could not parse key from hotkey string '{preset.HotKey}' for preset '{preset.Name}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error re-registering hotkey '{preset.HotKey}' for preset '{preset.Name}': {ex.Message}", ex);
                    }
                }
            }
            _logger.LogInfo("Finished registering saved hotkeys.");
        }

        private void SimpleMainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // If user clicks X button, minimize to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                MinimizeToTray();
                return;
            }
            
            // Clean up resources
            _hotkeyManager?.Dispose();
            _trayManager?.Dispose();
            _hotkeyMessageWindow?.Dispose(); // Dispose the message-only window
            _activationWindow?.Dispose();
        }

        /// <summary>
        /// Handle form resize event to minimize to tray when minimized
        /// </summary>
        private void SimpleMainForm_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        /// <summary>
        /// Minimize the form to system tray
        /// </summary>
        private void MinimizeToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// Restore the form from system tray
        /// </summary>
        private void RestoreFromTray()
        {
            try
            {
                this.ShowInTaskbar = true;
                if (!this.Visible)
                {
                    this.Show();
                }
                if (this.WindowState == FormWindowState.Minimized)
                {
                    NativeMethods.ShowWindow(this.Handle, NativeMethods.SW_RESTORE);
                }
                NativeMethods.AllowSetForegroundWindow(-1); // ASFW_ANY
                NativeMethods.SetForegroundWindow(this.Handle);
                this.Activate();
            }
            catch { /* best-effort */ }
        }

        #region Tray Manager Event Handlers

        /// <summary>
        /// Handle show main window event from tray
        /// </summary>
        private void TrayManager_ShowMainWindow(object? sender, EventArgs e)
        {
            RestoreFromTray();
        }

        /// <summary>
        /// Handle exit application event from tray
        /// </summary>
        private void TrayManager_ExitApplication(object? sender, EventArgs e)
        {
            // Actually exit the application
            _hotkeyManager?.Dispose();
            _trayManager?.Dispose();
            Application.Exit();
        }

        /// <summary>
        /// Handle apply preset event from tray
        /// </summary>
        private async void TrayManager_ApplyPreset(object? sender, string presetName)
        {
            try
            {
                var preset = await _presetManager.LoadPresetAsync(presetName);
                if (preset == null)
                {
                    _trayManager.ShowNotification("Preset Not Found", 
                        $"Preset '{presetName}' could not be loaded", 
                        ToolTipIcon.Error);
                    return;
                }

                var success = await _displayService.ApplyMonitorConfigurationAsync(preset);
                if (success)
                {
                    _trayManager.ShowNotification("Preset Applied", 
                        $"Successfully applied preset: {presetName}", 
                        ToolTipIcon.Info);
                }
                else
                {
                    _trayManager.ShowNotification("Preset Failed", 
                        $"Failed to apply preset: {presetName}", 
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                _trayManager.ShowNotification("Error", 
                    $"Error applying preset '{presetName}': {errorMsg}", 
                    ToolTipIcon.Error);
                
                // Log detailed error for debugging
                using (var logger = new FileLogger())
                {
                    logger.LogError($"Error applying preset '{presetName}' from tray", ex, "TrayApply");
                }
            }
        }

        #endregion
        
        private async void ButtonSavePreset_Click(object? sender, EventArgs e)
        {
            try
            {
                var presetName = ShowInputDialog("Save Preset", "Enter preset name:");
                if (string.IsNullOrWhiteSpace(presetName))
                {
                    return;
                }
                
                var currentMonitors = await _displayService.GetCurrentMonitorConfigurationAsync();
                var preset = new DisplayPreset
                {
                    Name = presetName,
                    Monitors = currentMonitors
                };
                
                await _presetManager.SavePresetAsync(preset);
                await LoadPresets();
                
                MessageBox.Show($"Preset '{presetName}' saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save preset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async void ButtonApplyPreset_Click(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset == null)
            {
                MessageBox.Show("Please select a preset to apply.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                // Validate preset first
                var validation = await _displayService.ValidatePresetAsync(selectedPreset);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Cannot apply preset '{selectedPreset.Name}':\n\n{validation.ErrorMessage}", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var success = await _displayService.ApplyMonitorConfigurationAsync(selectedPreset);
                
                if (success)
                {
                    MessageBox.Show($"Preset '{selectedPreset.Name}' has been applied successfully.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to apply preset '{selectedPreset.Name}'.\n\nThe display configuration could not be changed.", 
                        "Apply Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                
                MessageBox.Show($"Error applying preset '{selectedPreset.Name}':\n\n{errorMsg}\n\nPlease check that:\n" +
                    "• All monitors in the preset are currently connected\n" +
                    "• The display settings are supported by your hardware\n" +
                    "• No other applications are blocking display changes", 
                    "Apply Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Log detailed error for debugging
                using (var logger = new FileLogger())
                {
                    logger.LogError($"Error applying preset '{selectedPreset.Name}' from button", ex, "ButtonApply");
                }
            }
        }
        
        private async void ButtonDeletePreset_Click(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset == null)
            {
                MessageBox.Show("Please select a preset to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var result = MessageBox.Show($"Are you sure you want to delete preset '{selectedPreset.Name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _presetManager.DeletePresetAsync(selectedPreset.Name);
                    await LoadPresets();
                    MessageBox.Show($"Preset '{selectedPreset.Name}' deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete preset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private async void ButtonRenamePreset_Click(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset == null)
            {
                MessageBox.Show("Please select a preset to rename.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var newName = ShowInputDialog("Rename Preset", "Enter new name:", selectedPreset.Name);
            if (string.IsNullOrWhiteSpace(newName) || newName == selectedPreset.Name)
                return;
            
            try
            {
                await _presetManager.RenamePresetAsync(selectedPreset.Name, newName);
                await LoadPresets();
                MessageBox.Show($"Preset renamed to '{newName}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename preset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ListViewPresets_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            UpdatePresetDetails(selectedPreset);
            
            var hasSelection = selectedPreset != null;
            buttonApplyPreset.Enabled = hasSelection;
            buttonRenamePreset.Enabled = hasSelection;
            buttonDeletePreset.Enabled = hasSelection;
        }
        
        private void ListViewPresets_DoubleClick(object? sender, EventArgs e)
        {
            var selectedPreset = GetSelectedPreset();
            if (selectedPreset != null)
            {
                ButtonApplyPreset_Click(sender, e);
            }
        }
        
        private async Task LoadPresets()
        {
            try
            {
                listViewPresets.Items.Clear();
                var presets = await _presetManager.GetAllPresetsAsync();
                
                foreach (var preset in presets)
                {
                    var item = new ListViewItem(preset.Name);
                    item.SubItems.Add(preset.Monitors.Count.ToString());
                    item.SubItems.Add(string.IsNullOrEmpty(preset.HotKey) ? "None" : preset.HotKey);
                    item.SubItems.Add(preset.CreatedDate.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = preset;
                    
                    listViewPresets.Items.Add(item);
                }
                
                // Update tray menu with current presets
                await UpdateTrayPresetMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load presets: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update the tray preset menu with current presets
        /// </summary>
        private async Task UpdateTrayPresetMenu()
        {
            try
            {
                var presetNames = await _presetManager.GetPresetNamesAsync();
                _trayManager.UpdatePresetMenu(presetNames);
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as this is background operation
                _logger.LogError("Failed to update tray preset menu", ex);
            }
        }
        
        private DisplayPreset? GetSelectedPreset()
        {
            if (listViewPresets.SelectedItems.Count > 0)
            {
                return listViewPresets.SelectedItems[0].Tag as DisplayPreset;
            }
            return null;
        }
        
        private void UpdatePresetDetails(DisplayPreset? preset)
        {
            if (preset == null)
            {
                labelDetailsInfo.Text = "Select a preset to view details";
                return;
            }
            
            var details = $"Name: {preset.Name}\n\n";
            details += $"Created: {preset.CreatedDate:yyyy-MM-dd HH:mm}\n";
            details += $"Modified: {preset.ModifiedDate:yyyy-MM-dd HH:mm}\n";
            details += $"Hotkey: {(string.IsNullOrEmpty(preset.HotKey) ? "None" : preset.HotKey)}\n\n";
            details += $"Monitors ({preset.Monitors.Count}):\n";
            
            foreach (var monitor in preset.Monitors)
            {
                details += $"• {monitor.DeviceName}\n";
                details += $"  Resolution: {monitor.Width}x{monitor.Height}\n";
                details += $"  Position: ({monitor.PositionX}, {monitor.PositionY})\n";
                details += $"  Primary: {(monitor.IsPrimary ? "Yes" : "No")}\n";
                details += $"  Orientation: {monitor.Orientation}\n\n";
            }
            
            labelDetailsInfo.Text = details;
        }
        
        private string ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            using (var form = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var label = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
                var textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = defaultValue };
                var buttonOk = new Button() { Text = "OK", Left = 210, Width = 80, Top = 100, DialogResult = DialogResult.OK };
                var buttonCancel = new Button() { Text = "Cancel", Left = 300, Width = 80, Top = 100, DialogResult = DialogResult.Cancel };

                buttonOk.Click += (sender, e) => { form.Close(); };
                buttonCancel.Click += (sender, e) => { form.Close(); };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);

                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
            }
        }

        /// <summary>
        /// Handle hotkey settings menu item click
        /// </summary>
        private void HotkeySettingsMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                // Ensure HotkeyManager is properly initialized
                if (_hotkeyManager == null)
                {
                    MessageBox.Show("Hotkey manager is not initialized. Please restart the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var hotkeyForm = new Forms.HotkeySettingsForm(_presetManager, _hotkeyManager);
                if (hotkeyForm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadPresets();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open hotkey settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle general settings menu item click
        /// </summary>
        private void GeneralSettingsMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                using var settingsForm = new Forms.SettingsForm(new StartupManager());
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Override WndProc to handle optional legacy messages
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            const int WM_USER = 0x0400;
            if (m.Msg == WM_USER + 1)
            {
                RestoreFromTray();
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Handle hotkey pressed event from HotkeyManager
        /// </summary>
        private async void HotkeyManager_HotkeyPressed(object? sender, HotkeyPressedEventArgs e)
        {
            _logger.LogInfo($"HotkeyManager_HotkeyPressed event fired: ID={e.HotkeyId}, Modifiers={e.Modifiers}, Key={e.Key}");
            try
            {
                var preset = await _presetManager.GetPresetByHotkeyAsync(e.Modifiers, e.Key);
                if (preset != null)
                {
                    _logger.LogInfo($"Found preset '{preset.Name}' for hotkey. Applying configuration...");
                    await _displayService.ApplyMonitorConfigurationAsync(preset);
                    _logger.LogInfo($"Successfully applied preset '{preset.Name}' via hotkey.");
                }
                else
                {
                    _logger.LogWarning($"No preset found for hotkey: Modifiers={e.Modifiers}, Key={e.Key}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HotkeyManager_HotkeyPressed for hotkey ID {e.HotkeyId}", ex);
            }
        }
    }
}