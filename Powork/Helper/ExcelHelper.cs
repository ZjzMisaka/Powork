using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using NPOI.SS.UserModel;
using Powork.Model.Evidence;
using NPOI.HSSF.UserModel;
using NPOI;

namespace Powork.Helper
{
    public static class ExcelHelper
    {
        public static XSSFCell GetOrCreateCell(XSSFSheet sheet, int rowIndex, int columnIndex)
        {
            XSSFRow row = (XSSFRow)sheet.GetRow(rowIndex) ?? (XSSFRow)sheet.CreateRow(rowIndex);
            XSSFCell cell = (XSSFCell)row.GetCell(columnIndex) ?? (XSSFCell)row.CreateCell(columnIndex);

            return cell;
        }

        public static ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static byte[] ConvertImageSourceToByteArray(ImageSource imageSource)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();

            byte[] bytes = null;
            BitmapSource bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        public static void ClearSheet(ISheet sheet)
        {
            if (sheet != null)
            {
                for (int rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row != null)
                    {
                        for (int cellIndex = row.FirstCellNum; cellIndex < row.LastCellNum; cellIndex++)
                        {
                            ICell cell = row.GetCell(cellIndex);
                            if (cell != null)
                            {
                                // 清空单元格内容
                                cell.SetCellValue(string.Empty);
                            }
                        }
                    }
                }
            }

            int lastRowNum = sheet.LastRowNum;
            for (int rowIndex = lastRowNum; rowIndex >= 0; rowIndex--)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null)
                {
                    sheet.RemoveRow(row);
                }
            }

            if (sheet is HSSFSheet)
            {
                HSSFPatriarch drawingPatriarch = (HSSFPatriarch)sheet.DrawingPatriarch;
                drawingPatriarch.Clear();
            }
            else if (sheet is XSSFSheet xssfSheet)
            {
                xssfSheet.CreateDrawingPatriarch();
            }
        }
    }
}
