using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MonitorPresetManager.Services;

namespace MonitorPresetManager
{
    internal static class Program
    {
        private const string AppGuid = "c6b6a8e2-9c1e-4a4b-8c8a-7e7b6f9a5b5d";
        private static Mutex? _mutex;
        internal static uint WM_SHOWFIRSTINSTANCE;

        [STAThread]
        static void Main(string[] args)
        {
            _mutex = new Mutex(true, AppGuid, out bool createdNew);
            WM_SHOWFIRSTINSTANCE = NativeMethods.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE_" + AppGuid);

            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                bool startMinimized = args.Any(a => string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase));
                Application.Run(new MainForm(startMinimized));
                _mutex.ReleaseMutex();
            }
            else
            {
                // Grant foreground rights to the existing instance (best-effort)
                try
                {
                    var currentProcess = Process.GetCurrentProcess();
                    var otherProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                                              .FirstOrDefault(p => p.Id != currentProcess.Id);
                    if (otherProcess != null)
                    {
                        NativeMethods.AllowSetForegroundWindow(otherProcess.Id);
                    }
                }
                catch { /* ignore */ }

                // Ask existing instance to show/activate itself and then exit
                NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
