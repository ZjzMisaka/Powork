using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastHotKeyForWPF;
using Markdig;
using Pi18n;
using Powork.Constant;
using Powork.Helper;
using Powork.Repository;
using Wpf.Ui.Appearance;

namespace Powork.ViewModel
{
    class MemoPageViewModel : ObservableObject
    {
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private readonly string _htmlEnd = @"</html>";

        public ResourceManager ResourceManager => ResourceManager.Instance;
        private string _date;
        public string Date
        {
            get => _date;
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
        private string _search;
        public string Search
        {
            get => _search;
            set => SetProperty<string>(ref _search, value);
        }
        private bool _searchFocused;
        public bool SearchFocused
        {
            get => _searchFocused;
            set => SetProperty<bool>(ref _searchFocused, value);
        }
        private string _memo;
        public string Memo
        {
            get => _memo;
            set
            {
                SetProperty<string>(ref _memo, value);
                double percentage = 0;
                if (Memo != null && Memo.Length != 0)
                {
                    percentage = (double)CurrentLineIndex(Memo, CaretIndex) / TotalLineCount(Memo);
                }
                Preview = GetHtmlStart(percentage) + Markdown.ToHtml(value, _pipeline) + _htmlEnd;
            }
        }
        private string _preview;
        public string Preview
        {
            get => _preview;
            set => SetProperty<string>(ref _preview, value);
        }
        private int _memoColumn;
        public int MemoColumn
        {
            get => _memoColumn;
            set => SetProperty<int>(ref _memoColumn, value);
        }
        private int _previewColumn;
        public int PreviewColumn
        {
            get => _previewColumn;
            set => SetProperty<int>(ref _previewColumn, value);
        }
        private int _memoColumnSpan;
        public int MemoColumnSpan
        {
            get => _memoColumnSpan;
            set => SetProperty<int>(ref _memoColumnSpan, value);
        }
        private int _previewColumnSpan;
        public int PreviewColumnSpan
        {
            get => _previewColumnSpan;
            set => SetProperty<int>(ref _previewColumnSpan, value);
        }
        private Visibility _memoVisibility;
        public Visibility MemoVisibility
        {
            get => _memoVisibility;
            set => SetProperty<Visibility>(ref _memoVisibility, value);
        }
        private Visibility _previewVisibility;
        public Visibility PreviewVisibility
        {
            get => _previewVisibility;
            set => SetProperty<Visibility>(ref _previewVisibility, value);
        }
        private Thickness _memoMargin;
        public Thickness MemoMargin
        {
            get => _memoMargin;
            set => SetProperty<Thickness>(ref _memoMargin, value);
        }
        private int _caretIndex;
        public int CaretIndex
        {
            get => _caretIndex;
            set => SetProperty<int>(ref _caretIndex, value);
        }
        private Thickness _previewMargin;
        public Thickness PreviewMargin
        {
            get => _previewMargin;
            set => SetProperty<Thickness>(ref _previewMargin, value);
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand EditVisibleChangeCommand { get; set; }
        public ICommand PreviewVisibleChangeCommand { get; set; }
        public ICommand SwapCommand { get; set; }
        public ICommand SaveDocumentCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public MemoPageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            EditVisibleChangeCommand = new RelayCommand(EditVisibleChange);
            PreviewVisibleChangeCommand = new RelayCommand(PreviewVisibleChange);
            SwapCommand = new RelayCommand(Swap);
            SaveDocumentCommand = new RelayCommand(SaveDocument);
            SaveCommand = new RelayCommand(Save);

            MemoColumn = 0;
            PreviewColumn = 2;
            MemoColumnSpan = 3;
            PreviewColumnSpan = 1;
            MemoVisibility = Visibility.Visible;
            PreviewVisibility = Visibility.Hidden;
            MemoMargin = new Thickness(5);
            PreviewMargin = new Thickness(5);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            Preview = GetHtmlStart(0) + _htmlEnd;
            Date = DateTime.Now.ToString(Format.DateTimeFormat);

            ApplicationThemeManager.Changed += ThemeChanged;

            GlobalHotKey.Register(VirtualModifiers.Ctrl, VirtualKeys.F, (s, e) => SearchFocused = true);
            GlobalHotKey.Register(VirtualModifiers.Ctrl, VirtualKeys.Left, PreviousDay);
            GlobalHotKey.Register(VirtualModifiers.Ctrl, VirtualKeys.Right, NextDay);
            GlobalHotKey.Register(VirtualModifiers.Ctrl | VirtualModifiers.Shift, VirtualKeys.Left, PreviousMemo);
            GlobalHotKey.Register(VirtualModifiers.Ctrl | VirtualModifiers.Shift, VirtualKeys.Right, NextMemo);
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            ApplicationThemeManager.Changed -= ThemeChanged;

            GlobalHotKey.Unregister(VirtualModifiers.Ctrl, VirtualKeys.F);
            GlobalHotKey.Unregister(VirtualModifiers.Ctrl, VirtualKeys.Left);
            GlobalHotKey.Unregister(VirtualModifiers.Ctrl, VirtualKeys.Right);
            GlobalHotKey.Unregister(VirtualModifiers.Ctrl | VirtualModifiers.Shift, VirtualKeys.Left);
            GlobalHotKey.Unregister(VirtualModifiers.Ctrl | VirtualModifiers.Shift, VirtualKeys.Right);
        }

        private void PreviousDay(object s, HotKeyEventArgs e)
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                dateTime = dateTime.AddDays(-1);
                string formattedDate = dateTime.ToString(Format.DateTimeFormat);
                Date = dateTime.ToString(Format.DateTimeFormat);
                Memo = MemoRepository.SelectMemo(formattedDate);
            }
        }

        private void NextDay(object s, HotKeyEventArgs e)
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                dateTime = dateTime.AddDays(1);
                Date = dateTime.ToString(Format.DateTimeFormat);
            }
        }

        private void PreviousMemo(object s, HotKeyEventArgs e)
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                string date = MemoRepository.SelectPreviousMemoDate(dateTime.ToString(Format.DateTimeFormat), Search);
                if (date != null)
                {
                    Date = date;
                }
            }
        }

        private void NextMemo(object s, HotKeyEventArgs e)
        {
            if (DateTime.TryParse(Date, out DateTime dateTime))
            {
                string date = MemoRepository.SelectNextMemoDate(dateTime.ToString(Format.DateTimeFormat), Search);
                if (date != null)
                {
                    Date = date;
                }
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
                if (MemoVisibility == Visibility.Visible && MemoColumn == 2)
                {
                    Swap(false);
                }
                if (PreviewVisibility == Visibility.Visible && PreviewColumn == 2)
                {
                    Swap(false);
                }
            }
        }

        private void SetColSpan()
        {
            if (IsSingleElement())
            {
                if (MemoVisibility == Visibility.Visible && MemoColumn == 0)
                {
                    MemoColumnSpan = 3;
                }
                else if (PreviewVisibility == Visibility.Visible && PreviewColumn == 0)
                {
                    PreviewColumnSpan = 3;
                }
            }
            else
            {
                MemoColumnSpan = 1;
                PreviewColumnSpan = 1;
            }
        }

        private void SetMargin()
        {
            if (IsSingleElement())
            {
                if (MemoVisibility == Visibility.Visible && MemoColumn == 0)
                {
                    MemoMargin = new Thickness(5);
                }
                else if (PreviewVisibility == Visibility.Visible && PreviewColumn == 0)
                {
                    PreviewMargin = new Thickness(5);
                }
            }
            else
            {
                if (MemoColumn == 0)
                {
                    MemoMargin = new Thickness(5, 5, 0, 5);
                    PreviewMargin = new Thickness(0, 5, 5, 5);
                }
                else if (PreviewColumn == 0)
                {
                    PreviewMargin = new Thickness(5, 5, 0, 5);
                    MemoMargin = new Thickness(0, 5, 5, 5);
                }
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
            SetMargin();
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
            SetMargin();
        }

        private void Swap()
        {
            Swap(true);
        }

        private void Swap(bool swapVisibility)
        {
            if (swapVisibility && IsSingleElement())
            {
                Visibility tempVisibility = MemoVisibility;
                MemoVisibility = PreviewVisibility;
                PreviewVisibility = tempVisibility;
            }

            int temp = MemoColumn;
            MemoColumn = PreviewColumn;
            PreviewColumn = temp;

            if (swapVisibility && IsSingleElement())
            {
                SetCol();
            }

            SetColSpan();
            SetMargin();
        }

        private void SaveDocument()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".html",
                Filter = "Html documents (.html)|*.html|All files (*.*)|*.*",
                FileName = $"Memo-{Date}.html",
            };

            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                File.WriteAllText(filePath, Preview);
            }
        }

        private void Save()
        {
            MemoRepository.InsertOrUpdateMemo(Date, Memo);
        }

        private void ThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            Memo = _memo;
        }

        private string GetHtmlStart(double percentage)
        {
            return $@"<!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Document</title>
                        <style>
                            html {{
                                overflow: auto;
                                {ThemeHelper.ScrollbarStyleText}
                            }}

                            body {{
                                background-color: {ThemeHelper.BackgroundColorText};
                                color: {ThemeHelper.ForegroundColorText};
                            }}

                            p, ul, ol {{
                                margin-top: 0.5em;
                                margin-bottom: 0.5em;
                            }}

                            code {{
                                background-color: {ThemeHelper.PreBackgroundColorText};
                                color: {ThemeHelper.PreForegroundColorText};
                                padding: 0.2em 0.4em;
                                border-radius: 4px;
                                font-family: Consolas, ""Courier New"", monospace;
                                font-size: 0.95em;
                                display: inline-block;
                                white-space: pre;
                                overflow: hidden;
                            }}

                            pre code {{
                                display: block;
                                padding: 1em;
                                overflow-x: auto;
                            }}

                            pre {{
                                background-color: {ThemeHelper.PreBackgroundColorText};
                                color: {ThemeHelper.PreForegroundColorText};
                                padding: 0.2em;
                                border-radius: 4px;
                                font-family: Consolas, ""Courier New"", monospace;
                                font-size: 0.95em;
                                overflow: auto;
                            }}

                            table {{
                                width: 100%;
                                border-collapse: collapse;
                                margin: 1em 0;
                                background-color: {ThemeHelper.PreBackgroundColorText};
                                color: {ThemeHelper.PreForegroundColorText};
                                font-family: Arial, sans-serif;
                                font-size: 0.95em;
                            }}

                            th, td {{
                                padding: 0.6em 0.8em;
                                border: 1px solid #444;
                                text-align: left;
                            }}

                            th {{
                                background-color: {ThemeHelper.TableHeaderBackgroundColorText};
                                font-weight: bold;
                            }}

                            tr:nth-child(even) {{
                                background-color: {ThemeHelper.PreEvenBackgroundColorText};
                            }}

                            tr:hover {{
                                background-color: {ThemeHelper.TableHoverBackgroundColorText};
                            }}
                        </style>

                        <script>
                            (function() {{
                                window.onload = function() {{
                                    var totalHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
                                    var scrollPosition = totalHeight * {percentage};
                                    window.scrollTo(0, scrollPosition);
                                }};
                            }})();
                        </script>

                    </head>";
        }

        private int CurrentLineIndex(string text, int caretIndex)
        {
            int line = 1;
            for (int i = 0; i < caretIndex && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    ++line;
                }
            }

            if (line == 1)
            {
                line = 0;
            }

            return line;
        }

        private int TotalLineCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int lineCount = 1;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    ++lineCount;
                }
            }

            return lineCount;
        }
    }
}
