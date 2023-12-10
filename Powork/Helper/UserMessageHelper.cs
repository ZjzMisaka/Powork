using Powork.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Powork.Helper
{
    static internal class UserMessageHelper
    {
        static internal void ConvertImageInMessage(UserMessage userMessage)
        {
            foreach (UserMessageBody messageBody in userMessage.MessageBody)
            {
                if (messageBody.Type == ContentType.Picture)
                {
                    byte[] imageBytes = Convert.FromBase64String(messageBody.Content);

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                    }

                    bitmapImage.Freeze();

                    string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + Guid.NewGuid() + ".png");

                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder(); // 或者使用JpegBitmapEncoder等其他编码器
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(fileStream);
                    }

                    messageBody.Content = localFilePath;
                }
            }
        }
    }
}
