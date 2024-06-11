using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Powork.Helper
{
    public static class ScrollViewerExtensions
    {
        public static readonly DependencyProperty ScrollAtTopCommandProperty =
            DependencyProperty.RegisterAttached(
                "ScrollAtTopCommand",
                typeof(ICommand),
                typeof(ScrollViewerExtensions),
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
    }
}
