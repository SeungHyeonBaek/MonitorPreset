using System;
using System.Windows.Forms;

namespace MonitorPresetManager.Services
{
	/// <summary>
	/// Hidden top-level window to receive activation messages even when the main form is hidden.
	/// </summary>
	internal sealed class ActivationMessageWindow : NativeWindow, IDisposable
	{
		private readonly Action _onActivate;
		private bool _disposed;

		public ActivationMessageWindow(Action onActivate)
		{
			_onActivate = onActivate ?? throw new ArgumentNullException(nameof(onActivate));

			var cp = new CreateParams
			{
				Caption = "MonitorPresetManager_ActivationWindow",
				X = 0,
				Y = 0,
				Width = 0,
				Height = 0,
				Style = unchecked((int)0x80000000), // WS_DISABLED (prevents focus), window remains hidden
				ExStyle = 0x00000080 // WS_EX_TOOLWINDOW (no taskbar)
			};

			CreateHandle(cp);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Program.WM_SHOWFIRSTINSTANCE)
			{
				try
				{
					// Grant any process foreground rights, then call activation
					NativeMethods.AllowSetForegroundWindow(-1);
					_onActivate();
				}
				catch { /* best-effort */ }
				return;
			}

			base.WndProc(ref m);
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			try { DestroyHandle(); } catch { /* ignore */ }
			GC.SuppressFinalize(this);
		}
	}
}
