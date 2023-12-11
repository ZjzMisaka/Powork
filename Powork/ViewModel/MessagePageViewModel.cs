using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Powork.ViewModel
{
    class MessagePageViewModel : ObservableObject
    {
        private int firstMessageID = -1;
        private User nowUser = new User() { GroupName = "1", IP = "1", Name = "1" };

        public ObservableCollection<TextBlock> messageList;
        public ObservableCollection<TextBlock> MessageList
        {
            get
            {
                return messageList;
            }
            set
            {
                SetProperty<ObservableCollection<TextBlock>>(ref messageList, value);
            }
        }

        private ObservableCollection<User> userList;
        public ObservableCollection<User> UserList
        {
            get
            {
                return userList;
            }
            set
            {
                SetProperty<ObservableCollection<User>>(ref userList, value);
            }
        }
        private bool pageEnabled;
        public bool PageEnabled
        {
            get
            {
                return pageEnabled;
            }
            set
            {
                SetProperty<bool>(ref pageEnabled, value);
            }
        }
        private bool sendEnabled;
        public bool SendEnabled
        {
            get
            {
                return sendEnabled;
            }
            set
            {
                SetProperty<bool>(ref sendEnabled, value);
            }
        }
        private FlowDocument richTextBoxDocument;
        public FlowDocument RichTextBoxDocument
        {
            get { return richTextBoxDocument; }
            set
            {
                SetProperty<FlowDocument>(ref richTextBoxDocument, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand UserClickCommand { get; set; }
        public ICommand ScrollAtTopCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public MessagePageViewModel()
        {
            PageEnabled = true;
            SendEnabled = false;
            RichTextBoxDocument = new FlowDocument();

            MessageList = new ObservableCollection<TextBlock>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SendMessageCommand = new RelayCommand(SendMessage);
            UserClickCommand = new RelayCommand<User>(UserClick);
            ScrollAtTopCommand = new RelayCommand(ScrollAtTop);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);

            UserList = GlobalVariables.UserList;
            GlobalVariables.UserListChanged += (s, e) =>
            {
                UserList = (ObservableCollection<User>)s;
            };

            GlobalVariables.GetMessage += (s, e) =>
            {
                if (nowUser == null)
                {
                    return;
                }

                UserMessage userMessage = (UserMessage)s;
                UserMessageRepository.InsertMessage(userMessage, GlobalVariables.SelfInfo[0].IP, GlobalVariables.SelfInfo[0].Name);

                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
                Application.Current.Dispatcher.Invoke(() =>
                {

                    MessageList.Add(timeTextBlock);
                    MessageList.Add(textBlock);
                });
            };
        }

        public void InsertImage(string uri)
        {
            var image = new Wpf.Ui.Controls.Image();
            image.Source = new BitmapImage(new Uri(uri));
            image.Width = image.Height = 64;
            var container = new BlockUIContainer(image);
            RichTextBoxDocument.Blocks.Add(container);
        }


        public void InsertFile(string displayText, string url)
        {
            Hyperlink link = new Hyperlink()
            {
                NavigateUri = new Uri(url),
                Foreground = Brushes.AliceBlue,
                TextDecorations = TextDecorations.Underline
            };

            link.RequestNavigate += (sender, e) =>
            {
                string uri = e.Uri.LocalPath;
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(uri)
                {
                    UseShellExecute = true
                };
                p.Start();
                e.Handled = true;
            };

            link.Inlines.Add(displayText);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(link);

            RichTextBoxDocument.Blocks.Add(paragraph);
        }

        public void InsertText(string text)
        {
            Run run = new Run(text);
            Paragraph paragraph = new Paragraph(run);
            RichTextBoxDocument.Blocks.Add(paragraph);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
        }

        private void WindowClosed()
        {
        }

        private void UserClick(User user)
        {
            if (nowUser == null || nowUser.IP == user.IP || nowUser.Name == user.Name)
            {
                return;
            }

            MessageList.Clear();
            nowUser = user;
            SendEnabled = true;

            List<UserMessage> messageList = UserMessageRepository.SelectMessgae(nowUser.IP, nowUser.Name);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
            }
            
            foreach (UserMessage message in messageList)
            {
                if (message.Type == MessageType.Message)
                {
                    TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(message);
                    TextBlock textBlock = TextBlockHelper.GetMessageControl(message);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Add(timeTextBlock);
                        MessageList.Add(textBlock);
                    });
                }
                else if (message.Type == MessageType.Error)
                {
                    TextBlock textBlock = TextBlockHelper.GetMessageControl(message);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Add(textBlock);
                    });
                }
            }
        }

        private void SendMessage()
        {
            if (nowUser == null)
            {
                return;
            }
            List<UserMessageBody> userMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToUserMessage(RichTextBoxDocument);
            UserMessage userMessage = new UserMessage
            {
                IP = GlobalVariables.LocalIP.ToString(),
                MessageBody = userMessageBodyList,
                Name = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.Message,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            string message = JsonConvert.SerializeObject(userMessage);

            //bool sendSucceed = GlobalVariables.TcpServerClient.SendMessage(message, nowUser.IP, GlobalVariables.TcpPort);

            UserMessageHelper.ConvertImageInMessage(userMessage);

            TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
            TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageList.Add(timeTextBlock);
                MessageList.Add(textBlock);
            });
            if (!true)
            {
                List<UserMessageBody> errorContent = [new UserMessageBody() { Content = "Send failed: User not online" }];
                UserMessage errorMessage = new UserMessage()
                {
                    Type = MessageType.Error,
                    MessageBody = errorContent,
                };
                TextBlock errorTextBlock = TextBlockHelper.GetMessageControl(errorMessage);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageList.Add(errorTextBlock);
                });

                UserMessageRepository.InsertMessage(errorMessage, nowUser.IP, nowUser.Name);
            }
            UserMessageRepository.InsertMessage(userMessage, nowUser.IP, nowUser.Name);

            RichTextBoxDocument = new FlowDocument();
        }

        private void ScrollAtTop()
        {
            if (firstMessageID == -1)
            {
                return;
            }

            List<UserMessage> messageList = UserMessageRepository.SelectMessgae(nowUser.IP, nowUser.Name, firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
            }
            int index = 0;
            foreach (UserMessage message in messageList)
            {
                if (message.Type == MessageType.Message)
                {
                    TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(message);
                    TextBlock textBlock = TextBlockHelper.GetMessageControl(message);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Insert(index++, timeTextBlock);
                        MessageList.Insert(index++, textBlock);
                    });
                }
                else if (message.Type == MessageType.Error)
                {
                    TextBlock textBlock = TextBlockHelper.GetMessageControl(message);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Insert(index++, textBlock);
                    });
                }
            }
        }

        private void Drop(DragEventArgs args)
        {
            string[] pathList = (string[])args.Data.GetData(DataFormats.FileDrop, false);
            foreach (string path in pathList) 
            {
                FileAttributes attr = File.GetAttributes(path);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    InsertFile("Send directory: " + new DirectoryInfo(path).Name, path);
                }
                else
                {
                    string extension = Path.GetExtension(path).ToLower();
                    if (extension == ".png" || extension == ".jpg" || extension == ".bmp")
                    {
                        InsertImage(path);
                    }
                    else
                    {
                        InsertFile("Send file: " + Path.GetFileName(path), path);
                    }
                }
            }
        }
    }
}