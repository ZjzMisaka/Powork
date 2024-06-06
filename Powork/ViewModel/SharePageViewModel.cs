using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Model;
using Powork.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Powork.ViewModel
{
    class SharePageViewModel : ObservableObject
    {
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public SharePageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
        }
    }
}
