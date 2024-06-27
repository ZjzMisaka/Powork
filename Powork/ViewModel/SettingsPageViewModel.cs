using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Helper;
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
            get => _ip;
            set => SetProperty<string>(ref _ip, value);
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty<string>(ref _name, value);
        }
        private string _group;
        public string Group
        {
            get => _group;
            set => SetProperty<string>(ref _group, value);
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

            User user = GlobalVariables.SelfInfo;
            if (user != null)
            {
                Name = user.Name;
                Group = user.GroupName;
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {

        }

        private void OKClick()
        {
            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show(Application.Current.FindResource("EmptyNameErrorMessage").ToString());
                return;
            }
            if (string.IsNullOrEmpty(Group))
            {
                MessageBox.Show(Application.Current.FindResource("EmptyGroupErrorMessage").ToString());
                return;
            }

            User user = new User()
            {
                IP = IP,
                GroupName = Group,
                Name = Name
            };

            if (UserHelper.IsUserLogon())
            {
                UserRepository.UpdateLogonUserByIP(user);
            }
            else
            {
                UserRepository.InsertLogonUser(user);
            }

            _navigationService.Navigate(typeof(MessagePage), new MessagePageViewModel(ServiceLocator.GetService<INavigationService>()));
        }
    }
}
