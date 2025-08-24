using MonitorPresetManager.Models;
using System.Runtime.InteropServices;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Manager for global hotkey registration and handling
    /// </summary>
    public class HotkeyManager : IHotkeyManager
    {
        #region Win32 API Declarations

        /// <summary>
        /// Registers a hot key with Windows
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        /// <summary>
        /// Unregisters a hot key that was previously registered
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        #region Constants

        // Windows message for hotkey
        private const int WM_HOTKEY = 0x0312;

        // Modifier key constants
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        #endregion

        #region Fields

        private readonly IntPtr _windowHandle;
        private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys;
        private readonly Random _random;
        private readonly ILogger _logger;
        private bool _disposed;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a registered hotkey is pressed
        /// </summary>
        public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize HotkeyManager with window handle for message processing
        /// </summary>
        /// <param name="windowHandle">Handle to window that will receive hotkey messages</param>
        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _registeredHotkeys = new Dictionary<int, HotkeyInfo>();
            _random = new Random();
            _logger = new FileLogger();
        }

        #endregion

        #region IHotkeyManager Implementation

        /// <summary>
        /// Register a global hotkey
        /// </summary>
        /// <param name="id">Unique identifier for the hotkey</param>
        /// <param name="modifiers">Modifier keys (Ctrl, Alt, Shift, Win)</param>
        /// <param name="key">Main key</param>
        /// <returns>True if registration was successful</returns>
        public bool RegisterHotkey(int id, HotkeyModifiers modifiers, Keys key)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HotkeyManager));
            }

            if (_registeredHotkeys.ContainsKey(id))
            {
                throw new ArgumentException($"Hotkey with ID {id} is already registered", nameof(id));
            }

            try
            {
                var winModifiers = ConvertToWinModifiers(modifiers);
                var virtualKey = (uint)key;

                _logger.LogInfo($"Attempting to register hotkey: ID={id}, Modifiers={modifiers}, Key={key}, WindowHandle={_windowHandle}");
                var success = RegisterHotKey(_windowHandle, id, winModifiers, virtualKey);
                
                if (success)
                {
                    _registeredHotkeys[id] = new HotkeyInfo(id, modifiers, key);
                    _logger.LogInfo($"Successfully registered hotkey: ID={id}, WindowHandle={_windowHandle}");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error(); // Get last Win32 error
                    _logger.LogError($"Failed to register hotkey: ID={id}, WindowHandle={_windowHandle}. Win32 Error: {error}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while registering hotkey: ID={id}", ex);
                return false;
            }
        }

        /// <summary>
        /// Unregister a global hotkey
        /// </summary>
        /// <param name="id">Unique identifier for the hotkey</param>
        /// <returns>True if unregistration was successful</returns>
        public bool UnregisterHotkey(int id)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HotkeyManager));
            }

            if (!_registeredHotkeys.ContainsKey(id))
            {
                return false;
            }

            try
            {
                _logger.LogInfo($"Attempting to unregister hotkey: ID={id}, WindowHandle={_windowHandle}");
                var success = UnregisterHotKey(_windowHandle, id);
                
                if (success)
                {
                    _registeredHotkeys.Remove(id);
                    _logger.LogInfo($"Successfully unregistered hotkey: ID={id}, WindowHandle={_windowHandle}");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error(); // Get last Win32 error
                    _logger.LogError($"Failed to unregister hotkey: ID={id}, WindowHandle={_windowHandle}. Win32 Error: {error}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while unregistering hotkey: ID={id}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if a hotkey is currently registered
        /// </summary>
        /// <param name="id">Hotkey identifier</param>
        /// <returns>True if hotkey is registered</returns>
        public bool IsHotkeyRegistered(int id)
        {
            return _registeredHotkeys.ContainsKey(id);
        }

        /// <summary>
        /// Get all registered hotkey IDs
        /// </summary>
        /// <returns>List of registered hotkey IDs</returns>
        public List<int> GetRegisteredHotkeyIds()
        {
            return _registeredHotkeys.Keys.ToList();
        }

        /// <summary>
        /// Get all registered hotkeys
        /// </summary>
        /// <returns>Dictionary of registered hotkeys</returns>
        public Dictionary<int, HotkeyInfo> GetRegisteredHotkeys()
        {
            return new Dictionary<int, HotkeyInfo>(_registeredHotkeys);
        }

        /// <summary>
        /// Unregister all hotkeys
        /// </summary>
        public void UnregisterAllHotkeys()
        {
            if (_disposed)
            {
                return;
            }

            var hotkeyIds = _registeredHotkeys.Keys.ToList();
            foreach (var id in hotkeyIds)
            {
                UnregisterHotkey(id);
            }
        }

        /// <summary>
        /// Generate a unique hotkey ID
        /// </summary>
        /// <returns>Unique hotkey ID</returns>
        public int GenerateHotkeyId()
        {
            int id;
            do
            {
                id = _random.Next(1, int.MaxValue);
            } while (_registeredHotkeys.ContainsKey(id));

            return id;
        }

        /// <summary>
        /// Check if a hotkey combination is available (not already registered by another application)
        /// </summary>
        /// <param name="modifiers">Modifier keys</param>
        /// <param name="key">Main key</param>
        /// <returns>True if hotkey combination is available</returns>
        public bool IsHotkeyAvailable(HotkeyModifiers modifiers, Keys key)
        {
            if (_disposed)
            {
                return false;
            }

            // Check if window handle is valid
            if (_windowHandle == IntPtr.Zero)
            {
                // If no valid window handle, assume hotkey is available
                // This is a fallback for when the form isn't fully initialized
                return true;
            }

            // Check if this exact combination is already registered by us
            foreach (var registeredHotkey in _registeredHotkeys.Values)
            {
                if (registeredHotkey.Modifiers == modifiers && registeredHotkey.Key == key)
                {
                    return false; // Already registered by us
                }
            }

            // Generate a temporary ID for testing
            var testId = GenerateHotkeyId();
            
            try
            {
                var winModifiers = ConvertToWinModifiers(modifiers);
                var virtualKey = (uint)key;

                // Try to register the hotkey temporarily
                var success = RegisterHotKey(_windowHandle, testId, winModifiers, virtualKey);
                
                if (success)
                {
                    // Immediately unregister it
                    UnregisterHotKey(_windowHandle, testId);
                    return true;
                }
                else
                {
                    // Registration failed, hotkey is likely in use
                    return false;
                }
            }
            catch (Exception)
            {
                // If there's an exception, assume it's not available
                return false;
            }
        }

        #endregion

        #region Message Processing

        /// <summary>
        /// Process Windows messages to handle hotkey events
        /// Call this method from your window's WndProc method
        /// </summary>
        /// <param name="msg">Windows message</param>
        /// <param name="wParam">Message wParam</param>
        /// <param name="lParam">Message lParam</param>
        /// <returns>True if message was handled</returns>
        public bool ProcessMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                _logger.LogInfo($"WM_HOTKEY message received for ID: {hotkeyId}");
                
                if (_registeredHotkeys.TryGetValue(hotkeyId, out var hotkeyInfo))
                {
                    _logger.LogInfo($"Hotkey {hotkeyId} ({hotkeyInfo.Modifiers} + {hotkeyInfo.Key}) pressed.");
                    // Fire the hotkey pressed event
                    HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(
                        hotkeyInfo.Id, 
                        hotkeyInfo.Modifiers, 
                        hotkeyInfo.Key));
                    
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Received WM_HOTKEY for unregistered ID: {hotkeyId}");
                }
            }

            return false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convert HotkeyModifiers enum to Windows modifier flags
        /// </summary>
        /// <param name="modifiers">HotkeyModifiers enum value</param>
        /// <returns>Windows modifier flags</returns>
        private static uint ConvertToWinModifiers(HotkeyModifiers modifiers)
        {
            uint winModifiers = 0;

            if (modifiers.HasFlag(HotkeyModifiers.Alt))
                winModifiers |= MOD_ALT;

            if (modifiers.HasFlag(HotkeyModifiers.Control))
                winModifiers |= MOD_CONTROL;

            if (modifiers.HasFlag(HotkeyModifiers.Shift))
                winModifiers |= MOD_SHIFT;

            if (modifiers.HasFlag(HotkeyModifiers.Windows))
                winModifiers |= MOD_WIN;

            return winModifiers;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose of resources and unregister all hotkeys
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
                    UnregisterAllHotkeys();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure hotkeys are unregistered
        /// </summary>
        ~HotkeyManager()
        {
            Dispose(false);
        }

        #endregion

        
    }
}