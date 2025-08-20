using System;
using System.Windows.Forms;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// A hidden, message-only window used to receive global hotkey messages.
    /// This ensures hotkeys work reliably even when the main application window is minimized or hidden.
    /// It uses NativeWindow for a lightweight implementation.
    /// </summary>
    public class HotkeyMessageWindow : NativeWindow, IDisposable
    {
        private IHotkeyManager? _hotkeyManager;
        private readonly ILogger _logger;
        private bool _disposed;

        private const int HWND_MESSAGE = -3;

        public HotkeyMessageWindow()
        {
            _logger = new FileLogger();
            _logger.LogInfo("HotkeyMessageWindow constructor called. Creating handle.");
            
            // Create a message-only window by setting parent to HWND_MESSAGE
            var createParams = new CreateParams
            {
                Parent = (IntPtr)HWND_MESSAGE
            };
            
            CreateHandle(createParams);
            _logger.LogInfo($"HotkeyMessageWindow handle created: {this.Handle}");
        }

        /// <summary>
        /// Sets the HotkeyManager instance for message processing.
        /// </summary>
        /// <param name="hotkeyManager">The HotkeyManager instance.</param>
        public void SetHotkeyManager(IHotkeyManager hotkeyManager)
        {
            _hotkeyManager = hotkeyManager;
        }

        /// <summary>
        /// Processes Windows messages, passing hotkey messages to the HotkeyManager.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (_hotkeyManager != null)
            {
                if (_hotkeyManager.ProcessMessage(m.Msg, m.WParam, m.LParam))
                {
                    // Message was handled by the hotkey manager
                    return;
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Dispose of the window handle.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No managed resources to dispose here
                }

                // Destroy the window handle
                if (this.Handle != IntPtr.Zero)
                {
                    DestroyHandle();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~HotkeyMessageWindow()
        {
            Dispose(false);
        }
    }
}
