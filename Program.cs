using System;
using System.Threading;
using System.Windows.Forms;

namespace MonitorPresetManager
{
    internal static class Program
    {
        private const string AppGuid = "c6b6a8e2-9c1e-4a4b-8c8a-7e7b6f9a5b5d";
        private static Mutex? _mutex;

        [STAThread]
        static void Main(string[] args)
        {
            _mutex = new Mutex(true, AppGuid, out bool createdNew);

            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                _mutex.ReleaseMutex();
            }
            else
            {
                // Another instance is already running. Show a notification and exit.
                MessageBox.Show("Monitor Preset Manager is already running.", "Application Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
