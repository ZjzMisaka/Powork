using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Powork.Helper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;
    }

    public class FlashHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        private static void FlashWindow(IntPtr hWnd, uint count)
        {
            FLASHWINFO fw = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = hWnd,
                dwFlags = FLASHWINFO.FLASHW_ALL | FLASHWINFO.FLASHW_TIMERNOFG,
                uCount = count,
                dwTimeout = 0
            };
            FlashWindowEx(ref fw);
        }

        public static void FlashTaskbarIcon()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                if (!WindowHelper.IsWindowActive(mainWindow))
                {
                    IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
                    FlashWindow(hWnd, uint.MaxValue);
                }
            }, DispatcherPriority.Normal);
        }
    }
}
