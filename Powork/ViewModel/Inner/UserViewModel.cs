using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;
using System.Windows.Forms;
using System.Windows.Media;

namespace Powork.ViewModel.Inner
{
    internal class UserViewModel : ObservableObject
    {
        public UserViewModel()
        {
        }
        public UserViewModel(User user)
        {
            this.IP = user.IP;
            this.Name = user.Name;
            this.GroupName = user.GroupName;
        }
        public string IP { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }

        private bool selected;
        public bool Selected
        { 
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (selected)
                {
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                }
                else
                {
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                }
            }
        }
        private Brush backgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
        public Brush BackgroundColor
        { 
            get
            { 
                return backgroundColor;
            }
            set
            {
                SetProperty<Brush>(ref backgroundColor, value);
            }
        }
    }
}
