using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using PowerThreadPool;
using Powork.Control;
using Powork.ControlViewModel;
using Powork.Model;
using Powork.Model.Evidence;
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
        private XSSFWorkbook nowWorkBook;
        private XSSFSheet nowSheet;
        private Sheet sheetModel;

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
                    FileList = new ObservableCollection<string>(allfiles);
                }
                catch
                { }
            }
        }

        private ObservableCollection<string> fileList;

        public ObservableCollection<string> FileList 
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
                ObservableCollection<string> newSheetList = new ObservableCollection<string>();
                nowWorkBook = new XSSFWorkbook(System.IO.Path.Combine(Path, FileName));
                for (int i = 0; i < nowWorkBook.NumberOfSheets; ++i)
                {
                    newSheetList.Add(nowWorkBook.GetSheetAt(i).SheetName);
                }

                SheetList = newSheetList;
            }
        }

        private ObservableCollection<string> sheetList;

        public ObservableCollection<string> SheetList
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
                    sheetModel = new Sheet();
                    sheetModel.ColumnList = new List<Column>();

                    nowSheet = (XSSFSheet)nowWorkBook.GetSheet(SheetName);
                    XSSFRow row = (XSSFRow)nowSheet.GetRow(1);
                    if (row == null)
                    {
                        return;
                    }
                    ColumnList.Clear();
                    ObservableCollection<string> newColumnList = new ObservableCollection<string>(); ;
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

                            Column column = new Column();
                            column.Name = str;
                            sheetModel.ColumnList.Add(column);
                        }
                    }

                    ColumnList = newColumnList;
                }
                catch
                { }
            }
        }

        private ObservableCollection<string> columnList;
        public ObservableCollection<string> ColumnList
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
                    if (sheetModel.RowTitleList != null)
                    {
                        return;
                    }

                    sheetModel.RowTitleList = new List<string>();
                    BlockList.Clear();
                    int columnIndex = -1;
                    foreach (Column column in sheetModel.ColumnList)
                    {
                        if (column.Name == ColumnName)
                        {
                            columnIndex = column.GetIndex(nowSheet);
                        }
                    }
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
                                Button button = new Button();
                                button.Content = str;
                                BlockList.Add(button);

                                sheetModel.RowTitleList.Add(str);
                            }
                        }
                    }
                }
                catch
                { }
            }
        }

        private ObservableCollection<System.Windows.Controls.Control> blockList;
        public ObservableCollection<System.Windows.Controls.Control> BlockList
        {
            get => blockList;
            set
            {
                SetProperty<ObservableCollection<System.Windows.Controls.Control>>(ref blockList, value);
            }
        }

        private string newFileName;

        public string NewFileName 
        { 
            get => newFileName; 
            set => SetProperty(ref newFileName, value); 
        }
        private string newSheetName;

        public string NewSheetName 
        { 
            get => newSheetName;
            set => SetProperty(ref newSheetName, value); 
        }
        private string newColumnName;

        public string NewColumnName
        {
            get => newColumnName;
            set => SetProperty(ref newColumnName, value); 
        }
        private string newRowTitle;

        public string NewRowTitle
        {
            get => newRowTitle;
            set => SetProperty(ref newRowTitle, value);
        }


        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand SetImageCommand { get; set; }
        public ICommand AddEmptyRectangleCommand { get; set; }
        public ICommand AddFileCommand { get; set; }
        public ICommand AddSheetCommand { get; set; }
        public ICommand AddColumnCommand { get; set; }
        public ICommand AddRowCommand { get; set; }

        public TestingPageViewModel()
        {
            ShapeItems = new ObservableCollection<UserControl>();
            FileList = new ObservableCollection<string>();
            SheetList = new ObservableCollection<string>();
            ColumnList = new ObservableCollection<string>();
            BlockList = new ObservableCollection<System.Windows.Controls.Control>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SetImageCommand = new RelayCommand(SetImage);
            AddEmptyRectangleCommand = new RelayCommand(AddEmptyRectangle);
            AddFileCommand = new RelayCommand(AddFile);
            AddSheetCommand = new RelayCommand(AddSheet);
            AddColumnCommand = new RelayCommand(AddColumn);
            AddRowCommand = new RelayCommand(AddRow);
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

        private void AddFile()
        {
        }

        private void AddSheet()
        {
            SheetList.Add(NewSheetName);
        }

        private void AddColumn()
        {
        }

        private void AddRow()
        {
            sheetModel.RowTitleList.Add(NewRowTitle);
            Button button = new Button();
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            button.Margin = new Thickness(0);
            button.Content = NewRowTitle;
            BlockList.Add(button);
        }
    }
}
