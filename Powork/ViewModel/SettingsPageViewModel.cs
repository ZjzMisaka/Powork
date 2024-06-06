using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Powork.ViewModel
{
    class SettingsPageViewModel : ObservableObject
    {
        private string ip;
        public string IP 
        {
            get
            {
                return ip;
            }
            set
            {
                SetProperty<string>(ref ip, value);
            }
        }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetProperty<string>(ref name, value);
            }
        }
        private string group;
        public string Group
        {
            get
            {
                return group;
            }
            set
            {
                SetProperty<string>(ref group, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public SettingsPageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            OKClickCommand = new RelayCommand(OKClick);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            IP = GlobalVariables.LocalIP.ToString();

            List<User> userList = UserRepository.SelectUserByIp(IP);
            if (userList.Count == 1)
            { 
                Name = userList[0].Name;
                Group = userList[0].GroupName;
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            
        }

        private void OKClick()
        {
            if (string.IsNullOrEmpty(Name))
            {
                System.Windows.MessageBox.Show("Name should not be empty");
                return;
            }
            if (string.IsNullOrEmpty(Group))
            {
                System.Windows.MessageBox.Show("Group should not be empty");
                return;
            }

            User user = new User()
            {
                IP = IP,
                GroupName = Group,
                Name = Name
            };
            UserRepository.RemoveUserByIp(IP);
            UserRepository.InsertUser(user);
        }
    }
}
