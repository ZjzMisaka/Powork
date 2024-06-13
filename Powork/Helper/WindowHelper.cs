using System.Windows;

namespace Powork.Helper
{
    public static class WindowHelper
    {
        public static bool IsWindowActive(Window window)
        {
            return window.WindowState != WindowState.Minimized && window.Visibility == Visibility.Visible;
        }
    }
}
