using Powork.Control;
using Powork.Model;
using System.Diagnostics;
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
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
                timeTextBlock.Foreground = Brushes.LightGreen;
                timeTextBlock.Text = userMessage.Time;
            });
            
            return timeTextBlock;
        }
        static public TextBlock GetMessageControl (UserMessage userMessage)
        {
            TextBlock textBlock = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (userMessage.Type == MessageType.Message)
                {
                    List<UserMessageBody> userMessageBodyList = userMessage.MessageBody;
                    textBlock = new SelectableTextBlock();

                    textBlock.Foreground = Brushes.White;

                    foreach (UserMessageBody body in userMessageBodyList)
                    {
                        if (textBlock.Inlines.Count > 0)
                        {
                            textBlock.Inlines.Add(new LineBreak());
                        }

                        if (body.Type == ContentType.Text)
                        {
                            Run run = new Run(body.Content);
                            textBlock.Inlines.Add(run);
                        }
                        else if (body.Type == ContentType.Picture)
                        {
                            InlineUIContainer container = new InlineUIContainer(ButtonHelper.CreateImageButton(body.Content, new RoutedEventHandler((s, e) =>
                            {
                                Process p = new Process();
                                p.StartInfo = new ProcessStartInfo(body.Content)
                                {
                                    UseShellExecute = true
                                };
                                p.Start();
                            })));
                            textBlock.Inlines.Add(container);
                        }
                        else if (body.Type == ContentType.File)
                        {
                            InlineUIContainer container = new InlineUIContainer(ButtonHelper.CreateImageButton(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image\\file.png"), new RoutedEventHandler((s, e) => 
                            {
                                GlobalVariables.TcpServerClient.RequestFile(body.Content.Split(" | ")[1], userMessage.IP, GlobalVariables.TcpPort);
                            })));
                            textBlock.Inlines.Add(container);

                            textBlock.Inlines.Add(new LineBreak());

                            string name = body.Content.Split(" | ")[0];
                            Run run = new Run(name);
                            textBlock.Inlines.Add(run);
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
                }
                else if (userMessage.Type == MessageType.Error)
                {
                    List<UserMessageBody> userMessageBodyList = userMessage.MessageBody;
                    textBlock = new TextBlock();

                    textBlock.Foreground = Brushes.Pink;

                    foreach (UserMessageBody body in userMessageBodyList)
                    {
                        if (textBlock.Inlines.Count > 0)
                        {
                            textBlock.Inlines.Add(new LineBreak());
                        }

                        Run run = new Run(body.Content);
                        textBlock.Inlines.Add(run);
                    }

                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                }
            });
            

            return textBlock;
        }
    }
}
