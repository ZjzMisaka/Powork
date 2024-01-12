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

        public static void ClearSheet(XSSFWorkbook workbook, string name)
        {
            int sheetIndex = workbook.GetSheetIndex(name);
            workbook.RemoveSheetAt(sheetIndex);
            if (sheetIndex != -1)
            {
                ISheet newSheet = workbook.CreateSheet(name);

                workbook.SetSheetOrder(newSheet.SheetName, sheetIndex);
            }
            else
            {
                throw new InvalidOperationException("Sheet with provided name doesn't exist.");
            }
        }

        public static double ExcelLengthToCanvasLength(double length, double canvasImageScale, double excelImageScale)
        {
            return length / excelImageScale * canvasImageScale;
        }

        public static double CanvasLengthToExcelLength(double length, double canvasImageScale, double excelImageScale)
        {
            return length / canvasImageScale * excelImageScale;
        }
    }
}
