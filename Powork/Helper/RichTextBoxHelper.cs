using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Powork.Model;
using Powork.Repository;
using Wpf.Ui.Controls;

namespace Powork.Helper
{
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                "Document",
                typeof(FlowDocument),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (RichTextBox)d;

            richTextBox.Document = (FlowDocument)e.NewValue;
        }

        public static FlowDocument GetDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentProperty);
        }

        public static void SetDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentProperty, value);
        }

        public static List<TCPMessageBody> ConvertFlowDocumentToMessageBodyList(FlowDocument document)
        {
            List<TCPMessageBody> messages = new List<TCPMessageBody>();

            foreach (Block block in document.Blocks)
            {
                if (block is Paragraph)
                {
                    Paragraph paragraph = block as Paragraph;
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run)
                        {
                            Run run = inline as Run;
                            messages.Add(new TCPMessageBody
                            {
                                Content = run.Text,
                                Type = ContentType.Text
                            });
                        }
                        else if (inline is Hyperlink)
                        {
                            Hyperlink hyperlink = inline as Hyperlink;
                            string guid = Guid.NewGuid().ToString();
                            string name = new DirectoryInfo(hyperlink.NavigateUri.LocalPath).Name;
                            messages.Add(new TCPMessageBody
                            {
                                Content = name,
                                ID = guid,
                                Type = ContentType.File
                            });
                            FileRepository.InsertFile(guid, hyperlink.NavigateUri.LocalPath);
                        }
                        else if (inline is InlineUIContainer)
                        {
                            InlineUIContainer uiContainer = inline as InlineUIContainer;
                            if (uiContainer.Child is System.Windows.Controls.Image)
                            {
                                System.Windows.Controls.Image imageControl = uiContainer.Child as System.Windows.Controls.Image;

                                if (imageControl.Source is BitmapSource image)
                                {
                                    string imageContent = ConvertImageToBase64(image);
                                    messages.Add(new TCPMessageBody
                                    {
                                        Content = imageContent,
                                        Type = ContentType.Picture
                                    });
                                }
                            }
                        }
                    }
                }
                else if (block is BlockUIContainer)
                {
                    if ((block as BlockUIContainer).Child is Image)
                    {
                        Image imageControl = (block as BlockUIContainer).Child as Image;

                        if (imageControl.Source is BitmapSource image)
                        {
                            string imageContent = ConvertImageToBase64(image);
                            messages.Add(new TCPMessageBody
                            {
                                Content = imageContent,
                                Type = ContentType.Picture
                            });
                        }
                    }
                    else if ((block as BlockUIContainer).Child is System.Windows.Controls.Image)
                    {
                        System.Windows.Controls.Image imageControl = (block as BlockUIContainer).Child as System.Windows.Controls.Image;

                        if (imageControl.Source is BitmapSource image)
                        {
                            string imageContent = ConvertImageToBase64(image);
                            messages.Add(new TCPMessageBody
                            {
                                Content = imageContent,
                                Type = ContentType.Picture
                            });
                        }
                    }
                }
            }

            return messages;
        }

        private static string ConvertImageToBase64(BitmapSource image)
        {
            // 将BitmapSource转换为字节数组
            byte[] data;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            // 将字节数组转换为Base64字符串
            return Convert.ToBase64String(data);
        }
    }
}
