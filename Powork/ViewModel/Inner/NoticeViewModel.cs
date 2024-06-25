using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;

namespace Powork.ViewModel.Inner
{
    public class NoticeViewModel : ObservableObject
    {
        public NoticeViewModel()
        {
        }

        private string _notice;
        public string Notice
        {
            get => _notice;
            set => SetProperty<string>(ref _notice, value);
        }

        public int Count { get; set; }

        public UserMessage UserMessage { get; set; }

        public TeamMessage TeamMessage { get; set; }

    }
}
