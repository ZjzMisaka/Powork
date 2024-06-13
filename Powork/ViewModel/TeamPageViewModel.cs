using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PowerThreadPool.Results;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using Powork.ViewModel.Inner;
using Wpf.Ui.Controls;
using static System.Windows.Forms.LinkLabel;

namespace Powork.ViewModel
{
    class TeamPageViewModel : ObservableObject
    {
        private int _firstMessageID = -1;
        private TeamViewModel _nowTeam = null;
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

        public TeamPageViewModel()
        {
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

            GlobalVariables.GetMessage += OnGetMessage;
            GlobalVariables.GetFile += OnGetFile;
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }

            List<Team> teamList = TeamRepository.SelectTeam();
            foreach (Team team in teamList)
            {
                TeamList.Add(new TeamViewModel(team));
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetMessage -= OnGetMessage;
            GlobalVariables.GetFile -= OnGetFile;
        }

        private void OnGetMessage(object sender, EventArgs e)
        {
            TCPMessage teamMessage = (TCPMessage)sender;

            if (teamMessage.Type != MessageType.TeamMessage)
            {
                return;
            }

            if (_nowTeam != null && teamMessage.TeamID == _nowTeam.ID)
            {
                TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(teamMessage, true);
                TextBlock textBlock = TextBlockHelper.GetMessageControl(teamMessage);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageList.Add(timeTextBlock);
                    MessageList.Add(textBlock);
                });
            }
        }

        private void OnGetFile(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show($"{(sender as Model.FileInfo).Name} received successfully.");
        }

        public void InsertImage(string uri)
        {
            var image = new Image();
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

        private void ManageTeamMember()
        {
            ManageTeamMemberWindowViewModel dataContext = new ManageTeamMemberWindowViewModel(UserRepository.SelectUser(), TeamRepository.SelectTeamMember(_nowTeam.ID));
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
            nowTeam.LastModifiedTime = DateTime.Now;
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
        }

        private async void SendMessage()
        {
            if (_nowTeam == null)
            {
                return;
            }
            List<TCPMessageBody> teamMessageBodyList = RichTextBoxHelper.ConvertFlowDocumentToMessageBodyList(RichTextBoxDocument);
            TCPMessage teamMessage = new TCPMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = teamMessageBodyList,
                SenderName = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.TeamMessage,
                TeamID = _nowTeam.ID,
            };

            string message = JsonConvert.SerializeObject(teamMessage);

            foreach (User member in TeamRepository.SelectTeamMember(_nowTeam.ID))
            {
                if (member.IP == GlobalVariables.SelfInfo[0].IP && member.Name == GlobalVariables.SelfInfo[0].Name)
                {
                    continue;
                }

                Task<ExecuteResult<Exception>> task = GlobalVariables.TcpServerClient.SendMessage(message, member.IP, GlobalVariables.TcpPort);
                Exception ex = (await task).Result;
                if (ex != null)
                {
                    List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = $"Send failed: User {member.Name} not online" }];
                    TCPMessage errorMessage = new TCPMessage()
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
            TeamMessageRepository.InsertMessage(teamMessage);

            RichTextBoxDocument = new FlowDocument();
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

            List<TCPMessage> messageList = TeamMessageRepository.SelectMessgae(_nowTeam.ID);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }

            foreach (TCPMessage message in messageList)
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

            List<TCPMessage> messageList = TeamMessageRepository.SelectMessgae(_nowTeam.ID, _firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                _firstMessageID = messageList[0].ID;
            }
            int index = 0;
            foreach (TCPMessage message in messageList)
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
