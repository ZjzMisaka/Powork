using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PowerThreadPool.Results;
using Powork.Constant;
using Powork.CustomEventArgs;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using Powork.Service;
using Powork.View;
using Powork.ViewModel.Inner;
using Wpf.Ui.Controls;

namespace Powork.ViewModel
{
    class MessagePageViewModel : ObservableObject
    {
        private string _needSelectUserIP = null;
        private string _needSelectUserName = null;
        private int _firstMessageID = -1;
        private UserViewModel _nowUser = null;
        private INavigationService _navigationService;

        private bool _isScrollAtBottom;
        public bool IsScrollAtBottom
        {
            get
            {
                return _isScrollAtBottom;
            }
            set
            {
                SetProperty<bool>(ref _isScrollAtBottom, value);
            }
        }

        private bool _scrollToEnd;
        public bool ScrollToEnd
        {
            get
            {
                return _scrollToEnd;
            }
            set
            {
                SetProperty<bool>(ref _scrollToEnd, value);
            }
        }

        private List<UserViewModel> SelectedUserList => UserList.Where(x => x.Selected).ToList();

        private ObservableCollection<TextBlock> _messageList;
        public ObservableCollection<TextBlock> MessageList
        {
            get
            {
                return _messageList;
            }
            set
            {
                SetProperty<ObservableCollection<TextBlock>>(ref _messageList, value);
            }
        }

        private ObservableCollection<UserViewModel> _userList;
        public ObservableCollection<UserViewModel> UserList
        {
            get
            {
                return _userList;
            }
            set
            {
                SetProperty<ObservableCollection<UserViewModel>>(ref _userList, value);
            }
        }
        private bool _pageEnabled;
        public bool PageEnabled
        {
            get
            {
                return _pageEnabled;
            }
            set
            {
                SetProperty<bool>(ref _pageEnabled, value);
            }
        }
        private bool _sendEnabled;
        public bool SendEnabled
        {
            get
            {
                return _sendEnabled;
            }
            set
            {
                SetProperty<bool>(ref _sendEnabled, value);
            }
        }
        private FlowDocument _richTextBoxDocument;
        public FlowDocument RichTextBoxDocument
        {
            get { return _richTextBoxDocument; }
            set
            {
                SetProperty<FlowDocument>(ref _richTextBoxDocument, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand UserClickCommand { get; set; }
        public ICommand CreateTeamCommand { get; set; }
        public ICommand SharedItemsCommand { get; set; }
        public ICommand RemoveUserCommand { get; set; }
        public ICommand ScrollAtTopCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public MessagePageViewModel(INavigationService navigationService, string userIP = null, string userName = null)
        {
            _navigationService = navigationService;

            _needSelectUserIP = userIP;
            _needSelectUserName = userName;

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
            RemoveUserCommand = new RelayCommand(RemoveUser);
            ScrollAtTopCommand = new RelayCommand(ScrollAtTop);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);

            UserList = new ObservableCollection<UserViewModel>();
            UserViewModel needSelectUserViewModel = null;
            foreach (User user in GlobalVariables.UserList)
            {
                UserViewModel userViewModel = new UserViewModel(user);
                if (_needSelectUserIP != null && _needSelectUserName != null)
                {
                    if (userViewModel.IP == _needSelectUserIP && userViewModel.Name == _needSelectUserName)
                    {
                        needSelectUserViewModel = userViewModel;
                    }
                }
                UserList.Add(userViewModel);
            }

            if (needSelectUserViewModel != null)
            {
                UserClick(needSelectUserViewModel);
            }
        }

        private void OnGetMessage(object sender, MessageEventArgs e)
        {
            if (_nowUser == null)
            {
                return;
            }

            UserMessage userMessage = (UserMessage)sender;

            if (userMessage.SenderIP == _nowUser.IP && userMessage.SenderName == _nowUser.Name)
            {
                bool isScrollAtBottom = IsScrollAtBottom;

                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageList.Add(timeTextBlock);
                    MessageList.Add(textBlock);
                });

                if (isScrollAtBottom)
                {
                    ScrollToEnd = true;
                    ScrollToEnd = false;
                }

                e.Received = true;
            }
        }

        private void UserListChanged(object s, EventArgs e)
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
        }

        private void OnGetFile(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show($"{(sender as Model.FileInfo).Name} received successfully.");
        }

        public void InsertImage(string uri)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(uri));
            image.Width = image.Height = 64;
            BlockUIContainer container = new BlockUIContainer(image);
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
            GlobalVariables.UserListChanged += UserListChanged;
            GlobalVariables.GetUserMessage += OnGetMessage;
            GlobalVariables.GetFile += OnGetFile;

            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetUserMessage -= OnGetMessage;
            GlobalVariables.UserListChanged -= UserListChanged;
            GlobalVariables.GetFile -= OnGetFile;
        }

        private void UserClick(UserViewModel userViewModel)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                userViewModel.Selected = true;
                return;
            }
            else
            {
                foreach (UserViewModel userVM in SelectedUserList)
                {
                    userVM.Selected = false;
                }
            }

            MessageList.Clear();
            if (_nowUser != null)
            {
                _nowUser.Selected = false;
            }
            _nowUser = userViewModel;
            _nowUser.Selected = true;
            SendEnabled = true;

            List<UserMessage> messageList = UserMessageRepository.SelectMessgae(_nowUser.IP, _nowUser.Name);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }

            foreach (UserMessage message in messageList)
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

            ScrollToEnd = true;
            ScrollToEnd = false;
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
                        Name = userViewModel.Name,
                        GroupName = userViewModel.GroupName,
                    });
                }
            }

            if (teamMemberList.Count == 0)
            {
                return;
            }

            teamMemberList.Add(new User()
            {
                IP = GlobalVariables.SelfInfo.IP,
                Name = GlobalVariables.SelfInfo.Name,
                GroupName = GlobalVariables.SelfInfo.GroupName,
            });

            Team team = new Team();
            team.ID = Guid.NewGuid().ToString();
            team.MemberList = teamMemberList;
            team.LastModifiedTime = DateTime.Now.ToString(Format.DateTimeFormatWithMilliseconds);

            InputWindowViewModel dataContext = new InputWindowViewModel("Team name");
            InputWindow window = new InputWindow
            {
                DataContext = dataContext
            };
            window.ShowDialog();
            if (!(bool)window.DialogResult)
            {
                return;
            }

            team.Name = dataContext.Value;

            if (string.IsNullOrEmpty(team.Name))
            {
                System.Windows.MessageBox.Show("Name should not be empty");
                return;
            }

            TeamRepository.InsertOrUpdateTeam(team);

            _navigationService.Navigate(typeof(TeamPage), new TeamPageViewModel(team.ID));
        }

        private void SharedItems()
        {
            if (UserList.Count != 1)
            {
                return;
            }

            UserViewModel userViewModel = UserList[0];
            User user = new User();
            user.IP = userViewModel.IP;
            user.Name = userViewModel.Name;

            _navigationService.Navigate(typeof(SharePage), new SharePageViewModel(user));
        }

        private void RemoveUser()
        {
            foreach (UserViewModel user in SelectedUserList)
            {
                UserRepository.RemoveUser(user.IP, user.Name);
                UserList.Remove(user);
            }
        }

        private async void SendMessage()
        {
            if (_nowUser == null)
            {
                return;
            }
            List<TCPMessageBody> tcpMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToMessageBodyList(RichTextBoxDocument);
            RichTextBoxDocument = new FlowDocument();
            UserMessage userMessage = new UserMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = tcpMessageBodyList,
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.UserMessage,
            };

            string message = JsonConvert.SerializeObject(userMessage);

            Task<ExecuteResult<Exception>> task = GlobalVariables.TcpServerClient.SendMessage(message, _nowUser.IP, GlobalVariables.TcpPort);
            Exception ex = (await task).Result;

            MessageHelper.ConvertImageInMessage(userMessage);

            TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(userMessage);
            TextBlock textBlock = TextBlockHelper.GetMessageControl(userMessage);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageList.Add(timeTextBlock);
                MessageList.Add(textBlock);
            });
            ScrollToEnd = true;
            ScrollToEnd = false;
            if (ex != null)
            {
                List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = "Send failed: User not online" }];
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

                UserMessageRepository.InsertMessage(errorMessage, _nowUser.IP, _nowUser.Name);
            }
            UserMessageRepository.InsertMessage(userMessage, _nowUser.IP, _nowUser.Name);
        }

        private void ScrollAtTop()
        {
            if (_firstMessageID == -1)
            {
                return;
            }

            List<UserMessage> messageList = UserMessageRepository.SelectMessgae(_nowUser.IP, _nowUser.Name, _firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }
            int index = 0;
            foreach (UserMessage message in messageList)
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
