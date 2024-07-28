using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Ookii.Dialogs.Wpf;
using Pi18n;
using Powork.Constant;
using Powork.Control;
using Powork.Model;
using Powork.Repository;
using Wpf.Ui.Controls;

namespace Powork.Helper
{
    public static class TextBlockHelper
    {
        public static TextBlock GetTimeControl(TCPMessageBase tcpMessage, bool showName = false)
        {
            TextBlock timeTextBlock = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                timeTextBlock = new TextBlock();
                if (UserRepository.IsSelf(tcpMessage.SenderIP, tcpMessage.SenderName))
                {
                    timeTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    timeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                }
                timeTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "TimeTextBrush");
                string timeStr = null;
                if (DateTime.TryParse(tcpMessage.Time, out DateTime dateTime))
                {
                    timeStr = dateTime.ToString(Format.DateTimeFormatWithSeconds);
                }
                if (showName)
                {
                    timeTextBlock.Text = tcpMessage.SenderName + " [" + timeStr + "]";
                }
                else
                {
                    timeTextBlock.Text = timeStr;
                }
            });

            return timeTextBlock;
        }
        public static TextBlock GetMessageControl(TCPMessageBase tcpMessage)
        {
            TextBlock textBlock = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (tcpMessage.Type == MessageType.UserMessage || tcpMessage.Type == MessageType.TeamMessage)
                {
                    List<TCPMessageBody> userMessageBodyList = tcpMessage.MessageBody;
                    textBlock = new SelectableTextBlock();

                    textBlock.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");

                    foreach (TCPMessageBody body in userMessageBodyList)
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
                                if (!System.IO.Path.Exists(body.Content))
                                {
                                    System.Windows.MessageBox.Show($"{ResourceManager.Instance["NoSuchFile"]}: {body.Content}");
                                    return;
                                }
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
                            InlineUIContainer container = new InlineUIContainer(ButtonHelper.CreateImageButton(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image\\file.png"), new RoutedEventHandler((s, e) =>
                            {
                                VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog
                                {
                                    SelectedPath = AppDomain.CurrentDomain.BaseDirectory,
                                    Description = "Select a folder",
                                    UseDescriptionForTitle = true
                                };

                                bool result = (bool)fbd.ShowDialog();

                                if (result && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                {
                                    GlobalVariables.TcpServerClient.RequestFile(body.ID, tcpMessage.SenderIP, GlobalVariables.TcpPort, fbd.SelectedPath);
                                }
                            })));
                            textBlock.Inlines.Add(container);

                            textBlock.Inlines.Add(new LineBreak());

                            string name = body.Content;
                            Run run = new Run(name);
                            textBlock.Inlines.Add(run);
                        }
                    }

                    if (UserRepository.IsSelf(tcpMessage.SenderIP, tcpMessage.SenderName))
                    {
                        textBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    else
                    {
                        textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                }
                else if (tcpMessage.Type == MessageType.Error)
                {
                    List<TCPMessageBody> tcpMessageBodyList = tcpMessage.MessageBody;
                    textBlock = new TextBlock();

                    textBlock.SetResourceReference(TextBlock.ForegroundProperty, "ErrorTextBrush");

                    foreach (TCPMessageBody body in tcpMessageBodyList)
                    {
                        if (textBlock.Inlines.Count > 0)
                        {
                            textBlock.Inlines.Add(new LineBreak());
                        }

                        Run run = new Run(body.Content);
                        textBlock.Inlines.Add(run);
                    }

                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.TextAlignment = TextAlignment.Center;
                }
            });


            return textBlock;
        }
    }
}
