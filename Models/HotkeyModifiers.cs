namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Enumeration for hotkey modifier keys
    /// </summary>
    [Flags]
    public enum HotkeyModifiers : uint
    {
        /// <summary>
        /// No modifier keys
        /// </summary>
        None = 0,

        /// <summary>
        /// Alt key modifier
        /// </summary>
        Alt = 1,

        /// <summary>
        /// Control key modifier
        /// </summary>
        Control = 2,

        /// <summary>
        /// Shift key modifier
        /// </summary>
        Shift = 4,

        /// <summary>
        /// Windows key modifier
        /// </summary>
        Windows = 8
    }
}