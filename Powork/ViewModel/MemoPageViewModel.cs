using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using Powork.Constant;
using Powork.Repository;

namespace Powork.ViewModel
{
    class MemoPageViewModel : ObservableObject
    {
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private readonly string _htmlStart = @"<!DOCTYPE html>
                                    <html lang=""en"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Document</title>
                                        <style>
                                            body {
                                                background-color: rgb(57, 57, 57);
                                                color: white;
                                            }

                                            code {
                                              background-color: #2d2d2d;
                                              color: #f8f8f2;
                                              padding: 0.2em 0.4em;
                                              border-radius: 4px;
                                              font-family: Consolas, ""Courier New"", monospace;
                                              font-size: 0.95em;
                                              display: inline-block;
                                              white-space: pre;
                                            }

                                            pre code {
                                              display: block;
                                              padding: 1em;
                                              overflow-x: auto;
                                            }

                                            pre {
                                              background-color: #2d2d2d;
                                              color: #f8f8f2;
                                              padding: 0.2em;
                                              border-radius: 4px;
                                              font-family: Consolas, ""Courier New"", monospace;
                                              font-size: 0.95em;
                                              overflow: auto;
                                            }

                                            table {
                                              width: 100%;
                                              border-collapse: collapse;
                                              margin: 1em 0;
                                              background-color: #2d2d2d;
                                              color: #f8f8f2;
                                              font-family: Arial, sans-serif;
                                            }

                                            th, td {
                                              padding: 0.6em 0.8em;
                                              border: 1px solid #444;
                                              text-align: left;
                                            }

                                            th {
                                              background-color: #444;
                                              font-weight: bold;
                                            }

                                            tr:nth-child(even) {
                                              background-color: #383838;
                                            }

                                            tr:hover {
                                              background-color: #555;
                                            }

                                            caption {
                                              caption-side: bottom;
                                              padding: 0.5em;
                                              font-size: 1em;
                                              color: #f8f8f2;
                                            }
                                        </style>
                                    </head>";
        private readonly string _htmlEnd = @"</html>";
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
                Preview = _htmlStart + Markdown.ToHtml(value, _pipeline) + _htmlEnd;
            }
        }
        private string _preview;
        public string Preview
        {
            get
            {
                return _preview;
            }
            set
            {
                SetProperty<string>(ref _preview, value);
            }
        }
        private int _memoColumn;
        public int MemoColumn
        {
            get
            {
                return _memoColumn;
            }
            set
            {
                SetProperty<int>(ref _memoColumn, value);
            }
        }
        private int _previewColumn;
        public int PreviewColumn
        {
            get
            {
                return _previewColumn;
            }
            set
            {
                SetProperty<int>(ref _previewColumn, value);
            }
        }
        private int _memoColumnSpan;
        public int MemoColumnSpan
        {
            get
            {
                return _memoColumnSpan;
            }
            set
            {
                SetProperty<int>(ref _memoColumnSpan, value);
            }
        }
        private int _previewColumnSpan;
        public int PreviewColumnSpan
        {
            get
            {
                return _previewColumnSpan;
            }
            set
            {
                SetProperty<int>(ref _previewColumnSpan, value);
            }
        }
        private Visibility _memoVisibility;
        public Visibility MemoVisibility
        {
            get
            {
                return _memoVisibility;
            }
            set
            {
                SetProperty<Visibility>(ref _memoVisibility, value);
            }
        }
        private Visibility _previewVisibility;
        public Visibility PreviewVisibility
        {
            get
            {
                return _previewVisibility;
            }
            set
            {
                SetProperty<Visibility>(ref _previewVisibility, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand PreviousDayCommand { get; set; }
        public ICommand NextDayCommand { get; set; }
        public ICommand EditVisibleChangeCommand { get; set; }
        public ICommand PreviewVisibleChangeCommand { get; set; }
        public ICommand SwapCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public MemoPageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            PreviousDayCommand = new RelayCommand(PreviousDay);
            NextDayCommand = new RelayCommand(NextDay);
            EditVisibleChangeCommand = new RelayCommand(EditVisibleChange);
            PreviewVisibleChangeCommand = new RelayCommand(PreviewVisibleChange);
            SwapCommand = new RelayCommand(Swap);
            SaveCommand = new RelayCommand(Save);

            MemoColumn = 0;
            PreviewColumn = 1;
            MemoColumnSpan = 1;
            PreviewColumnSpan = 1;

            Preview = _htmlStart + _htmlEnd;
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

        private bool IsSingleElement()
        {
            return MemoVisibility == Visibility.Hidden || PreviewVisibility == Visibility.Hidden;
        }

        private void SetCol()
        {
            if (IsSingleElement())
            {
                if (MemoVisibility == Visibility.Visible && MemoColumn == 1)
                {
                    Swap();
                }
                if (PreviewVisibility == Visibility.Visible && PreviewColumn == 1)
                {
                    Swap();
                }
            }
        }

        private void SetColSpan()
        {
            if (IsSingleElement())
            {
                if (MemoVisibility == Visibility.Visible && MemoColumn == 0)
                {
                    MemoColumnSpan = 2;
                }
                if (PreviewVisibility == Visibility.Visible && PreviewColumn == 0)
                {
                    PreviewColumnSpan = 2;
                }
            }
            else
            {
                MemoColumnSpan = 1;
                PreviewColumnSpan = 1;
            }
        }

        private void EditVisibleChange()
        {
            if (MemoVisibility == Visibility.Hidden)
            {
                MemoVisibility = Visibility.Visible;
            }
            else
            {
                MemoVisibility = Visibility.Hidden;
            }
            SetCol();
            SetColSpan();
        }

        private void PreviewVisibleChange()
        {
            if (PreviewVisibility == Visibility.Hidden)
            {
                PreviewVisibility = Visibility.Visible;
            }
            else
            {
                PreviewVisibility = Visibility.Hidden;
            }
            SetCol();
            SetColSpan();
        }

        private void Swap()
        {
            int temp = MemoColumn;
            MemoColumn = PreviewColumn;
            PreviewColumn = temp;
        }

        private void Save()
        {
            MemoRepository.InsertOrUpdateMemo(Date, Memo);
        }
    }
}
