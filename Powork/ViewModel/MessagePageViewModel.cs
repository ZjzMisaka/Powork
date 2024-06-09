using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using Powork.Service;
using Powork.View;
using Powork.ViewModel.Inner;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Powork.ViewModel
{
    class MessagePageViewModel : ObservableObject
    {
        private int firstMessageID = -1;
        private UserViewModel nowUser = new UserViewModel() { GroupName = "1", IP = "1", Name = "1" };
        private List<UserViewModel> selectedUserList = new List<UserViewModel>();
        private INavigationService _navigationService;

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

        private ObservableCollection<UserViewModel> userList;
        public ObservableCollection<UserViewModel> UserList
        {
            get
            {
                return userList;
            }
            set
            {
                SetProperty<ObservableCollection<UserViewModel>>(ref userList, value);
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
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand UserClickCommand { get; set; }
        public ICommand CreateTeamCommand { get; set; }
        public ICommand SharedItemsCommand { get; set; }
        public ICommand ScrollAtTopCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public MessagePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            PageEnabled = true;
            SendEnabled = false;
            RichTextBoxDocument = new FlowDocument();

            MessageList = new ObservableCollection<TextBlock>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            SendMessageCommand = new RelayCommand(SendMessage);
            UserClickCommand = new RelayCommand<UserViewModel>(UserClick);
            CreateTeamCommand = new RelayCommand(CreateTeam);
            SharedItemsCommand = new RelayCommand(SharedItems);
            ScrollAtTopCommand = new RelayCommand(ScrollAtTop);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);

            UserList = new ObservableCollection<UserViewModel>();
            foreach (User user in GlobalVariables.UserList)
            {
                UserList.Add(new UserViewModel(user));
            }
            GlobalVariables.UserListChanged += (s, e) =>
            {
                foreach (User user in (ObservableCollection<User>)s)
                {
                    bool contain = false;
                    foreach (UserViewModel userVM in UserList)
                    {
                        if (userVM.IP == user.IP && userVM.Name == user.Name)
                        {
                            contain = true;
                        }
                    }
                    if (!contain)
                    {
                        UserList.Add(new UserViewModel(user));
                    }
                }
            };

            GlobalVariables.GetMessage += OnGetMessage;
        }

        private void OnGetMessage(object sender, EventArgs e)
        {
            if (nowUser == null)
            {
                return;
            }

            TCPMessage userMessage = (TCPMessage)sender;

            if (userMessage.Type != MessageType.UserMessage)
            {
                return;
            }

            if (userMessage.SenderIP == nowUser.IP && userMessage.SenderName == nowUser.Name)
            {
                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageList.Add(timeTextBlock);
                    MessageList.Add(textBlock);
                });
            }
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

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetMessage -= OnGetMessage;
        }

        private void UserClick(UserViewModel userViewModel)
        {
            if (nowUser == null)
            {
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                selectedUserList.Add(userViewModel);
                userViewModel.Selected = true;
                return;
            }
            else
            {
                foreach (UserViewModel userVM in selectedUserList)
                {
                    userVM.Selected = false;
                }
                selectedUserList.Clear();
            }

            MessageList.Clear();
            nowUser.Selected = false;
            nowUser = userViewModel;
            nowUser.Selected = true;
            SendEnabled = true;

            List<TCPMessage> messageList = UserMessageRepository.SelectMessgae(nowUser.IP, nowUser.Name);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
            }
            
            foreach (TCPMessage message in messageList)
            {
                if (message.Type == MessageType.UserMessage)
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

        private void CreateTeam()
        {
            if (UserList.Count == 0)
            {
                return;
            }

            List<User> teamMemberList = new List<User>();

            foreach (UserViewModel userViewModel in UserList)
            {
                if (userViewModel.Selected)
                {
                    teamMemberList.Add(new User()
                    {
                        IP = userViewModel.IP,
                        Name = userViewModel.Name
                    });
                }
            }

            if (teamMemberList.Count == 0)
            {
                return;
            }

            teamMemberList.Add(new User()
            {
                IP = GlobalVariables.SelfInfo[0].IP,
                Name = GlobalVariables.SelfInfo[0].Name,
            });

            Team team = new Team();
            team.ID = Guid.NewGuid().ToString();
            team.MemberList = teamMemberList;

            InputWindowViewModel dataContext = new InputWindowViewModel("Team name");
            InputWindow window = new InputWindow
            {
                DataContext = dataContext
            };
            window.ShowDialog();
            team.Name = dataContext.Value;

            if (string.IsNullOrEmpty(team.Name))
            {
                System.Windows.MessageBox.Show("Name should not be empty");
                return;
            }

            TeamRepository.InsertTeam(team);

            _navigationService.Navigate(typeof(TeamPage), new TeamPageViewModel());
        }

        private void SharedItems()
        {
            if (UserList.Count != 1)
            {
                return;
            }

            UserViewModel userViewModel = UserList[0];

            _navigationService.Navigate(typeof(SharePage), new SharePageViewModel(userViewModel));
        }

        private void SendMessage()
        {
            if (nowUser == null)
            {
                return;
            }
            List<TCPMessageBody> userMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToMessageBodyList(RichTextBoxDocument);
            TCPMessage userMessage = new TCPMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = userMessageBodyList,
                SenderName = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.UserMessage,
            };

            string message = JsonConvert.SerializeObject(userMessage);

            Exception ex = GlobalVariables.TcpServerClient.SendMessage(message, nowUser.IP, GlobalVariables.TcpPort);

            MessageHelper.ConvertImageInMessage(userMessage);

            TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
            TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageList.Add(timeTextBlock);
                MessageList.Add(textBlock);
            });
            if (ex != null)
            {
                List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = "Send failed: User not online" }];
                TCPMessage errorMessage = new TCPMessage()
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

            List<TCPMessage> messageList = UserMessageRepository.SelectMessgae(nowUser.IP, nowUser.Name, firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
            }
            int index = 0;
            foreach (TCPMessage message in messageList)
            {
                if (message.Type == MessageType.UserMessage)
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
                if (FileHelper.GetType(path) == FileHelper.Type.Directory)
                {
                    InsertFile("Send directory: " + new DirectoryInfo(path).Name, path);
                }
                else if (FileHelper.GetType(path) == FileHelper.Type.Image)
                {
                    InsertImage(path);
                }
                else if (FileHelper.GetType(path) == FileHelper.Type.File)
                {
                    InsertFile("Send file: " + Path.GetFileName(path), path);
                }
            }
        }
    }
}