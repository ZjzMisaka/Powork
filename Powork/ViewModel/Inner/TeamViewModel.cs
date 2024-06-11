using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
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
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                }
                else
                {
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                }
            }
        }
        private Brush _backgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
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

        private string _memberList;
        public string MemberList
        {
            get
            {
                return _memberList;
            }
            set
            {
                SetProperty<string>(ref _memberList, value);
            }
        }
    }
}
