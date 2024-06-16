using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

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

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty<string>(ref _name, value);
            }
        }
        private double _progress;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress == 100)
                {
                    return;
                }
                SetProperty<double>(ref _progress, value);
            }
        }
        private string _failedText;
        public string FailedText
        {
            get
            {
                return string.IsNullOrEmpty(_failedText) ? "" : _failedText;
            }
            set
            {
                SetProperty<string>(ref _failedText, value);
            }
        }
        private bool _failed;
        public bool Failed
        {
            get
            {
                return _failed;
            }
            set
            {
                if (value)
                {
                    FailedText = "×";
                }
            }
        }

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

        private long _totalSize = 0;
        public long TotalSize
        {
            get
            {
                return _totalSize;
            }
            set
            {
                _totalSize = value;
            }
        }

        private long _totalBytesReceived = 0;
        public long TotalBytesReceived
        {
            get
            {
                return _totalBytesReceived;
            }
            set
            {
                _totalBytesReceived = value;
            }
        }

        private int _fileCount;
        public int FileCount
        {
            get
            {
                return _fileCount;
            }
            set
            {
                _fileCount = value;
            }
        }

        private int _doneCount;
        public int DoneCount
        {
            get
            {
                return _doneCount;
            }
            set
            {
                _doneCount = value;
            }
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
                Progress = 100;
            }
        }
    }
}
