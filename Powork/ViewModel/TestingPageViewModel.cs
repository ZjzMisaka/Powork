using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using PowerThreadPool;
using Powork.Control;
using Powork.ControlViewModel;
using Powork.Helper;
using Powork.Model;
using Powork.Model.Evidence;
using Powork.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations.Model;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Powork.ViewModel
{
    public class TestingPageViewModel : ObservableObject
    {
        private XSSFWorkbook nowWorkBook;
        private XSSFSheet nowSheet;
        private double imageScale;
        private Dictionary<string, Sheet> sheetDict;

        private Sheet sheetModel;
        public Sheet SheetModel
        {
            get { return sheetModel; }
            set
            {
                SetProperty<Sheet>(ref sheetModel, value);

                if (sheetModel != null && sheetModel.ColumnList != null && sheetModel.ColumnList.Count > ColumnIndex)
                {
                    BlockListForDisplay = sheetModel.ColumnList[ColumnIndex].BlockList;
                }
                else
                {
                    BlockListForDisplay = new ObservableCollection<Block>();
                }
            }
        }

        private ObservableCollection<Block> blockListForDisplay;
        public ObservableCollection<Block> BlockListForDisplay
        {
            get { return blockListForDisplay; }
            set
            {
                SetProperty<ObservableCollection<Block>>(ref blockListForDisplay, value);
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

                    if (sheetDict.ContainsKey(SheetName))
                    {
                        SheetModel = sheetDict[SheetName];
                        ObservableCollection<string> newColumnList = new ObservableCollection<string>();
                        foreach (Column column in SheetModel.ColumnList)
                        {
                            newColumnList.Add(column.Name);
                        }
                        ColumnList = newColumnList;
                    }
                    else
                    {
                        SheetModel = new Sheet(this);
                        SheetModel.ColumnList = new ObservableCollection<Column>();

                        nowSheet = (XSSFSheet)nowWorkBook.GetSheet(SheetName);
                        XSSFRow row = (XSSFRow)nowSheet.GetRow(1);
                        if (row == null)
                        {
                            return;
                        }

                        ObservableCollection<string> newColumnList = new ObservableCollection<string>();
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

                                Column column = new Column(this);
                                column.Name = str;
                                SheetModel.ColumnList.Add(column);
                            }
                        }

                        ColumnList = newColumnList;
                    }
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

                    Column columnModel = SheetModel.ColumnList[ColumnIndex];
                    if (columnModel.BlockList == null)
                    {
                        columnModel.BlockList = new ObservableCollection<Block>();
                    }

                    SheetModel.RowTitleList = new ObservableCollection<string>();

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
                            SheetModel.RowTitleList.Add(rowTitle);
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
                                    Block blockModel = new Block(this);
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
                                    Block blockModel = new Block(this);
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

                    int columnIndex = SheetModel.ColumnList[ColumnIndex].GetIndex(nowSheet) + 1;
                    for (int i = 0; i < rowIndexList.Count; ++i)
                    {
                        int rowIndex = rowIndexList[i];
                        if (drawing is XSSFDrawing xssfDrawing)
                        {
                            XSSFClientAnchor imageAnchor = null;
                            foreach (XSSFShape shape in xssfDrawing.GetShapes())
                            {
                                if (shape is XSSFPicture picture)
                                {
                                    XSSFClientAnchor anchor = (XSSFClientAnchor)picture.GetPreferredSize();
                                    if (anchor.Row1 <= rowIndex + 3 && anchor.Row2 >= rowIndex + 3 && anchor.Col1 <= columnIndex && anchor.Col2 >= columnIndex)
                                    {
                                        imageAnchor = anchor;
                                        ImageInfo imageInfoModel = new Model.Evidence.ImageInfo();
                                        imageInfoModel.WidthInExcel = (anchor.Col2 - anchor.Col1 + 1) * nowSheet.GetColumnWidthInPixels(0) - ((anchor.Dx1 / 914400.0) * 96) - ((anchor.Dx2 / 914400.0) * 96);
                                        imageInfoModel.HeightInExcel = (anchor.Row2 - anchor.Row1 + 1) * nowSheet.DefaultRowHeightInPoints - ((anchor.Dy1 / 914400.0) * 96) - ((anchor.Dy2 / 914400.0) * 96);
                                        imageInfoModel.Anchor = imageAnchor;
                                        columnModel.BlockList[i].ImageSource = ExcelHelper.ConvertByteArrayToImageSource(picture.PictureData.Data);
                                        columnModel.BlockList[i].ImageInfo = imageInfoModel;
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
                                        shapePosition.Dx1 = (int)((anchor.Dx1 / 914400.0) * 96);
                                        shapePosition.Dy1 = (int)((anchor.Dy1 / 914400.0) * 96);
                                        shapePosition.Dx2 = (int)((anchor.Dx2 / 914400.0) * 96);
                                        shapePosition.Dy2 = (int)((anchor.Dy2 / 914400.0) * 96);
                                    }

                                    int nextRow = int.MaxValue;
                                    int nextColumn = int.MaxValue;
                                    if (i + 1 < rowIndexList.Count)
                                    {
                                        nextRow = rowIndexList[i + 1] - 1;
                                    }
                                    if (ColumnIndex + 1 < SheetModel.ColumnList.Count)
                                    {
                                        nextColumn = SheetModel.ColumnList[ColumnIndex + 1].GetIndex(nowSheet) + 1 - 1;
                                    }
                                    if (shapePosition.Row1 >= rowIndex + 3 && shapePosition.Row2 <= nextRow && shapePosition.Col1 >= columnIndex && shapePosition.Col2 <= nextColumn)
                                    {
                                        if (simpleShape.ShapeType == 5 && simpleShape.IsNoFill == true)
                                        {
                                            if (columnModel.BlockList[i].ShapeList == null)
                                            {
                                                columnModel.BlockList[i].ShapeList = new ObservableCollection<Shape>();
                                            }
                                            Shape shapeModel = new Shape();
                                            shapeModel.Type = Model.Evidence.Type.EmptyRectangle;
                                            shapeModel.Position = shapePosition;
                                            columnModel.BlockList[i].ShapeList.Add(shapeModel);
                                        }
                                    }
                                }
                            }

                            if (imageAnchor != null && columnModel.BlockList[i].ShapeList != null)
                            {
                                foreach (Shape shapeModel in columnModel.BlockList[i].ShapeList)
                                {
                                    shapeModel.Position.RowOffsetForImage = shapeModel.Position.Row1 - imageAnchor.Row1;
                                    shapeModel.Position.ColOffsetForImage = shapeModel.Position.Col1 - imageAnchor.Col1;
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
            set
            { 
                SetProperty(ref columnIndex, value);
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

        private Block selectedBlock;
        public Block SelectedBlock
        {
            get => selectedBlock;
            set
            {
                SetProperty(ref selectedBlock, value);

                ShapeItems = new ObservableCollection<UserControl>();
                if (value != null && value.ShapeList != null)
                {
                    foreach (Shape shapeModel in value.ShapeList)
                    {
                        if (shapeModel.Type == Model.Evidence.Type.EmptyRectangle)
                        {
                            double excelImageScaleX = SelectedBlock.ImageInfo.WidthInExcel / SelectedBlock.ImageSource.Width;
                            double excelImageScaleY = SelectedBlock.ImageInfo.HeightInExcel / SelectedBlock.ImageSource.Height;
                            double columnWidthInPixels = nowSheet.GetColumnWidthInPixels(0);
                            double defaultRowHeightInPoints = nowSheet.DefaultRowHeightInPoints;

                            double x = (shapeModel.Position.ColOffsetForImage * columnWidthInPixels + shapeModel.Position.Dx1) * imageScale / excelImageScaleX;
                            double y = (shapeModel.Position.RowOffsetForImage * defaultRowHeightInPoints + shapeModel.Position.Dy1) * imageScale / excelImageScaleY;
                            double width = ((shapeModel.Position.Col2 - shapeModel.Position.Col1 + 1) * columnWidthInPixels - shapeModel.Position.Dx1 - shapeModel.Position.Dx2) * imageScale / excelImageScaleX;
                            double height = ((shapeModel.Position.Row2 - shapeModel.Position.Row1 + 1) * defaultRowHeightInPoints - shapeModel.Position.Dy1 - shapeModel.Position.Dy2) * imageScale / excelImageScaleY;
                            AddEmptyRectangle(shapeModel, x, y, width, height);
                        }
                    }
                }
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
        public ICommand ImageSizeChangedCommand { get; set; }


        public TestingPageViewModel()
        {
            sheetDict = new Dictionary<string, Sheet>();

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
            SaveCommand = new RelayCommand(SaveFile);
            ImageSizeChangedCommand = new RelayCommand<SizeChangedEventArgs>(ImageSizeChanged);
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
            SelectedBlock.ImageSource = Clipboard.GetImage();
        }

        private void AddEmptyRectangle()
        {
            ShapePosition shapePosition = new ShapePosition();
            shapePosition.Col1 = 0;
            shapePosition.Col2 = 0;
            shapePosition.Row1 = 2;
            shapePosition.Row2 = 2;
            shapePosition.Dx1 = 0;
            shapePosition.Dy1 = 0;
            shapePosition.Dx2 = 0;
            shapePosition.Dy2 = 0;

            Shape shapeModel = new Shape();
            shapeModel.Type = Model.Evidence.Type.EmptyRectangle;
            shapeModel.Position = shapePosition;
            AddEmptyRectangle(shapeModel, 0, 0, 100, 100);
        }

        private void AddEmptyRectangle(Shape shapeModel, double x, double y, double width, double height)
        {
            Rectangle rectangle = new Rectangle();
            ((RectangleViewModel)rectangle.DataContext).X = x;
            ((RectangleViewModel)rectangle.DataContext).Y = y;
            ((RectangleViewModel)rectangle.DataContext).RectangleWidth = width;
            ((RectangleViewModel)rectangle.DataContext).RectangleHeight = height;
            ((RectangleViewModel)rectangle.DataContext).ShapeModel = shapeModel;
            ((RectangleViewModel)rectangle.DataContext).Remove += (sender) =>
            {
                ShapeItems.Remove(rectangle);
            };
            ((RectangleViewModel)rectangle.DataContext).Changed += (rectangleViewModel, origX, origY, origWidth, origHeight) =>
            {
                SaveShapes(rectangleViewModel, origX, origY, origWidth, origHeight);
            };
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            ShapeItems.Add(rectangle);

            // SaveShapes();
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
            Column column = new Column(this);
            column.Name = NewColumnName;
            SheetModel.ColumnList.Add(column);
        }

        private void AddRow()
        {
            SheetModel.RowTitleList.Add(NewRowTitle);
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

        private void SaveShapes(RectangleViewModel rectangleViewModel, double origX, double origY, double origWidth, double origHeight)
        {
            SelectedBlock.ShapeList = new ObservableCollection<Shape>();
            foreach (UserControl shape in ShapeItems)
            {
                Shape shapeModel = new Shape();
                if (shape.DataContext.GetType() == typeof(RectangleViewModel)) 
                {
                    shapeModel.Type = Model.Evidence.Type.EmptyRectangle;

                    double columnWidthInPixels = nowSheet.GetColumnWidthInPixels(0);
                    double defaultRowHeightInPoints = nowSheet.DefaultRowHeightInPoints;
                    double excelImageScaleX = SelectedBlock.ImageInfo.WidthInExcel / SelectedBlock.ImageSource.Width;
                    double excelImageScaleY = SelectedBlock.ImageInfo.HeightInExcel / SelectedBlock.ImageSource.Height;
                    XSSFClientAnchor anchor = SelectedBlock.ImageInfo.Anchor;

                    shapeModel.Position = new ShapePosition();

                    double diffX = (rectangleViewModel.X - origX) * excelImageScaleX / imageScale;
                    double diffY = (rectangleViewModel.Y - origY) * excelImageScaleY / imageScale;
                    double diffWidth = (rectangleViewModel.RectangleWidth - origWidth) * excelImageScaleX / imageScale;
                    double diffHeight = (rectangleViewModel.RectangleHeight - origHeight) * excelImageScaleY / imageScale;
                    shapeModel.Position.Col1 = rectangleViewModel.ShapeModel.Position.Col1;
                    shapeModel.Position.Col2 = rectangleViewModel.ShapeModel.Position.Col2;
                    shapeModel.Position.Row1 = rectangleViewModel.ShapeModel.Position.Row1;
                    shapeModel.Position.Row2 = rectangleViewModel.ShapeModel.Position.Row2;
                    shapeModel.Position.Dx1 = rectangleViewModel.ShapeModel.Position.Dx1 + (int)diffX;
                    shapeModel.Position.Dx2 = rectangleViewModel.ShapeModel.Position.Dx2 + (int)(diffX + diffWidth);
                    shapeModel.Position.Dy1 = rectangleViewModel.ShapeModel.Position.Dy1 + (int)diffY;
                    shapeModel.Position.Dy2 = rectangleViewModel.ShapeModel.Position.Dy2 + (int)(diffY + diffHeight);

                    int columnOffset1 = (int)(shapeModel.Position.Dx1 / columnWidthInPixels);
                    shapeModel.Position.Col1 += columnOffset1;
                    shapeModel.Position.Dx1 -= (int)(columnOffset1 * columnWidthInPixels);
                    int rowOffset1 = (int)(shapeModel.Position.Dy1 / defaultRowHeightInPoints);
                    shapeModel.Position.Row1 += rowOffset1;
                    shapeModel.Position.Dy1 -= (int)(rowOffset1 * defaultRowHeightInPoints);
                    int columnOffset2 = (int)(shapeModel.Position.Dx2 / columnWidthInPixels);
                    shapeModel.Position.Col2 += columnOffset2;
                    shapeModel.Position.Dx2 -= (int)(columnOffset2 * columnWidthInPixels);
                    int rowOffset2 = (int)(shapeModel.Position.Dy2 / defaultRowHeightInPoints);
                    shapeModel.Position.Row2 += rowOffset2;
                    shapeModel.Position.Dy2 -= (int)(rowOffset2 * defaultRowHeightInPoints);

                    shapeModel.Position.RowOffsetForImage = shapeModel.Position.Row1 - anchor.Row1;
                    shapeModel.Position.ColOffsetForImage = shapeModel.Position.Col1 - anchor.Col1;
                }

                SelectedBlock.ShapeList.Add(shapeModel);
            }

        }

        private void SaveFile() 
        {
            foreach (string sheetName in SheetList)
            {
                if (sheetDict.ContainsKey(sheetName))
                { 
                    Sheet sheetModel = sheetDict[sheetName];
                    if (sheetModel.Removed)
                    {
                        nowWorkBook.RemoveSheetAt(nowWorkBook.GetSheetIndex(sheetName));
                    }
                    else
                    {
                        // Save
                        XSSFSheet sheet = (XSSFSheet)nowWorkBook.GetSheet(sheetName);
                        int columnIndex = 0;

                        Dictionary<string, int> rowDict = new Dictionary<string, int>();
                        int nowRowTitleRowIndex = 3;
                        foreach (string rowTitle in sheetModel.RowTitleList)
                        {
                            XSSFCell cell = ExcelHelper.GetOrCreateCell(sheet, nowRowTitleRowIndex, 1);
                            cell.SetCellValue(rowTitle);

                            rowDict[rowTitle] = nowRowTitleRowIndex;

                            nowRowTitleRowIndex += 2;
                        }

                        foreach (Column columnModel in sheetModel.ColumnList)
                        {
                            int columnAndRowTitleColumnIndex = columnIndex + 1;

                            XSSFCell cell = ExcelHelper.GetOrCreateCell(sheet, 1, columnAndRowTitleColumnIndex);
                            cell.SetCellValue(columnModel.Name);

                            foreach (Block blockModel in columnModel.BlockList)
                            {
                                string rowTitle = sheetModel.RowTitleList[columnModel.BlockList.IndexOf(blockModel)];
                                int blockRowIndex = rowDict[rowTitle] + 1;
                                int blockColumnIndex = columnIndex + 2;


                            }
                        }
                    }
                }
            }

            string filePath = System.IO.Path.Combine(Path, FileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                nowWorkBook.Write(stream);
            }

            nowWorkBook.Close();
        }

        private void ImageSizeChanged(SizeChangedEventArgs e)
        {
            if (SelectedBlock == null || SelectedBlock.ImageSource == null)
            {
                return;
            }
            imageScale = e.NewSize.Height / SelectedBlock.ImageSource.Height;
            SelectedBlock = SelectedBlock;
        }
    }
}
