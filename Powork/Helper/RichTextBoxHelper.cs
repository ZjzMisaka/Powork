using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using Wpf.Ui.Controls;
using Powork.Model;
using System.IO;
using System.Windows.Media.Imaging;

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

        public static List<UserMessageBody> ConvertFlowDocumentToUserMessage(FlowDocument document)
        {
            List<UserMessageBody> messages = new List<UserMessageBody>();

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
                            messages.Add(new UserMessageBody
                            {
                                Content = run.Text,
                                Type = ContentType.Text
                            });
                        }
                        else if (inline is Hyperlink)
                        {
                            Hyperlink hyperlink = inline as Hyperlink;
                            messages.Add(new UserMessageBody
                            {
                                Content = hyperlink.NavigateUri.ToString(),
                                Type = ContentType.File
                            });
                        }
                        else if (inline is InlineUIContainer)
                        {
                            InlineUIContainer uiContainer = inline as InlineUIContainer;
                            if (uiContainer.Child is System.Windows.Controls.Image)
                            {
                                System.Windows.Controls.Image imageControl = uiContainer.Child as System.Windows.Controls.Image;
                                BitmapSource image = imageControl.Source as BitmapSource;

                                if (image != null)
                                {
                                    string imageContent = ConvertImageToBase64(image);
                                    messages.Add(new UserMessageBody
                                    {
                                        Content = imageContent,
                                        Type = ContentType.Picture
                                    });
                                }
                            }
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
