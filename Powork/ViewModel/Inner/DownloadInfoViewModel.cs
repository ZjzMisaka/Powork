using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Helper;

namespace Powork.ViewModel.Inner
{
    public class DownloadInfoViewModel : ObservableObject
    {
        public DownloadInfoViewModel()
        {
        }
        public string RequestID { get; set; }
        public string ID { get; set; }
        public string Path { get; set; }
        public bool Stop { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty<string>(ref _name, value);
        }
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress == 100)
                {
                    return;
                }
                SetProperty<double>(ref _progress, value);
            }
        }
        private string _statusText;
        public string StatusText
        {
            get => string.IsNullOrEmpty(_statusText) ? "" : _statusText;
            set => SetProperty<string>(ref _statusText, value);
        }
        private bool _failed;
        public bool Failed
        {
            get => _failed;
            set
            {
                if (value)
                {
                    StatusText = "×";
                }
            }
        }

        private bool _selected;
        public bool Selected
        {
            get => _selected;
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
            get => _backgroundColor;
            set => SetProperty<Brush>(ref _backgroundColor, value);
        }

        private Brush _foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        public Brush ForegroundColor
        {
            get => _foregroundColor;
            set => SetProperty<Brush>(ref _foregroundColor, value);
        }

        private long _totalSize = 0;
        public long TotalSize
        {
            get => _totalSize;
            set => _totalSize = value;
        }

        private long _totalBytesReceived = 0;
        public long TotalBytesReceived
        {
            get => _totalBytesReceived;
            set => _totalBytesReceived = value;
        }

        private int _fileCount;
        public int FileCount
        {
            get => _fileCount;
            set => _fileCount = value;
        }

        private int _doneCount;
        public int DoneCount
        {
            get => _doneCount;
            set => _doneCount = value;
        }

        public void Received(long size)
        {
            Interlocked.Add(ref _totalBytesReceived, size);
            Progress = (double)TotalBytesReceived / TotalSize * 100;
        }

        public void Done()
        {
            Interlocked.Increment(ref _doneCount);
            if (_doneCount == FileCount)
            {
                StatusText = "〇";
                Progress = 100;
            }
        }
    }
}
