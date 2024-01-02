using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
    }
}
