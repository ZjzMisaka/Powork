using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Powork.ViewModel.ModalDialog
{
    public partial class ManageProjectWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<User> _otherUsers;

        [ObservableProperty]
        private ObservableCollection<User> _managers;

        [ObservableProperty]
        private User _selectedUser;

        [ObservableProperty]
        private User _selectedManager;

        public ICommand AddManagerCommand { get; }
        public ICommand RemoveManagerCommand { get; }
        public ICommand ExitProjectCommand { get; }

        public ManageProjectWindowViewModel(Project project)
        {
            var managerIPs = project.Managers.Split(';').ToList();
            var allUsers = new ObservableCollection<User>(GlobalVariables.UserList);
            
            Managers = new ObservableCollection<User>(allUsers.Where(u => managerIPs.Contains(u.IP)));
            OtherUsers = new ObservableCollection<User>(allUsers.Except(Managers));

            AddManagerCommand = new RelayCommand(AddManager);
            RemoveManagerCommand = new RelayCommand(RemoveManager);
        }

        private void AddManager()
        {
            if (SelectedUser != null)
            {
                OtherUsers.Remove(SelectedUser);
                Managers.Add(SelectedUser);
            }
        }

        private void RemoveManager()
        {
            if (SelectedManager != null)
            {
                Managers.Remove(SelectedManager);
                OtherUsers.Add(SelectedManager);
            }
        }
    }
}
