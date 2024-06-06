using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Powork.ViewModel
{
    class InputWindowViewModel : ObservableObject
    {
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                SetProperty<string>(ref title, value);
            }
        }
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetProperty<string>(ref _value, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public InputWindowViewModel()
        {
        }
        public InputWindowViewModel(string title)
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            OKClickCommand = new RelayCommand(OKClick);

            Title = title;
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
