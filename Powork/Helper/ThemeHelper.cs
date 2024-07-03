using System.Windows;
using System.Windows.Media;

namespace Powork.Helper
{
    public static class ThemeHelper
    {
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

        public static string PreBackgroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(45, 45, 45)";
                }
                else
                {
                    return "rgb(252, 252, 252)";
                }
            }
        }

        public static string PreEvenBackgroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(56, 56, 56)";
                }
                else
                {
                    return "rgb(247, 247, 247)";
                }
            }
        }

        public static string PreForegroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(248, 248, 242)";
                }
                else
                {
                    return "rgb(0, 0, 0)";
                }
            }
        }

        public static string TableHeaderBackgroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(68, 68, 68)";
                }
                else
                {
                    return "rgb(230, 230, 230)";
                }
            }
        }

        public static string TableHoverBackgroundColorText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return "rgb(85, 85, 85)";
                }
                else
                {
                    return "rgb(200, 200, 200)";
                }
            }
        }

        public static string ScrollbarStyleText
        {
            get
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark)
                {
                    return @"
                                scrollbar-base-color: #222;
                                scrollbar-3dlight-color: #222;
                                scrollbar-highlight-color: #222;
                                scrollbar-track-color: #3e3e42;
                                scrollbar-arrow-color: #111;
                                scrollbar-shadow-color: #222;
                                scrollbar-dark-shadow-color: #222; ";
                }
                else
                {
                    return "";
                }
            }
        }

        public static Brush SelectedBackgroundBrush
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["SelectedBackgroundBrush"];
            }
        }

        public static Brush SelectedForegroundBrush
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["SelectedForegroundBrush"];
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
