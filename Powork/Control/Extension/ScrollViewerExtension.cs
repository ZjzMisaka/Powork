using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Powork.Control.Extension
{
    public static class ScrollViewerExtension
    {
        #region ScrollAtTopCommand
        public static readonly DependencyProperty ScrollAtTopCommandProperty =
            DependencyProperty.RegisterAttached(
                "ScrollAtTopCommand",
                typeof(ICommand),
                typeof(ScrollViewerExtension),
                new PropertyMetadata(null, OnScrollAtTopCommandChanged));

        public static void SetScrollAtTopCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(ScrollAtTopCommandProperty, value);
        }

        public static ICommand GetScrollAtTopCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(ScrollAtTopCommandProperty);
        }

        private static void OnScrollAtTopCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if (e.NewValue is ICommand)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                }
                else if (e.OldValue != null)
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                }
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && GetScrollAtTopCommand(scrollViewer) is ICommand command)
            {
                if (scrollViewer.VerticalOffset == 0 && e.Delta > 0)
                {
                    if (command.CanExecute(null))
                    {
                        command.Execute(null);
                    }
                }
            }
        }
        #endregion

        #region ScrollToEnd
        public static readonly DependencyProperty ScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToEnd",
                typeof(bool),
                typeof(ScrollViewerExtension),
                new PropertyMetadata(false, OnScrollToEndChanged));

        public static bool GetScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToEndProperty);
        }

        public static void SetScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToEndProperty, value);
        }

        private static void OnScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && (bool)e.NewValue)
            {
                scrollViewer.ScrollToEnd();
            }
        }
        #endregion

        #region IsAtBottom
        public static readonly DependencyProperty IsAtBottomProperty =
            DependencyProperty.RegisterAttached(
                "IsAtBottom",
                typeof(bool),
                typeof(ScrollViewerExtension),
                new PropertyMetadata(false));

        public static bool GetIsAtBottom(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAtBottomProperty);
        }

        public static void SetIsAtBottom(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAtBottomProperty, value);
        }

        public static readonly DependencyProperty MonitorScrollProperty =
            DependencyProperty.RegisterAttached(
                "MonitorScroll",
                typeof(bool),
                typeof(ScrollViewerExtension),
                new PropertyMetadata(false, OnMonitorScrollChanged));

        public static bool GetMonitorScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorScrollProperty);
        }

        public static void SetMonitorScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorScrollProperty, value);
        }

        private static void OnMonitorScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                bool isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;
                SetIsAtBottom(scrollViewer, isAtBottom);
            }
        }
        #endregion
    }
}
