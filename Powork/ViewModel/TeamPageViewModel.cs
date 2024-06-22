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
using Powork.ViewModel.Inner;
using Windows.Networking.NetworkOperators;
using Wpf.Ui.Controls;

namespace Powork.ViewModel
{
    class TeamPageViewModel : ObservableObject
    {
        private string _needSelectTeamID = null;
        private int _firstMessageID = -1;
        private TeamViewModel _nowTeam = null;
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
        private bool _textboxScrollToEnd;
        public bool TextboxScrollToEnd
        {
            get
            {
                return _textboxScrollToEnd;
            }
            set
            {
                SetProperty<bool>(ref _textboxScrollToEnd, value);
            }
        }

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

        private ObservableCollection<TeamViewModel> _teamList;
        public ObservableCollection<TeamViewModel> TeamList
        {
            get
            {
                return _teamList;
            }
            set
            {
                SetProperty<ObservableCollection<TeamViewModel>>(ref _teamList, value);
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
        public ICommand ManageTeamMemberCommand { get; set; }
        public ICommand GetTeamMemberCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand TeamClickCommand { get; set; }
        public ICommand RemoveTeamCommand { get; set; }
        public ICommand ScrollAtTopCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public TeamPageViewModel(string needSelectTeamID = null)
        {
            _needSelectTeamID = needSelectTeamID;
            PageEnabled = true;
            SendEnabled = false;
            RichTextBoxDocument = new FlowDocument();

            MessageList = new ObservableCollection<TextBlock>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            ManageTeamMemberCommand = new RelayCommand(ManageTeamMember);
            GetTeamMemberCommand = new RelayCommand(GetTeamMember);
            SendMessageCommand = new RelayCommand(SendMessage);
            TeamClickCommand = new RelayCommand<TeamViewModel>(TeamClick);
            RemoveTeamCommand = new RelayCommand(RemoveTeam);
            ScrollAtTopCommand = new RelayCommand(ScrollAtTop);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);

            TeamList = new ObservableCollection<TeamViewModel>();
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetTeamMessage += OnGetMessage;
            GlobalVariables.GetFile += OnGetFile;

            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }

            List<Team> teamList = TeamRepository.SelectTeam();
            TeamViewModel needSelectTeamViewModel = null;
            foreach (Team team in teamList)
            {
                TeamViewModel teamViewModel = new TeamViewModel(team);
                if (_needSelectTeamID != null && teamViewModel.ID == _needSelectTeamID)
                {
                    needSelectTeamViewModel = teamViewModel;
                }
                TeamList.Add(teamViewModel);
            }

            if (needSelectTeamViewModel != null)
            {
                TeamClick(needSelectTeamViewModel);
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetTeamMessage -= OnGetMessage;
            GlobalVariables.GetFile -= OnGetFile;
        }

        private void OnGetMessage(object sender, MessageEventArgs e)
        {
            if (_nowTeam == null)
            {
                return;
            }


            TeamMessage teamMessage = (TeamMessage)sender;

            if (!TeamRepository.ContainMember(teamMessage.TeamID, teamMessage.SenderIP, teamMessage.SenderName))
            {
                return;
            }

            if (teamMessage.TeamID == _nowTeam.ID)
            {
                bool isScrollAtBottom = IsScrollAtBottom;

                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(teamMessage, true);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(teamMessage);
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

        private void ManageTeamMember()
        {
            List<User> allUserIncludeSelf = UserRepository.SelectUser();
            allUserIncludeSelf.Add(GlobalVariables.SelfInfo);
            ManageTeamMemberWindowViewModel dataContext = new ManageTeamMemberWindowViewModel(allUserIncludeSelf, TeamRepository.SelectTeamMember(_nowTeam.ID));
            ManageTeamMemberWindow window = new ManageTeamMemberWindow
            {
                DataContext = dataContext
            };
            window.ShowDialog();
            if (!(bool)window.DialogResult)
            {
                return;
            }

            List<UserViewModel> newTeamMemberList = dataContext.TeamUserList.ToList();
            Team nowTeam = TeamRepository.SelectTeam(_nowTeam.ID);
            nowTeam.MemberList = new List<User>();
            foreach (UserViewModel newTeamMembe in newTeamMemberList)
            {
                nowTeam.MemberList.Add(new User()
                {
                    IP = newTeamMembe.IP,
                    GroupName = newTeamMembe.GroupName,
                    Name = newTeamMembe.Name,
                });
            }
            nowTeam.LastModifiedTime = DateTime.Now.ToString(Format.DateTimeFormatWithMilliseconds);
            TeamRepository.RemoveTeam(nowTeam.ID);
            TeamRepository.InsertOrUpdateTeam(nowTeam);
        }

        private void GetTeamMember()
        {
            SelectUserWindowViewModel dataContext = new SelectUserWindowViewModel(TeamRepository.SelectTeamMember(_nowTeam.ID));
            SelectUserWindow window = new SelectUserWindow
            {
                DataContext = dataContext
            };
            window.ShowDialog();
            if (!(bool)window.DialogResult)
            {
                return;
            }

            foreach (UserViewModel selectedUser in dataContext.SelectedUserList)
            {
                if (RichTextBoxDocument.Blocks.LastBlock is Paragraph)
                {
                    (RichTextBoxDocument.Blocks.LastBlock as Paragraph).Inlines.Add(new Run($"@{selectedUser.Name}"));
                }
                else
                {
                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run($"@{selectedUser.Name}"));
                    RichTextBoxDocument.Blocks.Add(paragraph);
                }
            }

            TextboxScrollToEnd = true;
            TextboxScrollToEnd = false;
        }

        private async void SendMessage()
        {
            if (_nowTeam == null)
            {
                return;
            }
            List<TCPMessageBody> teamMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToMessageBodyList(RichTextBoxDocument);
            RichTextBoxDocument = new FlowDocument();
            TeamMessage teamMessage = new TeamMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = teamMessageBodyList,
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.TeamMessage,
                TeamID = _nowTeam.ID,
                LastModifiedTime = DateTime.Parse(TeamRepository.SelectTeam(_nowTeam.ID).LastModifiedTime)
            };

            string message = JsonConvert.SerializeObject(teamMessage);

            foreach (User member in TeamRepository.SelectTeamMember(_nowTeam.ID))
            {
                if (member.IP == GlobalVariables.SelfInfo.IP && member.Name == GlobalVariables.SelfInfo.Name)
                {
                    continue;
                }

                Task<ExecuteResult<Exception>> task = GlobalVariables.TcpServerClient.SendMessage(message, member.IP, GlobalVariables.TcpPort);
                Exception ex = (await task).Result;
                if (ex != null)
                {
                    List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = $"Failed to send: User {member.Name} is offline. The message will be delayed." }];
                    TeamMessage errorMessage = new TeamMessage()
                    {
                        Type = MessageType.Error,
                        MessageBody = errorContent,
                        TeamID = _nowTeam.ID,
                    };
                    TextBlock errorTextBlock = TextBlockHelper.GetMessageControl(errorMessage);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Add(errorTextBlock);
                    });

                    TeamMessageRepository.InsertMessage(errorMessage);
                }
            }

            MessageHelper.ConvertImageInMessage(teamMessage);

            TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(teamMessage, true);
            TextBlock textBlock = TextBlockHelper.GetMessageControl(teamMessage);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageList.Add(timeTextBlock);
                MessageList.Add(textBlock);
            });
            ScrollToEnd = true;
            ScrollToEnd = false;
            TeamMessageRepository.InsertMessage(teamMessage);
        }

        private void TeamClick(TeamViewModel teamViewModel)
        {
            MessageList.Clear();
            if (_nowTeam != null)
            {
                _nowTeam.Selected = false;
            }

            _nowTeam = teamViewModel;
            _nowTeam.Selected = true;
            SendEnabled = true;

            List<TeamMessage> messageList = TeamMessageRepository.SelectMessgae(_nowTeam.ID);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }

            foreach (TeamMessage message in messageList)
            {
                if (message.Type == MessageType.TeamMessage)
                {
                    TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(message, true);
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

        private void RemoveTeam()
        {
            if (_nowTeam != null)
            {
                TeamRepository.RemoveTeam(_nowTeam.ID);
                TeamList.Remove(_nowTeam);
                _nowTeam = null;
            }
        }

        private void ScrollAtTop()
        {
            if (_firstMessageID == -1)
            {
                return;
            }

            List<TeamMessage> messageList = TeamMessageRepository.SelectMessgae(_nowTeam.ID, _firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }
            int index = 0;
            foreach (TeamMessage message in messageList)
            {
                if (message.Type == MessageType.TeamMessage)
                {
                    TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(message, true);
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
