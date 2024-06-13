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
    class ManageTeamMemberWindowViewModel : ObservableObject
    {
        private List<UserViewModel> SelectedInOtherUserList => OtherUserList.Where(x => x.Selected).ToList();

        private ObservableCollection<UserViewModel> _otherUserList;
        public ObservableCollection<UserViewModel> OtherUserList
        {
            get
            {
                return _otherUserList;
            }
            set
            {
                SetProperty<ObservableCollection<UserViewModel>>(ref _otherUserList, value);
            }
        }

        private List<UserViewModel> SelectedInTeamUserList => TeamUserList.Where(x => x.Selected).ToList();

        private ObservableCollection<UserViewModel> _teamUserList;
        public ObservableCollection<UserViewModel> TeamUserList
        {
            get
            {
                return _teamUserList;
            }
            set
            {
                SetProperty<ObservableCollection<UserViewModel>>(ref _teamUserList, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand ToTeamCommand { get; set; }
        public ICommand ToOtherCommand { get; set; }
        public ICommand CancelClickCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public ManageTeamMemberWindowViewModel()
        {
        }
        public ManageTeamMemberWindowViewModel(List<User> allUserList, List<User> memberList)
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            ToTeamCommand = new RelayCommand(ToTeam);
            ToOtherCommand = new RelayCommand(ToOther);
            CancelClickCommand = new RelayCommand(CancelClick);
            OKClickCommand = new RelayCommand(OKClick);

            TeamUserList = new ObservableCollection<UserViewModel>();
            OtherUserList = new ObservableCollection<UserViewModel>();

            foreach (User user in memberList)
            {
                TeamUserList.Add(new UserViewModel(user));
            }
            foreach (User user in allUserList)
            {
                bool hasSame = false;
                foreach (User member in memberList)
                {
                    if (member.Name == user.Name && member.IP == user.IP)
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame)
                {
                    OtherUserList.Add(new UserViewModel(user));
                }
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

        private void ToTeam()
        {
            foreach (UserViewModel userViewModel in SelectedInOtherUserList)
            {
                TeamUserList.Add(userViewModel);
                OtherUserList.Remove(userViewModel);
            }
        }

        private void ToOther()
        {
            foreach (UserViewModel userViewModel in SelectedInTeamUserList)
            {
                TeamUserList.Remove(userViewModel);
                OtherUserList.Add(userViewModel);
            }
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
