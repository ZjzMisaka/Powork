using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Constant;
using Powork.Repository;

namespace Powork.ViewModel
{
    class MemoPageViewModel : ObservableObject
    {
        private string _date;
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                SetProperty<string>(ref _date, value);
                if (DateTime.TryParse(value, out DateTime dateTime))
                {
                    string formattedDate = dateTime.ToString(Format.DateTimeFormat);
                    Memo = MemoRepository.SelectMemo(formattedDate);
                }
            }
        }
        private string _memo;
        public string Memo
        {
            get
            {
                return _memo;
            }
            set
            {
                SetProperty<string>(ref _memo, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand PreviousDayCommand { get; set; }
        public ICommand NextDayCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public MemoPageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            PreviousDayCommand = new RelayCommand(PreviousDay);
            NextDayCommand = new RelayCommand(NextDay);
            SaveCommand = new RelayCommand(Save);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            Date = DateTime.Now.ToString(Format.DateTimeFormat);
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
        }

        private void PreviousDay()
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                dateTime = dateTime.AddDays(-1);
                string formattedDate = dateTime.ToString(Format.DateTimeFormat);
                Date = dateTime.ToString(Format.DateTimeFormat);
                Memo = MemoRepository.SelectMemo(formattedDate);
            }
        }

        private void NextDay()
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                dateTime = dateTime.AddDays(1);
                string formattedDate = dateTime.ToString(Format.DateTimeFormat);
                Date = dateTime.ToString(Format.DateTimeFormat);
                Memo = MemoRepository.SelectMemo(formattedDate);
            }
        }

        private void Save()
        {
            MemoRepository.InsertOrUpdateMemo(Date, Memo);
        }
    }
}
