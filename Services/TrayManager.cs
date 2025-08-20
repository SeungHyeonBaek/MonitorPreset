using System.Drawing;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Manager for system tray functionality
    /// </summary>
    public class TrayManager : ITrayManager
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly ToolStripMenuItem _showMenuItem;
        private readonly ToolStripMenuItem _presetsMenuItem;
        private readonly ToolStripSeparator _separator1;
        private readonly ToolStripMenuItem _exitMenuItem;
        private bool _disposed;

        #region Events

        /// <summary>
        /// Event fired when user wants to show the main window
        /// </summary>
        public event EventHandler? ShowMainWindow;

        /// <summary>
        /// Event fired when user wants to exit the application
        /// </summary>
        public event EventHandler? ExitApplication;

        /// <summary>
        /// Event fired when user wants to apply a preset from tray menu
        /// </summary>
        public event EventHandler<string>? ApplyPreset;

        #endregion

        /// <summary>
        /// Initialize TrayManager
        /// </summary>
        public TrayManager()
        {
            // Create context menu items
            _showMenuItem = new ToolStripMenuItem("Show Monitor Preset Manager");
            _presetsMenuItem = new ToolStripMenuItem("Apply Preset");
            _separator1 = new ToolStripSeparator();
            _exitMenuItem = new ToolStripMenuItem("Exit");

            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.AddRange(new ToolStripItem[]
            {
                _showMenuItem,
                _presetsMenuItem,
                _separator1,
                _exitMenuItem
            });

            // Create notify icon
            _notifyIcon = new NotifyIcon()
            {
                ContextMenuStrip = _contextMenu,
                Text = "Monitor Preset Manager",
                Visible = false
            };

            // Set icon from resources
            _notifyIcon.Icon = LoadIconFromResources();

            // Wire up event handlers
            InitializeEventHandlers();
        }

        /// <summary>
        /// Initialize event handlers
        /// </summary>
        private void InitializeEventHandlers()
        {
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            _showMenuItem.Click += ShowMenuItem_Click;
            _exitMenuItem.Click += ExitMenuItem_Click;
        }

        /// <summary>
        /// Load icon from embedded resources
        /// </summary>
        private Icon LoadIconFromResources()
        {
            try
            {
                // Try to load the new monitor icon from resources
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "MonitorPresetManager.Resources.monitor-icon.ico";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                }
            }
            catch
            {
                // Fall back to default icon if loading fails
            }
            
            // Fallback to programmatically created icon
            return CreateDefaultIcon();
        }

        /// <summary>
        /// Create a default icon for the tray (fallback)
        /// </summary>
        private Icon CreateDefaultIcon()
        {
            // Create a 16x16 icon with single monitor and large refresh symbol
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                using (var monitorBrush = new SolidBrush(Color.FromArgb(148, 163, 184))) // Light slate
                using (var screenBrush = new SolidBrush(Color.FromArgb(71, 85, 105))) // Dark slate
                using (var refreshBrush = new SolidBrush(Color.White))
                using (var standBrush = new SolidBrush(Color.FromArgb(100, 116, 139))) // Medium slate
                {
                    // Main monitor frame
                    graphics.FillRectangle(monitorBrush, 2, 2, 12, 9);
                    
                    // Monitor screen
                    graphics.FillRectangle(screenBrush, 3, 3, 10, 7);
                    
                    // Monitor stand
                    graphics.FillRectangle(standBrush, 6, 11, 4, 2);
                    graphics.FillRectangle(standBrush, 4, 13, 8, 1);
                    
                    // Large circular refresh arrows
                    var centerX = 8;
                    var centerY = 6;
                    var radius = 2;
                    
                    using (var pen = new Pen(refreshBrush, 1.5f))
                    {
                        // Top arc (clockwise)
                        graphics.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, -30, 200);
                        // Bottom arc (clockwise)
                        graphics.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, 150, 200);
                    }
                    
                    // Arrow heads for refresh symbol
                    
                    // Top arrow head (pointing right)
                    graphics.FillPolygon(refreshBrush, new PointF[]
                    {
                        new PointF(centerX + radius - 0.5f, centerY - 1),
                        new PointF(centerX + radius + 0.5f, centerY - 0.5f),
                        new PointF(centerX + radius - 0.5f, centerY)
                    });
                    
                    // Bottom arrow head (pointing left)
                    graphics.FillPolygon(refreshBrush, new PointF[]
                    {
                        new PointF(centerX - radius + 0.5f, centerY + 1),
                        new PointF(centerX - radius - 0.5f, centerY + 0.5f),
                        new PointF(centerX - radius + 0.5f, centerY)
                    });
                }
            }

            // Convert bitmap to icon
            var iconHandle = bitmap.GetHicon();
            return Icon.FromHandle(iconHandle);
        }

        #region ITrayManager Implementation

        /// <summary>
        /// Show the tray icon
        /// </summary>
        public void ShowTrayIcon()
        {
            if (!_disposed)
            {
                _notifyIcon.Visible = true;
            }
        }

        /// <summary>
        /// Hide the tray icon
        /// </summary>
        public void HideTrayIcon()
        {
            if (!_disposed)
            {
                _notifyIcon.Visible = false;
            }
        }

        /// <summary>
        /// Check if tray icon is visible
        /// </summary>
        public bool IsVisible => !_disposed && _notifyIcon.Visible;

        /// <summary>
        /// Update the preset list in the tray context menu
        /// </summary>
        /// <param name="presetNames">List of preset names</param>
        public void UpdatePresetMenu(List<string> presetNames)
        {
            if (_disposed)
                return;

            // Clear existing preset menu items
            _presetsMenuItem.DropDownItems.Clear();

            if (presetNames == null || presetNames.Count == 0)
            {
                var noPresetsItem = new ToolStripMenuItem("No presets available")
                {
                    Enabled = false
                };
                _presetsMenuItem.DropDownItems.Add(noPresetsItem);
                return;
            }

            // Add preset menu items
            foreach (var presetName in presetNames)
            {
                var presetItem = new ToolStripMenuItem(presetName);
                presetItem.Click += (sender, e) => ApplyPreset?.Invoke(this, presetName);
                _presetsMenuItem.DropDownItems.Add(presetItem);
            }
        }

        /// <summary>
        /// Show a balloon tip notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="text">Notification text</param>
        /// <param name="icon">Notification icon type</param>
        public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (_disposed || !_notifyIcon.Visible)
                return;

            try
            {
                _notifyIcon.ShowBalloonTip(3000, title, text, icon);
            }
            catch
            {
                // Ignore notification errors
            }
        }

        /// <summary>
        /// Set the tray icon tooltip text
        /// </summary>
        /// <param name="text">Tooltip text</param>
        public void SetTooltipText(string text)
        {
            if (_disposed)
                return;

            _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle tray icon double-click
        /// </summary>
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowMainWindow?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handle show menu item click
        /// </summary>
        private void ShowMenuItem_Click(object? sender, EventArgs e)
        {
            ShowMainWindow?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handle exit menu item click
        /// </summary>
        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            ExitApplication?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method
        /// </summary>
        /// <param name="disposing">Whether disposing from Dispose method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _notifyIcon?.Dispose();
                    _contextMenu?.Dispose();
                    _showMenuItem?.Dispose();
                    _presetsMenuItem?.Dispose();
                    _separator1?.Dispose(); 
                    _exitMenuItem?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~TrayManager()
        {
            Dispose(false);
        }

        #endregion
    }
}