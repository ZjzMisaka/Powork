using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using PowerThreadPool;
using Powork.Control;
using Powork.ControlViewModel;
using Powork.Model;
using Powork.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Powork.ViewModel
{
    class TestingPageViewModel : ObservableObject
    {
        private Dictionary<string, int> columnDict;
        private Dictionary<string, int> rowDict;
        private XSSFWorkbook nowWorkBook;
        private XSSFSheet nowSheet;

        private BitmapSource bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return bitmapSource; }
            set
            {
                SetProperty<BitmapSource>(ref bitmapSource, value);
            }
        }

        private ObservableCollection<UserControl> shapeItems;
        public ObservableCollection<UserControl> ShapeItems
        {
            get { return shapeItems; }
            set
            {
                SetProperty<ObservableCollection<UserControl>>(ref shapeItems, value);
            }
        }

        private ObservableCollection<UserControl> outerItems;
        public ObservableCollection<UserControl> OuterItems
        {
            get { return outerItems; }
            set
            {
                SetProperty<ObservableCollection<UserControl>>(ref outerItems, value);
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                SetProperty<string>(ref path, value);

                try
                {
                    string[] allfiles = Directory.GetFiles(path, "*.xlsx", SearchOption.TopDirectoryOnly);
                    FileList = new List<string>(allfiles);
                }
                catch
                { }
            }
        }

        private List<string> fileList;

        public List<string> FileList 
        { 
            get => fileList;
            set
            {
                SetProperty(ref fileList, value);
            }
        }

        private string fileName;

        public string FileName 
        { 
            get => fileName;
            set
            {
                SetProperty<string>(ref fileName, value);

                SheetList.Clear();
                List<string> newSheetList = new List<string>();
                nowWorkBook = new XSSFWorkbook(System.IO.Path.Combine(Path, FileName));
                for (int i = 0; i < nowWorkBook.NumberOfSheets; ++i)
                {
                    newSheetList.Add(nowWorkBook.GetSheetAt(i).SheetName);
                }

                SheetList = newSheetList;
            }
        }

        private List<string> sheetList;

        public List<string> SheetList
        {
            get => sheetList;
            set
            {
                SetProperty(ref sheetList, value);
            }
        }

        private string sheetName;

        public string SheetName
        {
            get => sheetName;
            set
            {
                SetProperty<string>(ref sheetName, value);

                try
                {
                    nowSheet = (XSSFSheet)nowWorkBook.GetSheet(SheetName);
                    XSSFRow row = (XSSFRow)nowSheet.GetRow(1);
                    if (row == null)
                    {
                        return;
                    }
                    ColumnList.Clear();
                    columnDict.Clear();
                    List<string> newColumnList = new List<string>(); ;
                    for (int i = 0; i < row.Cells.Count; ++i)
                    {
                        if (row.Cells[i].CellType.ToString() != "String")
                        {
                            continue;
                        }
                        string str = row.Cells[i].StringCellValue;
                        if (!string.IsNullOrEmpty(str))
                        {
                            newColumnList.Add(str);
                            columnDict[str] = i;
                        }
                    }

                    ColumnList = newColumnList;
                }
                catch
                { }
            }
        }

        private List<string> columnList;
        public List<string> ColumnList
        {
            get => columnList;
            set
            {
                SetProperty(ref columnList, value);
            }
        }

        private string columnName;
        public string ColumnName
        {
            get => columnName;
            set
            {
                SetProperty<string>(ref columnName, value);

                try
                {
                    RowList.Clear();
                    rowDict.Clear();
                    int columnIndex = columnDict[ColumnName];
                    for (int rowNum = 2; rowNum < nowSheet.LastRowNum; ++rowNum)
                    {
                        IRow row = nowSheet.GetRow(rowNum);
                        if (row == null)
                        {
                            continue;
                        }
                        List<ICell> cellList = nowSheet.GetRow(rowNum).Cells;
                        if (cellList.Count > columnIndex)
                        {
                            XSSFCell cell = (XSSFCell)cellList[columnIndex];
                            string str = cell.StringCellValue;
                            if (!string.IsNullOrEmpty(str))
                            {
                                RowList.Add(new Button());
                                rowDict[str] = rowNum;
                            }
                        }
                    }
                }
                catch
                { }
            }
        }

        private List<System.Windows.Controls.Control> rowList;
        public List<System.Windows.Controls.Control> RowList
        {
            get => rowList;
            set
            {
                SetProperty<List<System.Windows.Controls.Control>>(ref rowList, value);
            }
        }


        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand SetImageCommand { get; set; }
        public ICommand AddEmptyRectangleCommand { get; set; }

        public TestingPageViewModel()
        {
            columnDict = new Dictionary<string, int>();
            rowDict = new Dictionary<string, int>();
            ShapeItems = new ObservableCollection<UserControl>();
            OuterItems = new ObservableCollection<UserControl>();
            FileList = new List<string>();
            SheetList = new List<string>();
            ColumnList = new List<string>();
            RowList = new List<System.Windows.Controls.Control>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SetImageCommand = new RelayCommand(SetImage);
            AddEmptyRectangleCommand = new RelayCommand(AddEmptyRectangle);
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

        private void SetImage()
        {
            BitmapSource = Clipboard.GetImage();
        }

        private void AddEmptyRectangle()
        {
            Powork.Control.Rectangle rectangle = new Rectangle();
            ((RectangleViewModel)rectangle.DataContext).Remove += (sender) =>
            {
                ShapeItems.Remove(rectangle);
            };
            ShapeItems.Add(rectangle);
        }
    }
}
