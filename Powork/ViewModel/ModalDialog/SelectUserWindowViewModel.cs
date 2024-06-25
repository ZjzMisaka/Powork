using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Model;
using Powork.ViewModel.Inner;

namespace Powork.ViewModel
{
    class SelectUserWindowViewModel : ObservableObject
    {
        public List<UserViewModel> SelectedUserList => UserList.Where(x => x.Selected).ToList();

        private ObservableCollection<UserViewModel> _userList;
        public ObservableCollection<UserViewModel> UserList
        {
            get => _userList;
            set => SetProperty<ObservableCollection<UserViewModel>>(ref _userList, value);
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand CancelClickCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public SelectUserWindowViewModel()
        {
        }
        public SelectUserWindowViewModel(List<User> memberList)
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            CancelClickCommand = new RelayCommand(CancelClick);
            OKClickCommand = new RelayCommand(OKClick);

            UserList = new ObservableCollection<UserViewModel>();

            foreach (User user in memberList)
            {
                UserList.Add(new UserViewModel(user));
            }
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

        private void CancelClick()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = false;
                    window.Close();
                    break;
                }
            }
        }

        private void OKClick()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}
