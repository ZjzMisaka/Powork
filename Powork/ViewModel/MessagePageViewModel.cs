using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public MessagePageViewModel()
        {
            PageEnabled = true;
            RichTextBoxDocument = new FlowDocument();

            MessageList = new ObservableCollection<TextBlock>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SendMessageCommand = new RelayCommand(SendMessage);
            UserClickCommand = new RelayCommand<User>(UserClick);

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
            nowUser = user;

            List<UserMessage> messageList = UserMessageRepository.SelectMessgae(nowUser.IP, nowUser.Name);
            foreach (UserMessage message in messageList) 
            {
                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(message);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(message);
                Application.Current.Dispatcher.Invoke(() =>
                {

                    MessageList.Add(timeTextBlock);
                    MessageList.Add(textBlock);
                });
                MessageList.Add(textBlock);
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
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            UserMessageRepository.InsertMessage(userMessage, nowUser.IP, nowUser.Name);
            string message = JsonConvert.SerializeObject(userMessage);
            GlobalVariables.TcpServerClient.SendMessage(message, "", GlobalVariables.TcpPort);

            TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
            TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
            Application.Current.Dispatcher.Invoke(() =>
            {

                MessageList.Add(timeTextBlock);
                MessageList.Add(textBlock);
            });

            RichTextBoxDocument = new FlowDocument();
        }
    }
}
