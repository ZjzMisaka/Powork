using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Powork.Helper
{
    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool IsWindowActive(Window window)
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                return false;
            }
            GetWindowThreadProcessId(foregroundWindow, out uint processID);

            int currentProcessID = Process.GetCurrentProcess().Id;
            return window.WindowState != WindowState.Minimized &&
                window.Visibility == Visibility.Visible &&
                 processID == currentProcessID;
        }
    }
}
