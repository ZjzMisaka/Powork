using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Model;
using Powork.Network;
using Powork.ViewModel.Inner;
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
    class SharePageViewModel : ObservableObject
    {
        private string userName;
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                SetProperty<string>(ref userName, value);
            }
        }
        private ObservableCollection<ShareInfo> shareInfoList;
        public ObservableCollection<ShareInfo> ShareInfoList
        {
            get
            {
                return shareInfoList;
            }
            set
            {
                SetProperty<ObservableCollection<ShareInfo>>(ref shareInfoList, value);
            }
        }
        
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand ShareCommand { get; set; }
        public SharePageViewModel(User user)
        {
            ShareInfoList = new ObservableCollection<ShareInfo>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            ShareCommand = new RelayCommand(Share);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
        }

        private void Share()
        {
        }
    }
}
