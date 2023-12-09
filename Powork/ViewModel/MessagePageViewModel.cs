using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
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
using System.Windows.Input;

namespace Powork.ViewModel
{
    class MessagePageViewModel : ObservableObject
    {
        public ObservableCollection<User> UserList { get; set; }
        public string MessageText { get; set; }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MessagePageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);

            UserList =
            [
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
                new User() { Name = "11", IP = "192.170.12.15", GroupName = "osaka" },
            ];
            MessageText = "wfafefgrewgre";
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
    }
}
