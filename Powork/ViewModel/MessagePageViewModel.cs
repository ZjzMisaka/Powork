using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PowerThreadPool;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace Powork.ViewModel
{
    class MessagePageViewModel : ObservableObject
    {
        private User nowUser;

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
        }

        private void SendMessage()
        {
            List<UserMessageBody> userMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToUserMessage(RichTextBoxDocument);
            UserMessage userMessage = new UserMessage
            {
                IP = GlobalVariables.LocalIP.ToString(),
                MessageBody = userMessageBodyList,
                Name = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.Message,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            UserMessageRepository.InsertMessage(userMessage);
            string message = JsonConvert.SerializeObject(userMessage);
            GlobalVariables.TcpServerClient.SendMessage(message, "", GlobalVariables.TcpPort);
        }
    }
}
