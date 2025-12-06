using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PowerThreadPool;
using Powork.CustomEventArgs;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using System;
using System.Collections.Generic;

namespace Powork
{
    public static partial class GlobalVariables
    {
        public static PowerPool PowerPool { get; set; }
        private static IPAddress s_localIP = GetLocalIPAddress();
        public static IPAddress LocalIP { get => s_localIP; }
        public static int UdpPort { get; } = 1096;
        public static int TcpPort { get; } = 614;
        public static string DbName { get; } = "PoworkDB";
        public static TcpServerClient TcpServerClient { get; set; }
        private static ObservableCollection<User> s_userList;
        public static ObservableCollection<User> UserList
        {
            get => s_userList;
            set
            {
                s_userList = value;
                if (UserListChanged != null)
                {
                    UserListChanged.Invoke(s_userList, new EventArgs());
                }
            }
        }
        public static event EventHandler UserListChanged;
        public static event EventHandler GetShareInfo;
        public static event EventHandler GetFile;
        public static event EventHandler StartGetFile;
        public static event EventHandler<MessageEventArgs> GetUserMessage;
        public static event EventHandler<MessageEventArgs> GetTeamMessage;
        public static event EventHandler UserOnline;

        public static event EventHandler<List<Project>> ProjectListReceived;
        public static event EventHandler ScheduleReceived;

        public static void InvokeScheduleReceived()
        {
            ScheduleReceived?.Invoke(null, EventArgs.Empty);
        }

        public static void InvokeProjectListReceived(List<Project> projects)
        {
            ProjectListReceived?.Invoke(null, projects);
        }

        public static void InvokeGetShareInfoEvent(List<ShareInfo> shareInfos)
        {
            if (GetShareInfo != null)
            {
                GetShareInfo.Invoke(shareInfos, new EventArgs());
            }
        }
        public static void InvokeStartGetFileEvent(FileInfo fileInfo)
        {
            if (StartGetFile != null)
            {
                StartGetFile.Invoke(fileInfo, new EventArgs());
            }
        }
        public static void InvokeGetFileEvent(FileInfo fileInfo)
        {
            if (GetFile != null)
            {
                GetFile.Invoke(fileInfo, new EventArgs());
            }
        }
        public static bool InvokeGetUserMessageEvent(TCPMessageBase tcpMessage)
        {
            if (GetUserMessage != null)
            {
                MessageEventArgs e = new MessageEventArgs();
                GetUserMessage.Invoke(tcpMessage, e);
                return e.Received;
            }
            return false;
        }
        public static bool InvokeGetTeamMessageEvent(TCPMessageBase tcpMessage)
        {
            if (GetTeamMessage != null)
            {
                MessageEventArgs e = new MessageEventArgs();
                GetTeamMessage.Invoke(tcpMessage, e);
                return e.Received;
            }
            return false;
        }
        public static void InvokeUserOnlineEvent(User user)
        {
            if (GlobalVariables.UserOnline != null)
            {
                GlobalVariables.UserOnline.Invoke(user, new EventArgs());
            }
        }
        public static User SelfInfo
        {
            get
            {
                return UserRepository.SelectLogonUserCurrentIP();
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = ni.GetIPProperties();
                    if (ipProperties.GatewayAddresses.Count > 0)
                    {
                        foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (!ip.Address.ToString().StartsWith("169.254"))
                                {
                                    return ip.Address;
                                }
                            }
                        }
                    }
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
