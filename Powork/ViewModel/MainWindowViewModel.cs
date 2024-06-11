using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PowerThreadPool;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using Powork.Service;
using Powork.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Powork.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        private PowerPool powerPool = null;

        private UdpBroadcaster udpBroadcaster;
        private static INavigationService _navigationService;

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            Thread.CurrentThread.CurrentCulture = ci;

            powerPool = new PowerPool();

            CommonRepository.CreateDatabase();
            CommonRepository.CreateTable();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
        }

        public static void Navigate(Type targetType, ObservableObject dataContext)
        {
            _navigationService.Navigate(targetType, dataContext);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            udpBroadcaster = new UdpBroadcaster(GlobalVariables.UdpPort, powerPool);
            GlobalVariables.UserList = new ObservableCollection<User>(UserRepository.SelectUser());

            udpBroadcaster.StartBroadcasting();
            udpBroadcaster.ListenForBroadcasts((user) =>
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

            GlobalVariables.TcpServerClient = new TcpServerClient(GlobalVariables.TcpPort, powerPool);
            GlobalVariables.TcpServerClient.StartListening((stream, ip) =>
            {
                if (ip == GlobalVariables.SelfInfo[0].IP)
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
                        TeamMessageRepository.InsertMessage(userMessage);
                    }
                    else if (userMessage.Type == MessageType.UserMessage)
                    {
                        UserMessageRepository.InsertMessage(userMessage, GlobalVariables.SelfInfo[0].IP, GlobalVariables.SelfInfo[0].Name);
                    }
                    GlobalVariables.InvokeGetMessageEvent(userMessage);
                }
                else if (userMessage.Type == MessageType.FileRequest)
                {
                    string guid = userMessage.MessageBody[0].Content;
                    string path = FileRepository.SelectFile(guid);
                    if (FileHelper.GetType(path) == FileHelper.Type.None)
                    {
                        MessageBox.Show("No such file: " + path);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.File)
                    {
                        GlobalVariables.TcpServerClient.SendFile(path, guid, userMessage.SenderIP, GlobalVariables.TcpPort);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.Directory)
                    {
                        string[] allfiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                        foreach (string file in allfiles)
                        {
                            string relativePath = Path.Combine(new DirectoryInfo(path).Name, FileHelper.GetRelativePath(file, path));
                            GlobalVariables.TcpServerClient.SendFile(file, guid, userMessage.SenderIP, GlobalVariables.TcpPort, relativePath);
                        }
                    }
                    GlobalVariables.TcpServerClient.SendFileFinish(path, guid, userMessage.SenderIP, GlobalVariables.TcpPort);
                }
                else if (userMessage.Type == MessageType.FileInfo)
                {
                    string json = userMessage.MessageBody[0].Content;
                    Model.FileInfo fileInfo = JsonConvert.DeserializeObject<Model.FileInfo>(json);

                    if (fileInfo.Status == Model.Status.Start)
                    {
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
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (fileInfo.Status == Model.Status.SendFileFinish)
                    {
                        // Check
                        GlobalVariables.TcpServerClient.savePathDict.Remove(fileInfo.Guid);
                        GlobalVariables.InvokeGetFileEvent(fileInfo);
                    }
                    else if (fileInfo.Status == Model.Status.NoSuchFile)
                    {
                        MessageBox.Show("No such file: " + fileInfo.Name);
                    }
                }
                else if (userMessage.Type == MessageType.TeamInfoRequest)
                {
                    string teamID = userMessage.MessageBody[0].Content;
                    GlobalVariables.TcpServerClient.SendTeamInfo(teamID, TeamRepository.SelectTeam(teamID).Name, TeamRepository.SelectTeamMember(teamID), userMessage.SenderIP);
                }
                else if (userMessage.Type == MessageType.TeamInfo)
                {
                    string teamID = null;
                    string teamName = null;
                    List<User> members = null;
                    foreach (TCPMessageBody messageBody in userMessage.MessageBody)
                    {
                        string json = messageBody.Content;
                        KeyValuePair<string, object> teamInfoPart = JsonConvert.DeserializeObject<KeyValuePair<string, object>>(json);
                        if (teamInfoPart.Key == "team id")
                        {
                            teamID = (string)teamInfoPart.Value;
                        }
                        else if (teamInfoPart.Key == "team name")
                        {
                            teamName = (string)teamInfoPart.Value;
                        }
                        else if (teamInfoPart.Key == "members")
                        {
                            members = JsonConvert.DeserializeObject<List<User>>(((Newtonsoft.Json.Linq.JArray)teamInfoPart.Value).ToString());
                        }
                    }
                    Team team = new Team();
                    team.ID = teamID;
                    team.Name = teamName;
                    team.MemberList = members;
                    TeamRepository.InsertTeam(team);
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
                        if (teamInfoPart.Key == "share infos")
                        {
                            shareInfos = JsonConvert.DeserializeObject<List<ShareInfo>>(((Newtonsoft.Json.Linq.JArray)teamInfoPart.Value).ToString());
                        }
                    }

                    GlobalVariables.InvokeGetShareInfoEvent(shareInfos);
                }
            });

            if (!UserHelper.IsUserLogon())
            {
                Navigate(typeof(SettingsPage), new SettingsPageViewModel());
            }
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
            powerPool.Dispose();
        }

        private void WindowClosed()
        {
        }
    }
}
