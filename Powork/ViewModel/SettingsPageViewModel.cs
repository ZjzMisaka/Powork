using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Model;
using Powork.Repository;
using Powork.Service;
using Powork.View;

namespace Powork.ViewModel
{
    class SettingsPageViewModel : ObservableObject
    {
        private INavigationService _navigationService;

        private string _ip;
        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                SetProperty<string>(ref _ip, value);
            }
        }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty<string>(ref _name, value);
            }
        }
        private string _group;
        public string Group
        {
            get
            {
                return _group;
            }
            set
            {
                SetProperty<string>(ref _group, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public SettingsPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

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

            _navigationService.Navigate(typeof(MessagePage), new MessagePageViewModel(ServiceLocator.GetService<INavigationService>()));
        }
    }
}
