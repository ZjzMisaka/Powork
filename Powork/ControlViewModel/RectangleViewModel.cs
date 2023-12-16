using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using PowerThreadPool.EventArguments;

namespace Powork.ControlViewModel
{
    class RectangleViewModel : ObservableObject
    {
        private bool isDragging;
        private bool isResizing;
        private Point clickPosition;

        public delegate void RemoveEventHandler(RectangleViewModel sender);
        public event RemoveEventHandler Remove;

        private double x;
        public double X
        {
            get { return x; }
            set
            {
                SetProperty<double>(ref x, value);
            }
        }

        private double y;
        public double Y
        {
            get { return y; }
            set
            {
                SetProperty<double>(ref y, value);
            }
        }

        private double rectangleWidth;
        public double RectangleWidth
        {
            get { return rectangleWidth; }
            set
            {
                SetProperty<double>(ref rectangleWidth, value);
            }
        }

        private double rectangleHeight;
        public double RectangleHeight
        {
            get { return rectangleHeight; }
            set
            {
                SetProperty<double>(ref rectangleHeight, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand MouseLeftButtonDownCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseLeftButtonUpCommand { get; }
        public ICommand MouseRightButtonUpCommand { get; }

        public RectangleViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            MouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(MouseLeftButtonDown);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(MouseMove);
            MouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(MouseLeftButtonUp);
            MouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(MouseRightButtonUp);

            RectangleWidth = 100;
            RectangleHeight = 100;
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
        }

        private void WindowClosed()
        {
        }

        private void MouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var rectangle = e.Source as FrameworkElement;
            Point _startPoint = e.GetPosition(rectangle);

            // TODO Click position
            isResizing = _startPoint.X >= RectangleWidth - 10 && _startPoint.Y >= RectangleHeight - 10;
            if (!isResizing)
            {
                isDragging = true;
            }

            Mouse.Capture(rectangle);
        }

        private void MouseMove(MouseEventArgs e)
        {
            var rectangle = e.Source as FrameworkElement;
            var currentPoint = e.GetPosition(rectangle);

            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(rectangle.Parent as UIElement);

                var newX = currentPosition.X - clickPosition.X;
                var newY = currentPosition.Y - clickPosition.Y;

                X= newX;
                Y = newY;

                var transform = rectangle.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    rectangle.RenderTransform = transform;
                }

                transform.X = currentPosition.X - clickPosition.X - rectangleWidth / 2;
                transform.Y = currentPosition.Y - clickPosition.Y - rectangleHeight / 2;
            }
            else if (isResizing)
            {
                RectangleWidth = currentPoint.X + 10;
                RectangleHeight = currentPoint.Y + 10;
            }
        }

        private void MouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var rectangle = e.Source as FrameworkElement;
            isDragging = false;
            isResizing = false;
            rectangle.ReleaseMouseCapture();
        }

        private void MouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (Remove != null)
            {
                Remove.Invoke(this);
            }
        }
    }
}
