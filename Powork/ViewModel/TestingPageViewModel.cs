﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NPOI.OpenXmlFormats.Spreadsheet;
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
        private double canvasImageScale;
        private double excelImageScaleX;
        private double excelImageScaleY;
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
                if (nowWorkBook != null)
                {
                    nowWorkBook.Close();
                }
                using (FileStream fs = new FileStream(System.IO.Path.Combine(Path, FileName), FileMode.Open, FileAccess.Read))
                {
                    nowWorkBook = new XSSFWorkbook(fs);
                }
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
                        sheetDict[SheetName] = SheetModel;
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
                            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                            float dpiX = graphics.DpiX;
                            float dpiY = graphics.DpiY;
                            foreach (XSSFShape shape in xssfDrawing.GetShapes())
                            {
                                if (shape is XSSFPicture picture)
                                {
                                    XSSFClientAnchor anchor = (XSSFClientAnchor)picture.GetPreferredSize();
                                    if (anchor.Row1 <= rowIndex + 3 && anchor.Row2 >= rowIndex + 3 && anchor.Col1 <= columnIndex && anchor.Col2 >= columnIndex)
                                    {
                                        imageAnchor = anchor;
                                        ImageInfo imageInfoModel = new Model.Evidence.ImageInfo();
                                        imageInfoModel.WidthInExcel = (anchor.Col2 - anchor.Col1) * nowSheet.GetColumnWidthInPixels(0) - ((anchor.Dx1 / 914400.0) * dpiX) + ((anchor.Dx2 / 914400.0) * dpiX);
                                        imageInfoModel.HeightInExcel = (anchor.Row2 - anchor.Row1) * nowSheet.DefaultRowHeightInPoints - ((anchor.Dy1 / 914400.0) * dpiY) + ((anchor.Dy2 / 914400.0) * dpiY);
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
                                        shapePosition.Dx1 = (int)(anchor.Dx1 / 914400.0 * dpiX);
                                        shapePosition.Dy1 = (int)(anchor.Dy1 / 914400.0 * dpiY);
                                        shapePosition.Dx2 = (int)(anchor.Dx2 / 914400.0 * dpiX);
                                        shapePosition.Dy2 = (int)(anchor.Dy2 / 914400.0 * dpiY);
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
                                    shapeModel.Position.X = (shapeModel.Position.Col1 - imageAnchor.Col1) * nowSheet.GetColumnWidthInPixels(0) + shapeModel.Position.Dx1;
                                    shapeModel.Position.Y = (shapeModel.Position.Row1 - imageAnchor.Row1) * nowSheet.DefaultRowHeightInPoints + shapeModel.Position.Dy1;
                                    shapeModel.Position.Width = ((shapeModel.Position.Col2 - imageAnchor.Col1) * nowSheet.GetColumnWidthInPixels(0) - shapeModel.Position.X) + shapeModel.Position.Dx2;
                                    shapeModel.Position.Height = ((shapeModel.Position.Row2 - imageAnchor.Row1) * nowSheet.DefaultRowHeightInPoints - shapeModel.Position.Y) + shapeModel.Position.Dy2;
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
                if (SelectedBlock != null && SelectedBlock.ImageInfo != null)
                {
                    excelImageScaleX = SelectedBlock.ImageInfo.WidthInExcel / SelectedBlock.ImageSource.Width;
                    excelImageScaleY = SelectedBlock.ImageInfo.HeightInExcel / SelectedBlock.ImageSource.Height;
                }
                
                if (SelectedBlock != null && SelectedBlock.ShapeList != null)
                {
                    foreach (Shape shapeModel in SelectedBlock.ShapeList)
                    {
                        if (shapeModel.Type == Model.Evidence.Type.EmptyRectangle)
                        {
                            double x = ExcelHelper.ExcelLengthToCanvasLength(shapeModel.Position.X, canvasImageScale, excelImageScaleX);
                            double y = ExcelHelper.ExcelLengthToCanvasLength(shapeModel.Position.Y, canvasImageScale, excelImageScaleY);
                            double width = ExcelHelper.ExcelLengthToCanvasLength(shapeModel.Position.Width, canvasImageScale, excelImageScaleX);
                            double height = ExcelHelper.ExcelLengthToCanvasLength(shapeModel.Position.Height, canvasImageScale, excelImageScaleY);
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
            shapePosition.Col1 = SelectedBlock.ImageInfo.Anchor.Col1;
            shapePosition.Col2 = SelectedBlock.ImageInfo.Anchor.Col1 + 1;
            shapePosition.Row1 = SelectedBlock.ImageInfo.Anchor.Row1;
            shapePosition.Row2 = SelectedBlock.ImageInfo.Anchor.Row1 + 1;
            shapePosition.Dx1 = 0;
            shapePosition.Dy1 = 0;
            shapePosition.Dx2 = 0;
            shapePosition.Dy2 = 0;

            Shape shapeModel = new Shape();
            shapeModel.Type = Model.Evidence.Type.EmptyRectangle;
            shapeModel.Position = shapePosition;

            if (SelectedBlock.ShapeList == null)
            {
                SelectedBlock.ShapeList = new ObservableCollection<Shape>();
            }
            SelectedBlock.ShapeList.Add(shapeModel);

            double columnWidthInPixels = nowSheet.GetColumnWidthInPixels(0);
            double defaultRowHeightInPoints = nowSheet.DefaultRowHeightInPoints;

            AddEmptyRectangle(shapeModel, 0, 0, columnWidthInPixels, defaultRowHeightInPoints);
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
            ((RectangleViewModel)rectangle.DataContext).Changed += SaveShape;
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            ShapeItems.Add(rectangle);

            shapeModel.ID = Guid.NewGuid().ToString();
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

        private void SaveShape(RectangleViewModel rectangleViewModel)
        {
            foreach (Shape shapeModel in SelectedBlock.ShapeList)
            {
                if (shapeModel.ID == rectangleViewModel.ShapeModel.ID)
                {
                    double x = ExcelHelper.CanvasLengthToExcelLength(rectangleViewModel.X, canvasImageScale, excelImageScaleX);
                    double y = ExcelHelper.CanvasLengthToExcelLength(rectangleViewModel.Y, canvasImageScale, excelImageScaleY);
                    double width = ExcelHelper.CanvasLengthToExcelLength(rectangleViewModel.RectangleWidth, canvasImageScale, excelImageScaleX);
                    double height = ExcelHelper.CanvasLengthToExcelLength(rectangleViewModel.RectangleHeight, canvasImageScale, excelImageScaleY);
                    shapeModel.Position.X = x;
                    shapeModel.Position.Y = y;
                    shapeModel.Position.Width = width;
                    shapeModel.Position.Height = height;

                    return;
                }
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
                        List<int> columnIndexList = new List<int>();
                        List<int> columnLengthList = new List<int>();
                        int sheetIndex = nowWorkBook.GetSheetIndex(sheetName);
                        foreach (Column columnModel in sheetModel.ColumnList)
                        {
                            columnIndexList.Add(columnModel.GetIndex(sheet));
                        }
                        double sheetDefaultColumnWidth = sheet.GetColumnWidthInPixels(0);
                        double sheetDefaultRowHeight = sheet.DefaultRowHeightInPoints;
                        ExcelHelper.ClearSheet(nowWorkBook, sheetName);
                        sheet = (XSSFSheet)nowWorkBook.GetSheet(sheetName);

                        // Column name
                        for (int i = 0; i < sheetModel.ColumnList.Count; ++i)
                        {
                            Column columnModel = sheetModel.ColumnList[i];

                            int columnIndex;
                            int columnLength;
                            if (columnIndexList.Count - 1 >= i)
                            {
                                columnIndex = columnIndexList[i];

                                if (columnIndexList.Count - 1 >= i + 1)
                                {
                                    columnLength = columnIndexList[i + 1] - columnIndex;
                                }
                                else if (i - 1 >= 0)
                                {
                                    columnLength = columnIndex - columnIndexList[i - 1];
                                }
                                else
                                {
                                    columnLength = 10;
                                }
                            }
                            else if (i - 1 >= 0)
                            {
                                if (i - 2 >= 0)
                                {
                                    columnLength = columnIndexList[i - 1] - columnIndexList[i - 2];
                                }
                                else
                                {
                                    columnLength = 10;
                                }

                                columnIndex = columnIndexList[i - 1] + columnLength;
                                columnIndexList.Add(columnIndex);
                            }
                            else
                            {
                                columnIndex = 1;
                                columnLength = 10;
                                columnIndexList.Add(columnIndex);
                            }
                            int blockColumnLength = columnLength - 2;
                            columnLengthList.Add(blockColumnLength);

                            XSSFCell cell = ExcelHelper.GetOrCreateCell(sheet, 1, columnIndex);
                            cell.SetCellValue(columnModel.Name);
                        }

                        int nowRowIndex = 3;

                        for (int i = 0; i < sheetModel.RowTitleList.Count; ++i)
                        {
                            // Row title
                            string nowRowTitle = sheetModel.RowTitleList[i];

                            XSSFCell cell = ExcelHelper.GetOrCreateCell(sheet, nowRowIndex, 1);
                            cell.SetCellValue(nowRowTitle);

                            int nowBlockRowIndex = nowRowIndex + 1;
                            // Block title
                            for (int j = 0; j < sheetModel.ColumnList.Count; ++j)
                            {
                                Column columnModel = sheetModel.ColumnList[j];
                                if (columnModel.BlockList == null)
                                {
                                    continue;
                                }
                                Block blockModel = columnModel.BlockList[i];

                                int blockColumnIndex = columnIndexList[j] + 1;
                                cell = ExcelHelper.GetOrCreateCell(sheet, nowBlockRowIndex, blockColumnIndex);
                                cell.SetCellValue(blockModel.Title);
                            }

                            // Image precomputation
                            int mRowOffsetForImage = 0;
                            int xRowOffsetForImage = 0;
                            int mColumnOffsetForImage = 0;
                            int xColumnOffsetForImage = 0;
                            int imageRowCount = 1;
                            for (int j = 0; j < sheetModel.ColumnList.Count; ++j)
                            {
                                Column columnModel = sheetModel.ColumnList[j];
                                if (columnModel.BlockList == null)
                                {
                                    continue;
                                }
                                Block blockModel = columnModel.BlockList[i];
                                
                                int blockColumnLength = columnLengthList[j];
                                if (blockModel.ImageInfo != null)
                                {
                                    if (blockModel.ImageInfo.WidthInExcel / sheetDefaultColumnWidth > blockColumnLength)
                                    {
                                        double newWidthInExcel = blockColumnLength * sheetDefaultColumnWidth;
                                        double newHeightInExcel = blockModel.ImageInfo.HeightInExcel * (newWidthInExcel / blockModel.ImageInfo.WidthInExcel);
                                        blockModel.ImageInfo.WidthInExcel = newWidthInExcel;
                                        blockModel.ImageInfo.HeightInExcel = newHeightInExcel;
                                    }

                                    if (blockModel.ImageInfo.HeightInExcel / sheetDefaultRowHeight > imageRowCount)
                                    {
                                        imageRowCount = (int)(blockModel.ImageInfo.HeightInExcel / sheetDefaultRowHeight);
                                    }
                                    
                                    if (blockModel.ShapeList != null)
                                    {
                                        foreach (Shape shapeModel in blockModel.ShapeList)
                                        {
                                            if (shapeModel.Type == Model.Evidence.Type.EmptyRectangle)
                                            {
                                                int rowOffsetForImage = (int)(shapeModel.Position.Y / nowSheet.DefaultRowHeightInPoints);
                                                int colOffsetForImage = (int)(shapeModel.Position.X / nowSheet.GetColumnWidthInPixels(0)); 
                                                int fullRowCount = (int)(shapeModel.Position.Height / nowSheet.DefaultRowHeightInPoints);
                                                int fullColCount = (int)(shapeModel.Position.Width / nowSheet.GetColumnWidthInPixels(0));
                                                if (rowOffsetForImage < mRowOffsetForImage)
                                                {
                                                    mRowOffsetForImage = rowOffsetForImage;
                                                }
                                                if (colOffsetForImage < mColumnOffsetForImage)
                                                {
                                                    mColumnOffsetForImage = colOffsetForImage;
                                                }
                                                if (rowOffsetForImage + fullRowCount > xRowOffsetForImage)
                                                {
                                                    xRowOffsetForImage = rowOffsetForImage + fullRowCount;
                                                }
                                                if (colOffsetForImage + fullColCount > xColumnOffsetForImage)
                                                {
                                                    xColumnOffsetForImage = colOffsetForImage + fullColCount;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // Image
                            nowBlockRowIndex += 2;
                            int imageRowIndex = nowBlockRowIndex;
                            if (xRowOffsetForImage > imageRowCount)
                            {
                                imageRowCount = xRowOffsetForImage;
                            }
                            if (mRowOffsetForImage < 0)
                            {
                                imageRowIndex -= mRowOffsetForImage;
                            }
                            int descriptionIndex = imageRowIndex + imageRowCount + 1;
                            for (int j = 0; j < sheetModel.ColumnList.Count; ++j)
                            {
                                Column columnModel = sheetModel.ColumnList[j];
                                if (columnModel.BlockList == null)
                                {
                                    continue;
                                }
                                Block blockModel = columnModel.BlockList[i];
                                IClientAnchor anchor = null;
                                int blockColumnIndex = columnIndexList[j] + 1;

                                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                                float dpiX = graphics.DpiX;
                                float dpiY = graphics.DpiY;

                                if (blockModel.ImageInfo != null)
                                {
                                    ICreationHelper helper = nowWorkBook.GetCreationHelper();
                                    IDrawing drawing = sheet.CreateDrawingPatriarch();
                                    anchor = helper.CreateClientAnchor();

                                    byte[] data = ExcelHelper.ConvertImageSourceToByteArray(blockModel.ImageSource);
                                    int pictureIndex = nowWorkBook.AddPicture(data, PictureType.PNG);

                                    anchor.Col1 = blockColumnIndex;
                                    anchor.Row1 = imageRowIndex;
                                    anchor.Col2 = blockColumnIndex + (int)(blockModel.ImageInfo.WidthInExcel / sheetDefaultColumnWidth);
                                    anchor.Row2 = imageRowIndex + (int)(blockModel.ImageInfo.HeightInExcel / sheetDefaultRowHeight);

                                    anchor.Dx2 = (int)((blockModel.ImageInfo.WidthInExcel % sheetDefaultColumnWidth) / dpiX * 914400);
                                    anchor.Dy2 = (int)((blockModel.ImageInfo.HeightInExcel % sheetDefaultRowHeight) / dpiX * 914400);
                                    IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                                }

                                // Shapes
                                if (blockModel.ShapeList != null)
                                {
                                    foreach (Shape shapeModel in blockModel.ShapeList)
                                    {
                                        double columnWidthInPixels = nowSheet.GetColumnWidthInPixels(0);
                                        double defaultRowHeightInPoints = nowSheet.DefaultRowHeightInPoints;
                                        int row1 = (int)(anchor.Row1 + shapeModel.Position.Y / defaultRowHeightInPoints);
                                        int col1 = (int)(anchor.Col1 + shapeModel.Position.X / columnWidthInPixels);
                                        int row2 = (int)(anchor.Row1 + (shapeModel.Position.Y + shapeModel.Position.Height) / defaultRowHeightInPoints);
                                        int col2 = (int)(anchor.Col1 + (shapeModel.Position.X + shapeModel.Position.Width) / columnWidthInPixels);
                                        int dx1 = (int)((shapeModel.Position.X % columnWidthInPixels) / dpiX * 914400);
                                        int dy1 = (int)((shapeModel.Position.Y % defaultRowHeightInPoints) / dpiY * 914400);
                                        int dx2 = (int)(((shapeModel.Position.X + shapeModel.Position.Width) % columnWidthInPixels) / dpiX * 914400);
                                        int dy2 = (int)(((shapeModel.Position.Y + shapeModel.Position.Height) % defaultRowHeightInPoints) / dpiY * 914400);
                                        ShapeHelper.CreateBorderRectangle(sheet, dx1, dy1, dx2, dy2, col1, row1, col2, row2);
                                    }
                                }

                                // Block description
                                cell = ExcelHelper.GetOrCreateCell(sheet, descriptionIndex, blockColumnIndex);
                                cell.SetCellValue(blockModel.Description);
                            }

                            nowRowIndex = descriptionIndex + 1;
                        }

                        XSSFCell cellEnd = ExcelHelper.GetOrCreateCell(sheet, nowRowIndex + 2, 1);
                        cellEnd.SetCellValue("END");
                    }
                }
            }

            using (var fs = new FileStream(System.IO.Path.Combine(Path, FileName), FileMode.Create, FileAccess.Write))
            {
                nowWorkBook.Write(fs);
            }

            MessageBox.Show("Saved");
        }

        private void ImageSizeChanged(SizeChangedEventArgs e)
        {
            if (SelectedBlock == null || SelectedBlock.ImageSource == null)
            {
                return;
            }
            canvasImageScale = e.NewSize.Height / SelectedBlock.ImageSource.Height;
            SelectedBlock = SelectedBlock;
        }
    }
}
