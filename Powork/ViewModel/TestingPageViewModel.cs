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
using System.Windows.Media;
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
                    ColumnList.Clear();

                    sheetModel = new Sheet();
                    sheetModel.ColumnList = new List<Column>();

                    nowSheet = (XSSFSheet)nowWorkBook.GetSheet(SheetName);
                    XSSFRow row = (XSSFRow)nowSheet.GetRow(1);
                    if (row == null)
                    {
                        return;
                    }
                    
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
                    BlockList.Clear();

                    Column columnModel = sheetModel.ColumnList[ColumnIndex];
                    if (columnModel.BlockList == null)
                    {
                        columnModel.BlockList = new List<Block>();
                    }

                    sheetModel.RowTitleList = new List<string>();

                    List<int> rowIndexList = new List<int>();
                    string rowTitle = "";
                    int rowTitleIndex = -1;

                    XSSFDrawing drawing = (XSSFDrawing)nowSheet.CreateDrawingPatriarch();

                    for (int rowNum = 2; rowNum < nowSheet.LastRowNum; ++rowNum)
                    {
                        IRow row = nowSheet.GetRow(rowNum);
                        if (row == null)
                        {
                            continue;
                        }
                        List<ICell> cellList = nowSheet.GetRow(rowNum).Cells;

                        if (cellList[0].ColumnIndex == 1)
                        {
                            rowTitleIndex = nowSheet.GetRow(rowNum).RowNum;
                            rowTitle = cellList[0].StringCellValue;
                            sheetModel.RowTitleList.Add(rowTitle);
                            rowIndexList.Add(nowSheet.GetRow(rowNum).RowNum);
                        }
                        else if (nowSheet.GetRow(rowNum).RowNum == rowTitleIndex + 1)
                        {
                            if (cellList.Count > ColumnIndex)
                            {
                                XSSFCell cell = (XSSFCell)cellList[ColumnIndex];
                                string str = cell.StringCellValue;

                                Button button = new Button();
                                button.Content = rowTitle + "\n" + str;
                                BlockList.Add(button);

                                if (BlockList.Count > columnModel.BlockList.Count)
                                {
                                    Block blockModel = new Block();
                                    blockModel.Title = str;
                                    columnModel.BlockList.Add(blockModel);
                                }
                            }
                            else
                            {
                                Button button = new Button();
                                button.Content = rowTitle + "\n" + "Empty";
                                BlockList.Add(button);

                                if (BlockList.Count > columnModel.BlockList.Count)
                                {
                                    Block blockModel = new Block();
                                    columnModel.BlockList.Add(blockModel);
                                }
                            }
                        }
                        else
                        {
                            if (cellList.Count > ColumnIndex)
                            {
                                XSSFCell cell = (XSSFCell)cellList[ColumnIndex];
                                string str = cell.StringCellValue;
                                if (!string.IsNullOrEmpty(str))
                                {
                                    columnModel.BlockList[rowIndexList.Count - 1].Description = str;
                                }
                            }
                            else
                            {
                                columnModel.BlockList[rowIndexList.Count - 1].Description = "";
                            }
                        }
                    }

                    int columnIndex = sheetModel.ColumnList[ColumnIndex].GetIndex(nowSheet) + 1;
                    for (int i = 0; i < rowIndexList.Count; ++i)
                    {
                        int rowIndex = rowIndexList[i];
                        if (drawing is XSSFDrawing xssfDrawing)
                        {
                            foreach (XSSFShape shape in xssfDrawing.GetShapes())
                            {
                                if (shape is XSSFPicture picture)
                                {
                                    XSSFClientAnchor anchor = (XSSFClientAnchor)picture.GetPreferredSize();
                                    if (anchor.Row1 <= rowIndex + 3 && anchor.Row2 >= rowIndex + 3 && anchor.Col1 <= columnIndex && anchor.Col2 >= columnIndex)
                                    {
                                        columnModel.BlockList[i].ImageSource = ConvertByteArrayToImageSource(picture.PictureData.Data);
                                    }
                                }
                                else if (shape is XSSFSimpleShape simpleShape)
                                {
                                    ShapePosition shapePosition = new ShapePosition();

                                    if (simpleShape.GetAnchor() is XSSFClientAnchor)
                                    {
                                        XSSFClientAnchor anchor = (XSSFClientAnchor)simpleShape.GetAnchor();

                                        shapePosition.Col1 = anchor.Col1;
                                        shapePosition.Col2 = anchor.Col2;
                                        shapePosition.Row1 = anchor.Row1;
                                        shapePosition.Row2 = anchor.Row2;
                                        shapePosition.Dx1 = anchor.Dx1;
                                        shapePosition.Dy1 = anchor.Dy1;
                                        shapePosition.Dx2 = anchor.Dx2;
                                        shapePosition.Dy2 = anchor.Dy2;
                                    }

                                    int nextRow = int.MaxValue;
                                    int nextColumn = int.MaxValue;
                                    if (i + 1 < rowIndexList.Count)
                                    {
                                        nextRow = rowIndexList[i + 1] - 1;
                                    }
                                    if (ColumnIndex + 1 < sheetModel.ColumnList.Count)
                                    {
                                        nextColumn = sheetModel.ColumnList[ColumnIndex + 1].GetIndex(nowSheet) + 1 - 1;
                                    }
                                    if (shapePosition.Row1 >= rowIndex + 3 && shapePosition.Row2 <= nextRow && shapePosition.Col1 >= columnIndex && shapePosition.Col2 <= nextColumn)
                                    {
                                        if (simpleShape.ShapeType == 5 && simpleShape.IsNoFill == true)
                                        {
                                            if (columnModel.BlockList[i].ShapeList == null)
                                            {
                                                columnModel.BlockList[i].ShapeList = new List<Shape>();
                                            }
                                            Shape shapeModel = new Shape();
                                            shapeModel.Type = Model.Evidence.Type.EmptyRectangle;
                                            shapeModel.Position = shapePosition;
                                            columnModel.BlockList[i].ShapeList.Add(shapeModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                { }
            }
        }

        private int columnIndex;
        public int ColumnIndex
        {
            get => columnIndex;
            set => SetProperty(ref columnIndex, value);
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

        private int blockIndex;
        public int BlockIndex
        {
            get => blockIndex;
            set => SetProperty(ref blockIndex, value);
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
        public ICommand OpenFileCommand { get; set; }
        public ICommand SendFileCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        

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
            OpenFileCommand = new RelayCommand(OpenFile);
            SendFileCommand = new RelayCommand(SendFile);
            SaveCommand = new RelayCommand(Save);
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
            Rectangle rectangle = new Rectangle();
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
            Column column = new Column();
            column.Name = NewColumnName;
            sheetModel.ColumnList.Add(column);
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

        private void OpenFile()
        {
        }

        private void SendFile()
        {
        }

        private void Save()
        {
            Column columnModel = sheetModel.ColumnList[ColumnIndex];
            Block blockModel = columnModel.BlockList[BlockIndex];
            blockModel.ImageSource = BitmapSource;
            blockModel.ShapeList = new List<Shape>();
            foreach (UserControl shape in ShapeItems)
            {
                Shape shapeModel = new Shape();
                if (shape.DataContext.GetType() == typeof(RectangleViewModel)) 
                {
                    RectangleViewModel rectangleViewModel = (RectangleViewModel)shape.DataContext;

                    shapeModel.Type = Model.Evidence.Type.EmptyRectangle;
                    ShapePosition shapePosition = new ShapePosition();

                    //shapeModel.Position = new System.Drawing.Point((int)rectangleViewModel.X, (int)rectangleViewModel.Y);

                    
                }


                blockModel.ShapeList.Add(shapeModel);
            }

        }
        private void SaveFile() 
        {
        
        }



        private ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 冻结对象，使其可以跨线程使用
                return bitmapImage;
            }
        }
    }
}
