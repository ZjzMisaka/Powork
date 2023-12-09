using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Helper;
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
        private ObservableCollection<User> userList;
        public ObservableCollection<User> UserList
        {
            get
            {
                return userList;
            }
            set
            {
                SetProperty<ObservableCollection<User>>(ref userList, value);
            }
        }
        private bool pageEnabled;
        public bool PageEnabled
        {
            get
            {
                return pageEnabled;
            }
            set
            {
                SetProperty<bool>(ref pageEnabled, value);
            }
        }
        private string messageText;
        public string MessageText
        {
            get
            {
                return messageText;
            }
            set
            {
                SetProperty<string>(ref messageText, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MessagePageViewModel()
        {
            PageEnabled = true;

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
        }

        private void WindowClosed()
        {
        }
    }
}
