using MonitorPresetManager.Models;

namespace MonitorPresetManager.Services
{
    public static class HotkeyHelper
    {
        public static string GetHotkeyString(HotkeyModifiers modifiers, Keys key)
        {
            if (key == Keys.None)
            {
                return "None";
            }

            var parts = new List<string>();

            if (modifiers.HasFlag(HotkeyModifiers.Control))
            {
                parts.Add("Ctrl");
            }

            if (modifiers.HasFlag(HotkeyModifiers.Alt))
            {
                parts.Add("Alt");
            }

            if (modifiers.HasFlag(HotkeyModifiers.Shift))
            {
                parts.Add("Shift");
            }

            if (modifiers.HasFlag(HotkeyModifiers.Windows))
            {
                parts.Add("Win");
            }

            parts.Add(key.ToString());

            return string.Join(" + ", parts);
        }
    }
}
