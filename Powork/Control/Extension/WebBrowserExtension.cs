using System.Windows;
using System.Windows.Controls;

namespace Powork.Control.Extension
{
    public class WebBrowserExtension
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(WebBrowserExtension), new UIPropertyMetadata(null, SourcePropertyChanged));

        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static void SourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = o as WebBrowser;
            if (browser != null)
            {
                string html = e.NewValue as string;
                if (!string.IsNullOrEmpty(html))
                {
                    browser.NavigateToString(html);
                }
                else
                {
                    browser.NavigateToString("<html></html>");
                }
            }
        }
    }
}
