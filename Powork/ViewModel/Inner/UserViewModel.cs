using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;

namespace Powork.ViewModel.Inner
{
    internal class UserViewModel : ObservableObject
    {
        public UserViewModel()
        {
        }
        public UserViewModel(User user)
        {
            IP = user.IP;
            Name = user.Name;
            GroupName = user.GroupName;
        }
        public string IP { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }

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
    }
}
