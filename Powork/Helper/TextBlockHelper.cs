using Powork.Model;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Powork.Helper
{
    static public class TextBlockHelper
    {
        static public TextBlock GetTimeControl(UserMessage userMessage)
        {
            TextBlock timeTextBlock = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                timeTextBlock = new TextBlock();
                if (userMessage.IP == GlobalVariables.SelfInfo[0].IP && userMessage.Name == GlobalVariables.SelfInfo[0].Name)
                {
                    timeTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    timeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                }
                timeTextBlock.Foreground = Brushes.AliceBlue;
                timeTextBlock.Text = userMessage.Time;
            });
            
            return timeTextBlock;
        }
        static public TextBlock GetMessageControl (UserMessage userMessage)
        {
            TextBlock textBlock = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                List<UserMessageBody> userMessageBodyList = userMessage.MessageBody;
                textBlock = new TextBlock();

                textBlock.Foreground = Brushes.White;

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

                if (userMessage.IP == GlobalVariables.SelfInfo[0].IP && userMessage.Name == GlobalVariables.SelfInfo[0].Name)
                {
                    textBlock.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                }
            });
            

            return textBlock;
        }
    }
}
