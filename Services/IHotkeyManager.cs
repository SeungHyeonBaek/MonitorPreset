using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    /// <summary>
    /// Interface for global hotkey management
    /// </summary>
    public interface IHotkeyManager : IDisposable
    {
        /// <summary>
        /// Event fired when a registered hotkey is pressed
        /// </summary>
        event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

        /// <summary>
        /// Register a global hotkey
        /// </summary>
        /// <param name="id">Unique identifier for the hotkey</param>
        /// <param name="modifiers">Modifier keys (Ctrl, Alt, Shift, Win)</param>
        /// <param name="key">Main key</param>
        /// <returns>True if registration was successful</returns>
        bool RegisterHotkey(int id, HotkeyModifiers modifiers, Keys key);

        /// <summary>
        /// Unregister a global hotkey
        /// </summary>
        /// <param name="id">Unique identifier for the hotkey</param>
        /// <returns>True if unregistration was successful</returns>
        bool UnregisterHotkey(int id);

        /// <summary>
        /// Check if a hotkey is currently registered
        /// </summary>
        /// <param name="id">Hotkey identifier</param>
        /// <returns>True if hotkey is registered</returns>
        bool IsHotkeyRegistered(int id);

        /// <summary>
        /// Get all registered hotkey IDs
        /// </summary>
        /// <returns>List of registered hotkey IDs</returns>
        List<int> GetRegisteredHotkeyIds();

        /// <summary>
        /// Get all registered hotkeys
        /// </summary>
        /// <returns>Dictionary of registered hotkeys</returns>
        Dictionary<int, HotkeyInfo> GetRegisteredHotkeys();

        /// <summary>
        /// Unregister all hotkeys
        /// </summary>
        void UnregisterAllHotkeys();

        /// <summary>
        /// Generate a unique hotkey ID
        /// </summary>
        /// <returns>Unique hotkey ID</returns>
        int GenerateHotkeyId();

        /// <summary>
        /// Check if a hotkey combination is available (not already registered by another application)
        /// </summary>
        /// <param name="modifiers">Modifier keys</param>
        /// <param name="key">Main key</param>
        /// <returns>True if hotkey combination is available</returns>
        bool IsHotkeyAvailable(HotkeyModifiers modifiers, Keys key);

        /// <summary>
        /// Process Windows messages to handle hotkey events
        /// </summary>
        /// <param name="msg">Windows message</param>
        /// <param name="wParam">Message wParam</param>
        /// <param name="lParam">Message lParam</param>
        /// <returns>True if message was handled</returns>
        bool ProcessMessage(int msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>
    /// Stores information about a registered hotkey
    /// </summary>
    public class HotkeyInfo
    {
        public int Id { get; }
        public HotkeyModifiers Modifiers { get; }
        public Keys Key { get; }

        public HotkeyInfo(int id, HotkeyModifiers modifiers, Keys key)
        {
            Id = id;
            Modifiers = modifiers;
            Key = key;
        }
    }

    /// <summary>
    /// Event arguments for hotkey pressed event
    /// </summary>
    public class HotkeyPressedEventArgs : EventArgs
    {
        /// <summary>
        /// Hotkey identifier
        /// </summary>
        public int HotkeyId { get; }

        /// <summary>
        /// Modifier keys that were pressed
        /// </summary>
        public HotkeyModifiers Modifiers { get; }

        /// <summary>
        /// Main key that was pressed
        /// </summary>
        public Keys Key { get; }

        /// <summary>
        /// Initialize hotkey pressed event arguments
        /// </summary>
        /// <param name="hotkeyId">Hotkey identifier</param>
        /// <param name="modifiers">Modifier keys</param>
        /// <param name="key">Main key</param>
        public HotkeyPressedEventArgs(int hotkeyId, HotkeyModifiers modifiers, Keys key)
        {
            HotkeyId = hotkeyId;
            Modifiers = modifiers;
            Key = key;
        }
    }
}