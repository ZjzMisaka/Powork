using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Powork.Control.Extension
{
    public class RichTextBoxExtension
    {
        #region Document
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                "Document",
                typeof(FlowDocument),
                typeof(RichTextBoxExtension),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)d;

            richTextBox.Document = (FlowDocument)e.NewValue;
        }

        public static FlowDocument GetDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentProperty);
        }

        public static void SetDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentProperty, value);
        }
        #endregion

        #region ScrollToEnd
        public static readonly DependencyProperty ScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToEnd",
                typeof(bool),
                typeof(RichTextBoxExtension),
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
            if (d is RichTextBox richTextBox && (bool)e.NewValue)
            {
                ScrollViewer scrollViewer = GetScrollViewer(richTextBox);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }

        public static readonly DependencyProperty MonitorScrollProperty =
            DependencyProperty.RegisterAttached(
                "MonitorScroll",
                typeof(bool),
                typeof(RichTextBoxExtension),
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
            if (d is RichTextBox richTextBox)
            {
                ScrollViewer scrollViewer = GetScrollViewer(richTextBox);
                if (scrollViewer != null)
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

        #region IsScrollAtBottom
        public static readonly DependencyProperty IsAtBottomProperty =
            DependencyProperty.RegisterAttached(
                "IsAtBottom",
                typeof(bool),
                typeof(RichTextBoxExtension),
                new PropertyMetadata(false));

        public static bool GetIsAtBottom(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAtBottomProperty);
        }

        public static void SetIsAtBottom(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAtBottomProperty, value);
        }

        private static ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            if (obj is RichTextBox richTextBox)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(richTextBox); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(richTextBox, i);
                    if (child is ScrollViewer)
                    {
                        return (ScrollViewer)child;
                    }
                    else
                    {
                        ScrollViewer result = GetScrollViewer(child);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
