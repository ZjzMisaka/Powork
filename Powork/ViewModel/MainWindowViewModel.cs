using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using PowerThreadPool;
using Powork.Constant;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using Powork.Service;
using Powork.View;

namespace Powork.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        private UdpBroadcaster _udpBroadcaster;
        private static INavigationService s_navigationService;

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get
            {
                return _applicationTitle;
            }
            set
            {
                SetProperty<string>(ref _applicationTitle, value);
            }
        }

        public ICommand ExitCommand { get; set; }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            s_navigationService = navigationService;
            NotificationHelper.NavigationService = navigationService;

            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = Format.DateTimeFormat;
            Thread.CurrentThread.CurrentCulture = ci;

            GlobalVariables.PowerPool = new PowerPool();

            CommonRepository.CreateDatabase();
            CommonRepository.CreateTable();

            ExitCommand = new RelayCommand(Exit);
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
        }

        public static void Navigate(Type targetType, ObservableObject dataContext)
        {
            s_navigationService.Navigate(targetType, dataContext);
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            ApplicationTitle = "Powork";

            _udpBroadcaster = new UdpBroadcaster(GlobalVariables.UdpPort);
            GlobalVariables.UserList = new ObservableCollection<User>(UserRepository.SelectUser());

            ToastNotificationManagerCompat.OnActivated += NotificationHelper.ToastNotificationManagerCompatActivated;

            _udpBroadcaster.StartBroadcasting();
            _udpBroadcaster.ListenForBroadcasts((user) =>
            {
                if (user.IP == GlobalVariables.LocalIP.ToString())
                {
                    return;
                }

                if (UserRepository.SelectUserByIpName(user.IP, user.Name).Count == 0)
                {
                    User insertUser = new User()
                    {
                        Name = user.Name,
                        IP = user.IP,
                        GroupName = user.GroupName
                    };
                    UserRepository.InsertUser(insertUser);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GlobalVariables.UserList.Add(insertUser);
                    });
                }
            });

            GlobalVariables.TcpServerClient = new TcpServerClient(GlobalVariables.TcpPort);
            GlobalVariables.TcpServerClient.StartListening((stream, ip) =>
            {
                if (GlobalVariables.SelfInfo.Count == 0 || ip == GlobalVariables.SelfInfo[0].IP)
                {
                    return;
                }

                using var reader = new BinaryReader(stream);
                // First read the length of the incoming message
                int length = reader.ReadInt32();
                // Then read the message itself
                byte[] messageBytes = reader.ReadBytes(length);
                string message = Encoding.UTF8.GetString(messageBytes);
                TCPMessage userMessage = JsonConvert.DeserializeObject<TCPMessage>(message);
                if (userMessage.Type == MessageType.UserMessage || userMessage.Type == MessageType.TeamMessage)
                {
                    MessageHelper.ConvertImageInMessage(userMessage);
                    if (userMessage.Type == MessageType.TeamMessage)
                    {
                        Team team = TeamRepository.SelectTeam(userMessage.TeamID);
                        if (team != null)
                        {
                            if (DateTime.Parse(team.LastModifiedTime) < userMessage.LastModifiedTime)
                            {
                                GlobalVariables.TcpServerClient.RequestTeamInfo(userMessage.TeamID, userMessage.SenderIP, GlobalVariables.TcpPort);
                            }

                            bool containThisUser = false;
                            foreach (User user in TeamRepository.SelectTeamMember(userMessage.TeamID))
                            {
                                if (user.IP == userMessage.SenderIP && user.Name == userMessage.SenderName)
                                {
                                    containThisUser = true;
                                    break;
                                }
                            }
                            if (!containThisUser)
                            {
                                return;
                            }
                        }
                        else
                        {
                            GlobalVariables.TcpServerClient.RequestTeamInfo(userMessage.TeamID, userMessage.SenderIP, GlobalVariables.TcpPort);
                        }

                        TeamMessageRepository.InsertMessage(userMessage);

                        FlashHelper.FlashTaskbarIcon();
                        NotificationHelper.ShowNotification(userMessage, team);
                    }
                    else if (userMessage.Type == MessageType.UserMessage)
                    {
                        UserMessageRepository.InsertMessage(userMessage, GlobalVariables.SelfInfo[0].IP, GlobalVariables.SelfInfo[0].Name);
                        FlashHelper.FlashTaskbarIcon();
                        NotificationHelper.ShowNotification(userMessage);
                    }
                    GlobalVariables.InvokeGetMessageEvent(userMessage);
                }
                else if (userMessage.Type == MessageType.FileRequest)
                {
                    string guid = userMessage.MessageBody[0].Content;
                    string path = FileRepository.SelectFile(guid);
                    List<string> sendFileWorkIDList = new List<string>();
                    if (FileHelper.GetType(path) == FileHelper.Type.None)
                    {
                        System.Windows.MessageBox.Show("No such file: " + path);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.File)
                    {
                        string id = GlobalVariables.TcpServerClient.SendFile(path, guid, userMessage.SenderIP, GlobalVariables.TcpPort);
                        sendFileWorkIDList.Add(id);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.Directory)
                    {
                        string[] allfiles = Directory.GetFiles(path, Format.AllFilePattern, SearchOption.AllDirectories);
                        foreach (string file in allfiles)
                        {
                            string relativePath = Path.Combine(new DirectoryInfo(path).Name, FileHelper.GetRelativePath(file, path));
                            string id = GlobalVariables.TcpServerClient.SendFile(file, guid, userMessage.SenderIP, GlobalVariables.TcpPort, relativePath);
                            sendFileWorkIDList.Add(id);
                        }
                    }
                    GlobalVariables.TcpServerClient.SendFileFinish(path, guid, userMessage.SenderIP, GlobalVariables.TcpPort, sendFileWorkIDList);
                }
                else if (userMessage.Type == MessageType.FileInfo)
                {
                    string json = userMessage.MessageBody[0].Content;
                    Model.FileInfo fileInfo = JsonConvert.DeserializeObject<Model.FileInfo>(json);

                    if (fileInfo.Status == Model.Status.SendFileStart)
                    {
                        GlobalVariables.InvokeStartGetFileEvent(fileInfo);
                        string path = Path.Combine(GlobalVariables.TcpServerClient.savePathDict[fileInfo.Guid], fileInfo.RelativePath);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        try
                        {
                            string receivedFilePath = Path.Combine(path, fileInfo.Name);
                            using (var fileStream = new FileStream(receivedFilePath, FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;

                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }
                    else if (fileInfo.Status == Model.Status.SendFileFinish)
                    {
                        GlobalVariables.TcpServerClient.savePathDict.Remove(fileInfo.Guid);
                        GlobalVariables.InvokeGetFileEvent(fileInfo);
                    }
                    else if (fileInfo.Status == Model.Status.NoSuchFile)
                    {
                        System.Windows.MessageBox.Show("No such file: " + fileInfo.Name);
                    }
                }
                else if (userMessage.Type == MessageType.TeamInfoRequest)
                {
                    string teamID = userMessage.MessageBody[0].Content;
                    GlobalVariables.TcpServerClient.SendTeamInfo(teamID, TeamRepository.SelectTeam(teamID).Name, TeamRepository.SelectTeam(teamID).LastModifiedTime, TeamRepository.SelectTeamMember(teamID), userMessage.SenderIP);
                }
                else if (userMessage.Type == MessageType.TeamInfo)
                {
                    string teamID = null;
                    string teamName = null;
                    string lastModifiedTime = null;
                    List<User> members = null;
                    foreach (TCPMessageBody messageBody in userMessage.MessageBody)
                    {
                        string json = messageBody.Content;
                        KeyValuePair<string, object> teamInfoPart = JsonConvert.DeserializeObject<KeyValuePair<string, object>>(json);
                        if (teamInfoPart.Key == MessageBodyContentKey.TeamID)
                        {
                            teamID = (string)teamInfoPart.Value;
                        }
                        else if (teamInfoPart.Key == MessageBodyContentKey.TeamName)
                        {
                            teamName = (string)teamInfoPart.Value;
                        }
                        else if (teamInfoPart.Key == MessageBodyContentKey.LastModifiedTime)
                        {
                            lastModifiedTime = (string)teamInfoPart.Value;
                        }
                        else if (teamInfoPart.Key == MessageBodyContentKey.Members)
                        {
                            members = JsonConvert.DeserializeObject<List<User>>(((Newtonsoft.Json.Linq.JArray)teamInfoPart.Value).ToString());
                        }
                    }
                    Team team = new Team();
                    team.ID = teamID;
                    team.Name = teamName;
                    team.LastModifiedTime = lastModifiedTime;
                    team.MemberList = members;
                    TeamRepository.RemoveTeam(team.ID);
                    TeamRepository.InsertOrUpdateTeam(team);
                }
                else if (userMessage.Type == MessageType.ShareInfoRequest)
                {
                    string teamID = userMessage.MessageBody[0].Content;
                    ShareRepository.ReCheckFiles();
                    GlobalVariables.TcpServerClient.SendShareInfo(ShareRepository.SelectFile(), userMessage.SenderIP);
                }
                else if (userMessage.Type == MessageType.ShareInfo)
                {
                    List<ShareInfo> shareInfos = null;
                    foreach (TCPMessageBody messageBody in userMessage.MessageBody)
                    {
                        string json = messageBody.Content;
                        KeyValuePair<string, object> teamInfoPart = JsonConvert.DeserializeObject<KeyValuePair<string, object>>(json);
                        if (teamInfoPart.Key == MessageBodyContentKey.ShareInfos)
                        {
                            shareInfos = JsonConvert.DeserializeObject<List<ShareInfo>>(((Newtonsoft.Json.Linq.JArray)teamInfoPart.Value).ToString());
                        }
                    }

                    GlobalVariables.InvokeGetShareInfoEvent(shareInfos);
                }
            });

            if (!UserHelper.IsUserLogon())
            {
                Navigate(typeof(SettingsPage), new SettingsPageViewModel(ServiceLocator.GetService<INavigationService>()));
            }
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
            eventArgs.Cancel = true;
            Application.Current.MainWindow.Hide();
        }

        private void WindowClosed()
        {
            GlobalVariables.PowerPool.Dispose();
        }
    }
}
