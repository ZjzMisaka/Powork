using System.IO;
using System.Windows.Media.Imaging;
using Powork.Model;

namespace Powork.Helper
{
    internal static class MessageHelper
    {
        internal static void ConvertImageInMessage(TCPMessage userMessage)
        {
            foreach (TCPMessageBody messageBody in userMessage.MessageBody)
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

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", DateTime.Now.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string localFilePath = Path.Combine(directoryPath, DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + Guid.NewGuid() + ".png");

                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(fileStream);
                    }

                    messageBody.Content = localFilePath;
                }
            }
        }
    }
}
