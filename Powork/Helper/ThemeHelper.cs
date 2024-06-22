using System.Windows;
using System.Windows.Media;

namespace Powork.Helper
{
    public static class ThemeHelper
    {
        public static Brush TimeTextBrush
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return Brushes.LightGreen;
                }
                else
                {
                    return Brushes.DarkGreen;
                }
            }
        }

        public static Brush ErrorTextBrush
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return Brushes.Pink;
                }
                else
                {
                    return Brushes.DarkRed;
                }
            }
        }

        public static string BackgroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(57, 57, 57)";
                }
                else
                {
                    return "rgb(255, 255, 255)";
                }
            }
        }

        public static string ForegroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(255, 255, 255)";
                }
                else
                {
                    return "rgb(0, 0, 0)";
                }
            }
        }

        public static Brush SelectedBackgroundBrush
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["SelectedBrush"];
            }
        }

        public static Brush SelectedForegroundBrush
        {
            get
            {
                return Brushes.White;
            }
        }

        public static Brush ForegroundBrush
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }
        }
    }
}
