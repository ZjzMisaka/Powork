using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
using Powork.ViewModel.Inner;
using Wpf.Ui.Appearance;

namespace Powork.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private UdpBroadcaster _udpBroadcaster;
        private static INavigationService s_navigationService;
        private DownloadInfoViewModel _nowDownloadInfoViewModel;
        private DispatcherTimer _blinkTimer;
        private bool _isBlinking;
        private ConcurrentDictionary<string, NoticeViewModel> _noticeInfoDic;
        private ConcurrentDictionary<string, DownloadInfoViewModel> _downloadInfoDic;

        private string _trayIcon;
        public string TrayIcon
        {
            get => _trayIcon;
            set => SetProperty<string>(ref _trayIcon, value);
        }

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty<string>(ref _applicationTitle, value);
        }

        private Visibility _openNoticeButtonVisibility;
        public Visibility OpenNoticeButtonVisibility
        {
            get => _openNoticeButtonVisibility;
            set => SetProperty<Visibility>(ref _openNoticeButtonVisibility, value);
        }

        private bool _topmost;
        public bool Topmost
        {
            get => _topmost;
            set => SetProperty<bool>(ref _topmost, value);
        }

        private bool _noticePopupOpen;
        public bool NoticePopupOpen
        {
            get => _noticePopupOpen;
            set => SetProperty<bool>(ref _noticePopupOpen, value);
        }

        private ObservableCollection<NoticeViewModel> _noticeList;
        public ObservableCollection<NoticeViewModel> NoticeList
        {
            get => _noticeList;
            set => SetProperty<ObservableCollection<NoticeViewModel>>(ref _noticeList, value);
        }

        private bool _downloadPopupOpen;
        public bool DownloadPopupOpen
        {
            get => _downloadPopupOpen;
            set => SetProperty<bool>(ref _downloadPopupOpen, value);
        }

        private bool _popupMenuEnable;
        public bool DownloadPopupMenuEnable
        {
            get => _popupMenuEnable;
            set => SetProperty<bool>(ref _popupMenuEnable, value);
        }

        public bool StopDownloadPopupMenuEnable
        {
            get
            {
                if (_nowDownloadInfoViewModel == null)
                {
                    return false;
                }
                return _nowDownloadInfoViewModel.DoneCount != _nowDownloadInfoViewModel.FileCount;
            }
        }

        private ObservableCollection<DownloadInfoViewModel> _downloadList;
        public ObservableCollection<DownloadInfoViewModel> DownloadList
        {
            get => _downloadList;
            set => SetProperty<ObservableCollection<DownloadInfoViewModel>>(ref _downloadList, value);
        }

        private bool _isDownloadPopupScrollAtBottom;
        public bool IsDownloadPopupScrollAtBottom
        {
            get => _isDownloadPopupScrollAtBottom;
            set => SetProperty<bool>(ref _isDownloadPopupScrollAtBottom, value);
        }

        private bool _downloadPopupScrollToEnd;
        public bool DownloadPopupScrollToEnd
        {
            get => _downloadPopupScrollToEnd;
            set => SetProperty<bool>(ref _downloadPopupScrollToEnd, value);
        }

        public ICommand SwitchLightThemeCommand { get; set; }
        public ICommand SwitchDarkThemeCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }
        public ICommand WindowActivatedCommand { get; set; }
        public ICommand OpenNoticeCommand { get; set; }
        public ICommand NoticeItemClickCommand { get; set; }
        public ICommand OpenDownloadInfoCommand { get; set; }
        public ICommand DownloadItemClickCommand { get; set; }
        public ICommand OpenItemCommand { get; set; }
        public ICommand OpenFolderCommand { get; set; }
        public ICommand RemoveItemCommand { get; set; }
        public ICommand StopDownloadCommand { get; set; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            s_navigationService = navigationService;
            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
            _blinkTimer.Tick += (s, e) => ToggleIcon();
            _noticeInfoDic = new ConcurrentDictionary<string, NoticeViewModel>();
            _downloadInfoDic = new ConcurrentDictionary<string, DownloadInfoViewModel>();

            CommonRepository.CreateDatabase();
            CommonRepository.CreateTable();
            SettingRepository.SetDefault("Theme", "Dark");

            ApplicationThemeManager.Changed += ThemeChanged;

            string theme = SettingRepository.SelectSetting("Theme");
            if (theme == "Dark")
            {
                SwitchDarkTheme();
            }
            else if (theme == "Light")
            {
                SwitchLightTheme();
            }

            NotificationHelper.NavigationService = navigationService;
            NotificationHelper.MainWindowViewModel = this;

            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = Format.DateTimeFormat;
            Thread.CurrentThread.CurrentCulture = ci;

            int maxThreads = Environment.ProcessorCount;
            int minThreads = 5;
            if (minThreads > maxThreads)
            {
                minThreads = maxThreads;
            }
            GlobalVariables.PowerPool = new PowerPool(new PowerThreadPool.Options.PowerPoolOption()
            {
                MaxThreads = maxThreads,
                DestroyThreadOption = new PowerThreadPool.Options.DestroyThreadOption()
                {
                    KeepAliveTime = 3000,
                    MinThreads = minThreads,
                }
            });
            GlobalVariables.PowerPool.ErrorOccurred += (s, e) =>
            {
                MessageBox.Show($"Error Occurred: \n{e.Exception.Message}\n{e.Exception.StackTrace}\nfrom: {e.ErrorFrom}");
            };

            NoticeList = new ObservableCollection<NoticeViewModel>();
            DownloadList = new ObservableCollection<DownloadInfoViewModel>();

            SwitchLightThemeCommand = new RelayCommand(SwitchLightTheme);
            SwitchDarkThemeCommand = new RelayCommand(SwitchDarkTheme);
            ExitCommand = new RelayCommand(Exit);
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
            WindowActivatedCommand = new RelayCommand(WindowActivated);
            OpenNoticeCommand = new RelayCommand(OpenNotice);
            NoticeItemClickCommand = new RelayCommand<NoticeViewModel>(NoticeItemClick);
            OpenDownloadInfoCommand = new RelayCommand(OpenDownloadInfo);
            DownloadItemClickCommand = new RelayCommand<DownloadInfoViewModel>(DownloadItemClick);
            OpenItemCommand = new RelayCommand(OpenItem);
            OpenFolderCommand = new RelayCommand(OpenFolder);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            StopDownloadCommand = new RelayCommand(StopDownload);
        }

        public static void Navigate(Type targetType, ObservableObject dataContext)
        {
            s_navigationService.Navigate(targetType, dataContext);
        }

        private void SwitchLightTheme()
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.Auto, true);
            SettingRepository.UpdateSetting("Theme", "Light");
        }

        private void SwitchDarkTheme()
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.Auto, true);
            SettingRepository.UpdateSetting("Theme", "Dark");
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            TrayIcon = "/Image/icon.ico";
            ApplicationTitle = "Powork";
            OpenNoticeButtonVisibility = Visibility.Collapsed;
            DownloadPopupMenuEnable = false;

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

                List<User> userList = UserRepository.SelectUserByIpName(user.IP, user.Name);
                if (userList.Count == 0)
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
                else
                {
                    if (userList[0].GroupName != user.GroupName)
                    {
                        UserRepository.UpdateUserByIpName(user);
                        foreach (User changeUser in GlobalVariables.UserList)
                        {
                            if (changeUser.IP == user.IP && changeUser.Name == user.Name)
                            {
                                changeUser.GroupName = user.GroupName;
                            }
                        }
                    }
                }

                GlobalVariables.InvokeUserOnlineEvent(user);
            });

            GlobalVariables.TcpServerClient = new TcpServerClient(GlobalVariables.TcpPort);
            GlobalVariables.TcpServerClient.StartListening((client, ip) =>
            {
                NetworkStream stream = client.GetStream();
                using BinaryReader reader = new BinaryReader(stream);
                int length = reader.ReadInt32();
                byte[] messageBytes = reader.ReadBytes(length);
                string message = Encoding.UTF8.GetString(messageBytes);
                TCPMessageBase tcpMessage = JsonConvert.DeserializeObject<TCPMessageBase>(message);

                if (tcpMessage.Type == MessageType.UserMessage || tcpMessage.Type == MessageType.TeamMessage)
                {
                    if (tcpMessage.Type == MessageType.TeamMessage)
                    {
                        TeamMessage teamMessage = JsonConvert.DeserializeObject<TeamMessage>(message);
                        MessageHelper.ConvertImageInMessage(teamMessage);
                        Team team = TeamRepository.SelectTeam(teamMessage.TeamID);
                        if (team != null)
                        {
                            if (DateTime.Parse(team.LastModifiedTime) < teamMessage.LastModifiedTime)
                            {
                                GlobalVariables.TcpServerClient.RequestTeamInfo(teamMessage.TeamID, tcpMessage.SenderIP, GlobalVariables.TcpPort);
                            }

                            if (!TeamRepository.ContainMember(teamMessage.TeamID, tcpMessage.SenderIP, tcpMessage.SenderName))
                            {
                                TeamMessageRepository.InsertMessage(teamMessage);
                                return;
                            }
                        }
                        else
                        {
                            GlobalVariables.TcpServerClient.RequestTeamInfo(teamMessage.TeamID, tcpMessage.SenderIP, GlobalVariables.TcpPort);
                        }

                        TeamMessageRepository.InsertMessage(teamMessage);

                        FlashHelper.FlashTaskbarIcon();
                        NotificationHelper.ShowNotification(teamMessage, team);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Window mainWindow = Application.Current.MainWindow;
                            if (!WindowHelper.IsWindowActive(mainWindow))
                            {
                                StartBlinking();
                            }
                        }, DispatcherPriority.Normal);
                        if (!GlobalVariables.InvokeGetTeamMessageEvent(teamMessage))
                        {
                            lock (_noticeInfoDic)
                            {
                                NoticeViewModel noticeViewModel = null;
                                Team msgTeam = TeamRepository.SelectTeam(teamMessage.TeamID);
                                string teamName = "Unknown";
                                if (msgTeam != null)
                                {
                                    teamName = msgTeam.Name;
                                }
                                _noticeInfoDic.TryGetValue(teamMessage.TeamID, out noticeViewModel);
                                if (noticeViewModel == null)
                                {
                                    noticeViewModel = new NoticeViewModel();
                                    noticeViewModel.Count = 1;
                                    noticeViewModel.Notice = $"{noticeViewModel.Count} Message{(noticeViewModel.Count == 1 ? "" : "s")} from Team {teamName}";
                                    noticeViewModel.TeamMessage = teamMessage;
                                    _noticeInfoDic[teamMessage.TeamID] = noticeViewModel;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        NoticeList.Add(noticeViewModel);
                                        if (NoticeList.Count > 0)
                                        {
                                            OpenNoticeButtonVisibility = Visibility.Visible;
                                        }
                                    });
                                }
                                else
                                {
                                    ++noticeViewModel.Count;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        noticeViewModel.Notice = $"{noticeViewModel.Count} Message{(noticeViewModel.Count == 1 ? "" : "s")} from Team {teamName}";
                                    });
                                }
                            }
                        }
                    }
                    else if (tcpMessage.Type == MessageType.UserMessage)
                    {
                        UserMessage userMessage = JsonConvert.DeserializeObject<UserMessage>(message);
                        MessageHelper.ConvertImageInMessage(userMessage);
                        UserMessageRepository.InsertMessage(userMessage, GlobalVariables.SelfInfo.IP, GlobalVariables.SelfInfo.Name);
                        FlashHelper.FlashTaskbarIcon();
                        NotificationHelper.ShowNotification(userMessage);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Window mainWindow = Application.Current.MainWindow;
                            if (!WindowHelper.IsWindowActive(mainWindow))
                            {
                                StartBlinking();
                            }
                        }, DispatcherPriority.Normal);
                        if (!GlobalVariables.InvokeGetUserMessageEvent(userMessage))
                        {
                            lock (_noticeInfoDic)
                            {
                                NoticeViewModel noticeViewModel = null;
                                _noticeInfoDic.TryGetValue(userMessage.SenderIP + userMessage.SenderName, out noticeViewModel);
                                if (noticeViewModel == null)
                                {
                                    noticeViewModel = new NoticeViewModel();
                                    noticeViewModel.Count = 1;
                                    noticeViewModel.Notice = $"{noticeViewModel.Count} Message{(noticeViewModel.Count == 1 ? "" : "s")} from {userMessage.SenderName}";
                                    noticeViewModel.UserMessage = userMessage;
                                    _noticeInfoDic[userMessage.SenderIP + userMessage.SenderName] = noticeViewModel;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        NoticeList.Add(noticeViewModel);
                                        OpenNoticeButtonVisibility = Visibility.Visible;
                                    });
                                }
                                else
                                {
                                    ++noticeViewModel.Count;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        noticeViewModel.Notice = $"{noticeViewModel.Count} Message{(noticeViewModel.Count == 1 ? "" : "s")} from {userMessage.SenderName}";
                                    });
                                }
                            }
                        }
                    }
                }
                else if (tcpMessage.Type == MessageType.FileRequest)
                {
                    string guid = tcpMessage.MessageBody[0].Content;
                    string path = FileRepository.SelectFile(guid);
                    List<string> sendFileWorkIDList = new List<string>();
                    if (FileHelper.GetType(path) == FileHelper.Type.None)
                    {
                        MessageBox.Show("No such file: " + path);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.File)
                    {
                        GlobalVariables.TcpServerClient.SendFile(tcpMessage.RequestID, path, guid, tcpMessage.SenderIP, GlobalVariables.TcpPort);
                    }
                    else if (FileHelper.GetType(path) == FileHelper.Type.Directory)
                    {
                        string[] allfiles = Directory.GetFiles(path, Format.AllFilePattern, SearchOption.AllDirectories);
                        long totalSize = 0;
                        foreach (string file in allfiles)
                        {
                            totalSize += new System.IO.FileInfo(file).Length;
                        }
                        foreach (string file in allfiles)
                        {
                            string relativePath = Path.Combine(new DirectoryInfo(path).Name, FileHelper.GetRelativePath(file, path));
                            GlobalVariables.TcpServerClient.SendFile(tcpMessage.RequestID, file, guid, tcpMessage.SenderIP, GlobalVariables.TcpPort, relativePath, allfiles.Length, totalSize, true, Path.GetFileName(path));
                        }
                    }
                }
                else if (tcpMessage.Type == MessageType.FileInfo)
                {
                    string json = tcpMessage.MessageBody[0].Content;
                    Model.FileInfo fileInfo = JsonConvert.DeserializeObject<Model.FileInfo>(json);

                    FileMessage fileMessage = JsonConvert.DeserializeObject<FileMessage>(message);

                    int fileCount = fileMessage.FileCount;
                    long totalSize = fileMessage.TotalSize;

                    if (fileInfo.Status == Status.SendFileStart)
                    {
                        GlobalVariables.InvokeStartGetFileEvent(fileInfo);

                        string path = Path.Combine(GlobalVariables.TcpServerClient.savePathDict[fileInfo.Guid], fileInfo.RelativePath);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        try
                        {
                            DownloadInfoViewModel downloadInfoViewModel = null;

                            string receivedFilePath = Path.Combine(path, fileInfo.Name);
                            long fileSize = totalSize > 0 ? totalSize : fileInfo.Size;
                            lock (_downloadInfoDic)
                            {
                                if (fileCount > 1)
                                {
                                    _downloadInfoDic.TryGetValue(fileMessage.RequestID, out downloadInfoViewModel);
                                }

                                if (downloadInfoViewModel == null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        downloadInfoViewModel = new DownloadInfoViewModel()
                                        {
                                            RequestID = fileMessage.RequestID,
                                            ID = fileInfo.Guid,
                                            Path = fileMessage.IsFolder ? Path.Combine(GlobalVariables.TcpServerClient.savePathDict[fileInfo.Guid], fileMessage.FolderName) : receivedFilePath,
                                            Name = fileMessage.IsFolder ? fileMessage.FolderName : fileInfo.Name,
                                            Progress = 0,
                                            Failed = false,
                                            FileCount = fileCount,
                                            TotalSize = fileSize,
                                        };
                                        _downloadInfoDic[fileMessage.RequestID] = downloadInfoViewModel;
                                        DownloadList.Add(downloadInfoViewModel);
                                    });
                                }
                            }

                            DownloadPopupOpen = true;
                            DownloadPopupScrollToEnd = true;
                            DownloadPopupScrollToEnd = false;

                            using (FileStream fileStream = new FileStream(receivedFilePath, FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;

                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (downloadInfoViewModel.Stop)
                                    {
                                        downloadInfoViewModel.Failed = true;
                                        break;
                                    }

                                    fileStream.Write(buffer, 0, bytesRead);

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        downloadInfoViewModel.Received(bytesRead);
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (fileInfo.Status == Status.SendFileFinish)
                    {
                        // GlobalVariables.TcpServerClient.savePathDict.TryRemove(fileInfo.Guid, out _);
                        GlobalVariables.InvokeGetFileEvent(fileInfo);

                        DownloadInfoViewModel downloadInfoViewModel = null;
                        SpinWait.SpinUntil(() => _downloadInfoDic.TryGetValue(tcpMessage.RequestID, out downloadInfoViewModel));
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            downloadInfoViewModel.Done();
                        });
                    }
                    else if (fileInfo.Status == Status.NoSuchFile)
                    {
                        MessageBox.Show("No such file: " + fileInfo.Name);

                        for (int i = 0; i < DownloadList.Count; ++i)
                        {
                            DownloadInfoViewModel downloadInfoViewModel = DownloadList[i];
                            if (downloadInfoViewModel.ID == fileInfo.Guid)
                            {
                                downloadInfoViewModel.Failed = true;
                            }
                        }
                    }
                }
                else if (tcpMessage.Type == MessageType.TeamInfoRequest)
                {
                    string teamID = tcpMessage.MessageBody[0].Content;
                    GlobalVariables.TcpServerClient.SendTeamInfo(teamID, TeamRepository.SelectTeam(teamID).Name, TeamRepository.SelectTeam(teamID).LastModifiedTime, TeamRepository.SelectTeamMember(teamID), tcpMessage.SenderIP);
                }
                else if (tcpMessage.Type == MessageType.TeamInfo)
                {
                    string teamID = null;
                    string teamName = null;
                    string lastModifiedTime = null;
                    List<User> members = null;
                    foreach (TCPMessageBody messageBody in tcpMessage.MessageBody)
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
                else if (tcpMessage.Type == MessageType.ShareInfoRequest)
                {
                    string teamID = tcpMessage.MessageBody[0].Content;
                    ShareRepository.ReCheckFiles();
                    GlobalVariables.TcpServerClient.SendShareInfo(ShareRepository.SelectFile(), tcpMessage.SenderIP);
                }
                else if (tcpMessage.Type == MessageType.ShareInfo)
                {
                    List<ShareInfo> shareInfos = null;
                    foreach (TCPMessageBody messageBody in tcpMessage.MessageBody)
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
            CommonRepository.CloseConnection();
            GlobalVariables.PowerPool.Dispose();

            Environment.Exit(0);
        }

        private void WindowActivated()
        {
            StopBlinking();
        }

        private void OpenNotice()
        {
            NoticePopupOpen = !NoticePopupOpen;
        }

        private void NoticeItemClick(NoticeViewModel noticeViewModel)
        {
            if (noticeViewModel.UserMessage != null)
            {
                lock (_noticeInfoDic)
                {
                    _noticeInfoDic.TryRemove(noticeViewModel.UserMessage.SenderIP + noticeViewModel.UserMessage.SenderName, out _);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NoticeList.Remove(noticeViewModel);
                        if (NoticeList.Count == 0)
                        {
                            OpenNoticeButtonVisibility = Visibility.Collapsed;
                        }
                    });
                }
                s_navigationService.Navigate(typeof(TeamPage), new TeamPageViewModel());

                s_navigationService.Navigate(typeof(MessagePage), new MessagePageViewModel(ServiceLocator.GetService<INavigationService>(), noticeViewModel.UserMessage.SenderIP, noticeViewModel.UserMessage.SenderName));
            }
            else if (noticeViewModel.TeamMessage != null)
            {
                lock (_noticeInfoDic)
                {
                    _noticeInfoDic.TryRemove(noticeViewModel.TeamMessage.TeamID, out _);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NoticeList.Remove(noticeViewModel);
                        if (NoticeList.Count == 0)
                        {
                            OpenNoticeButtonVisibility = Visibility.Collapsed;
                        }
                    });
                }
                s_navigationService.Navigate(typeof(MessagePage), new MessagePageViewModel(ServiceLocator.GetService<INavigationService>()));

                s_navigationService.Navigate(typeof(TeamPage), new TeamPageViewModel(noticeViewModel.TeamMessage.TeamID));
            }

            NoticePopupOpen = false;
        }

        private void OpenDownloadInfo()
        {
            DownloadPopupOpen = !DownloadPopupOpen;
        }

        private void DownloadItemClick(DownloadInfoViewModel downloadInfoViewModel)
        {
            if (_nowDownloadInfoViewModel != null)
            {
                _nowDownloadInfoViewModel.Selected = false;
            }
            _nowDownloadInfoViewModel = downloadInfoViewModel;
            DownloadPopupMenuEnable = true;
            _nowDownloadInfoViewModel.Selected = true;
        }

        private void OpenItem()
        {
            Process p = new Process();
            if (!Path.Exists(_nowDownloadInfoViewModel.Path))
            {
                MessageBox.Show($"{Path.GetFileName(_nowDownloadInfoViewModel.Path)} not found.");
                return;
            }
            p.StartInfo = new ProcessStartInfo(_nowDownloadInfoViewModel.Path)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private void OpenFolder()
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.GetDirectoryName(_nowDownloadInfoViewModel.Path))
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private void RemoveItem()
        {
            if (File.Exists(_nowDownloadInfoViewModel.Path))
            {
                File.Delete(_nowDownloadInfoViewModel.Path);
            }
            else if (Directory.Exists(_nowDownloadInfoViewModel.Path))
            {
                Directory.Delete(_nowDownloadInfoViewModel.Path, true);
            }
        }

        private void StopDownload()
        {
            _nowDownloadInfoViewModel.Stop = true;
        }

        private void StartBlinking()
        {
            if (!_isBlinking)
            {
                _isBlinking = true;
                _blinkTimer.Start();
            }
        }

        private void StopBlinking()
        {
            _isBlinking = false;
            _blinkTimer.Stop();
            TrayIcon = "/Image/icon.ico";
        }

        private void ToggleIcon()
        {
            if (TrayIcon == "/Image/icon.ico")
            {
                TrayIcon = "/Image/icon_flash.ico";
            }
            else
            {
                TrayIcon = "/Image/icon.ico";
            }
        }

        private void ThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            if (currentApplicationTheme == ApplicationTheme.Dark)
            {
                Application.Current.Resources["SelectedBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#646464"));
                Application.Current.Resources["SelectedForegroundBrush"] = Brushes.White;

                Application.Current.Resources["TimeTextBrush"] = Brushes.LightGreen;
                Application.Current.Resources["ErrorTextBrush"] = Brushes.Pink;
            }
            else
            {
                Application.Current.Resources["SelectedBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
                Application.Current.Resources["SelectedForegroundBrush"] = Brushes.Black;

                Application.Current.Resources["TimeTextBrush"] = Brushes.DarkGreen;
                Application.Current.Resources["ErrorTextBrush"] = Brushes.DarkRed;
            }
        }
    }
}
