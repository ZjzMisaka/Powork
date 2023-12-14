using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Control;
using Powork.Model;
using Powork.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Powork.ViewModel
{
    class TestingPageViewModel : ObservableObject
    {
        private BitmapSource bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return bitmapSource; }
            set
            {
                SetProperty<BitmapSource>(ref bitmapSource, value);
            }
        }

        private ObservableCollection<UserControl> shapeItems;
        public ObservableCollection<UserControl> ShapeItems
        {
            get { return shapeItems; }
            set
            {
                SetProperty<ObservableCollection<UserControl>>(ref shapeItems, value);
            }
        }

        private ObservableCollection<UserControl> outerItems;
        public ObservableCollection<UserControl> OuterItems
        {
            get { return outerItems; }
            set
            {
                SetProperty<ObservableCollection<UserControl>>(ref outerItems, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand SetImageCommand { get; set; }
        public ICommand AddEmptyRectangleCommand { get; set; }

        public TestingPageViewModel()
        {
            ShapeItems = new ObservableCollection<UserControl>();
            OuterItems = new ObservableCollection<UserControl>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SetImageCommand = new RelayCommand(SetImage);
            AddEmptyRectangleCommand = new RelayCommand(AddEmptyRectangle);
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

        private void SetImage()
        {
            BitmapSource = Clipboard.GetImage();
        }

        private void AddEmptyRectangle()
        {
            ShapeItems.Add(new Rectangle());
        }
    }
}
