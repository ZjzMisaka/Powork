using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;
using Powork.Repository;

namespace Powork.ViewModel.Inner
{
    public enum OnlineStatus
    {
        Online,
        Offline,
    }
    public class UserViewModel : ObservableObject
    {
        private Timer _timer;
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
        private OnlineStatus _status = OnlineStatus.Offline;
        public OnlineStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status == OnlineStatus.Offline && value == OnlineStatus.Online)
                {
                    List<string> messages = DelaySendingMessageRepository.SelectMessgae(IP);
                    if (messages.Count > 0)
                    {
                        DelaySendingMessageRepository.RemoveMessgae(IP);
                        foreach (string message in messages)
                        {
#pragma warning disable CS4014
                            GlobalVariables.TcpServerClient.SendMessage(message, IP, GlobalVariables.TcpPort);
#pragma warning restore CS4014
                        }
                    }
                }
                _status = value;
                if (_status == OnlineStatus.Online)
                {
                    Opacity = 1;
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }
                    _timer = new Timer((e) => { Status = OnlineStatus.Offline; }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
                }
                else
                {
                    Opacity = 0.5;
                }
            }
        }
        private double _opacity = 0.5;
        public double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                SetProperty<double>(ref _opacity, value);
            }
        }
    }
}
