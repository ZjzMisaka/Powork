using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Powork.ViewModel
{
    class TeamPageViewModel : ObservableObject
    {
        private int firstMessageID = -1;
        private TeamViewModel nowTeam = new TeamViewModel() { ID = "1", Name = "1" };
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

        private ObservableCollection<TeamViewModel> teamList;
        public ObservableCollection<TeamViewModel> TeamList
        {
            get
            {
                return teamList;
            }
            set
            {
                SetProperty<ObservableCollection<TeamViewModel>>(ref teamList, value);
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
        public ICommand TeamClickCommand { get; set; }
        public ICommand ScrollAtTopCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public TeamPageViewModel()
        {
            PageEnabled = true;
            SendEnabled = false;
            RichTextBoxDocument = new FlowDocument();

            MessageList = new ObservableCollection<TextBlock>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            SendMessageCommand = new RelayCommand(SendMessage);
            TeamClickCommand = new RelayCommand<TeamViewModel>(TeamClick);
            ScrollAtTopCommand = new RelayCommand(ScrollAtTop);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);

            TeamList = new ObservableCollection<TeamViewModel>();

            GlobalVariables.GetMessage += (s, e) =>
            {
                if (nowTeam == null)
                {
                    return;
                }

                TCPMessage teamMessage = (TCPMessage)s;

                if (teamMessage.Type != MessageType.TeamMessage)
                {
                    return;
                }

                TeamMessageRepository.InsertMessage(teamMessage);

                if (teamMessage.TeamID == nowTeam.ID)
                {
                    TextBlock timeTextBlock = TextBlockHelper.GetTimeControl(teamMessage, true);
                    TextBlock textBlock = TextBlockHelper.GetMessageControl(teamMessage);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Add(timeTextBlock);
                        MessageList.Add(textBlock);
                    });
                }

                // if team not exists, request team info and create team
                foreach (TeamViewModel teamViewModel in TeamList)
                {
                    if (teamMessage.TeamID == teamViewModel.ID)
                    {
                        return;
                    }
                }
                GlobalVariables.TcpServerClient.RequestTeamInfo(teamMessage.TeamID, teamMessage.SenderIP, GlobalVariables.TcpPort);
            };
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            List<Team> teamList = TeamRepository.SelectTeam();
            foreach (Team team in teamList)
            {
                TeamList.Add(new TeamViewModel(team));
            }
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
        }

        private void WindowClosed()
        {
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

        private void SendMessage()
        {
            if (nowTeam == null)
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
                TeamID = nowTeam.ID,
            };

            string message = JsonConvert.SerializeObject(teamMessage);

            foreach (User member in TeamRepository.SelectTeamMember(nowTeam.ID))
            {
                if (member.IP == GlobalVariables.SelfInfo[0].IP && member.Name == GlobalVariables.SelfInfo[0].Name)
                {
                    continue;
                }

                Exception ex = GlobalVariables.TcpServerClient.SendMessage(message, member.IP, GlobalVariables.TcpPort);
                if(ex != null)
                {
                    List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = $"Send failed: User {member.Name} not online" }];
                    TCPMessage errorMessage = new TCPMessage()
                    {
                        Type = MessageType.Error,
                        MessageBody = errorContent,
                        TeamID = nowTeam.ID,
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
            nowTeam.Selected = false;
            nowTeam = teamViewModel;
            nowTeam.Selected = true;
            SendEnabled = true;

            List<TCPMessage> messageList = TeamMessageRepository.SelectMessgae(nowTeam.ID);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
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

        private void ScrollAtTop()
        {
            if (firstMessageID == -1)
            {
                return;
            }

            List<TCPMessage> messageList = TeamMessageRepository.SelectMessgae(nowTeam.ID, firstMessageID);
            if (messageList != null && messageList.Count >= 1)
            {
                firstMessageID = messageList[0].ID;
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
