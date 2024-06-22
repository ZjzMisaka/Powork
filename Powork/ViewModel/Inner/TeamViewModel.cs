using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;

namespace Powork.ViewModel.Inner
{
    internal class TeamViewModel : ObservableObject
    {
        public TeamViewModel()
        {
        }
        public TeamViewModel(Team team)
        {
            ID = team.ID;
            Name = team.Name;

            List<User> memberList = TeamRepository.SelectTeamMember(ID);
            foreach (User user in memberList)
            {
                if (MemberList.Trim() != string.Empty)
                {
                    MemberList += "\n";
                }
                MemberList += user.Name;
            }
        }
        public string ID { get; set; }
        public string Name { get; set; }

        private bool _selected;
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (_selected)
                {
                    BackgroundColor = ThemeHelper.SelectedBackgroundBrush;
                    ForegroundColor = ThemeHelper.SelectedForegroundBrush;
                }
                else
                {
                    BackgroundColor = Brushes.Transparent;
                    ForegroundColor = ThemeHelper.ForegroundBrush;
                }
            }
        }
        private Brush _backgroundColor = Brushes.Transparent;
        public Brush BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                SetProperty<Brush>(ref _backgroundColor, value);
            }
        }
        private Brush _foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        public Brush ForegroundColor
        {
            get
            {
                return _foregroundColor;
            }
            set
            {
                SetProperty<Brush>(ref _foregroundColor, value);
            }
        }

        private string _memberList;
        public string MemberList
        {
            get
            {
                return _memberList == null ? string.Empty : _memberList;
            }
            set
            {
                SetProperty<string>(ref _memberList, value);
            }
        }
    }
}
