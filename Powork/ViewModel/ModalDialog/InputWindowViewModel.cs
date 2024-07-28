using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pi18n;

namespace Powork.ViewModel
{
    class InputWindowViewModel : ObservableObject
    {
        public ResourceManager ResourceManager => ResourceManager.Instance;
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty<string>(ref _title, value);
        }
        private string _value;
        public string Value
        {
            get => _value;
            set => SetProperty<string>(ref _value, value);
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand CancelClickCommand { get; set; }
        public ICommand OKClickCommand { get; set; }

        public InputWindowViewModel()
        {
        }
        public InputWindowViewModel(string title)
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            CancelClickCommand = new RelayCommand(CancelClick);
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
