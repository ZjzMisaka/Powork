using Powork.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Powork.Helper
{
    static public class TextBlockHelper
    {
        static public TextBlock GetMessageControl (List<UserMessageBody> userMessageBodyList)
        {
            TextBlock textBlock = new TextBlock();

            foreach (UserMessageBody body in userMessageBodyList) 
            {
                if (body.Type == ContentType.Text)
                {
                    Run run = new Run(body.Content);
                    textBlock.Inlines.Add(run);
                }
                else if (body.Type == ContentType.Picture)
                {
                    Image image = new Image
                    {
                        Source = new BitmapImage(new Uri(body.Content)),
                    };
                    image.MouseLeftButtonUp += (s, e) => 
                    {
                    
                    };
                    InlineUIContainer container = new InlineUIContainer(image);
                    textBlock.Inlines.Add(container);
                }
                else if (body.Type == ContentType.File)
                {
                    Image image = new Image
                    {
                        Source = new BitmapImage(),
                    };
                    image.MouseLeftButtonUp += (s, e) =>
                    {

                    };
                    InlineUIContainer container = new InlineUIContainer(image);
                    textBlock.Inlines.Add(container);
                }
            }

            return textBlock;
        }
    }
}
