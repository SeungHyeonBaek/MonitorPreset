using System;
using System.Runtime.InteropServices;

namespace MonitorPresetManager.Services
{
    internal static class NativeMethods
    {
        #region Win32 API Declarations

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplayDevices(
            string? lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplaySettings(
            string? deviceName,
            int modeNum,
            ref DEVMODE devMode);

        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettings(
            ref DEVMODE devMode,
            uint flags);

        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettingsEx(
            string? lpszDeviceName,
            ref DEVMODE lpDevMode,
            IntPtr hwnd,
            uint dwflags,
            IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            IntPtr pvParam,
            uint fWinIni);
            
        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettings(IntPtr lpDevMode, uint dwFlags);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    internal static extern bool AllowSetForegroundWindow(int dwProcessId);

        #endregion

        #region Win32 Structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion

        #region Constants

        internal const int ENUM_CURRENT_SETTINGS = -1;
        internal const int CDS_UPDATEREGISTRY = 0x01;
        internal const int CDS_TEST = 0x02;
        internal const int CDS_NORESET = 0x10000000;
        internal const int CDS_SET_PRIMARY = 0x00000010;

        internal const int SW_RESTORE = 9;

    internal static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        internal const uint SPI_SETDESKWALLPAPER = 0x0014;
        internal const uint SPIF_UPDATEINIFILE = 0x01;
        internal const uint SPIF_SENDCHANGE = 0x02;

        internal const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
        internal const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;

        internal enum DISP_CHANGE
        {
            SUCCESSFUL = 0,
            RESTART = 1,
            FAILED = -1,
            BADMODE = -2,
            NOTUPDATED = -3,
            BADFLAGS = -4,
            BADPARAM = -5,
            BADDUALVIEW = -6
        }

        [Flags]
        internal enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x01,
            CDS_TEST = 0x02,
            CDS_FULLSCREEN = 0x04,
            CDS_GLOBAL = 0x08,
            CDS_SET_PRIMARY = 0x10,
            CDS_VIDEOPARAMETERS = 0x20,
            CDS_ENABLE_UNSAFE_MODES = 0x100,
            CDS_DISABLE_UNSAFE_MODES = 0x200,
            CDS_RESET = 0x40000000,
            CDS_NORESET = 0x10000000
        }

        #endregion
    }
}