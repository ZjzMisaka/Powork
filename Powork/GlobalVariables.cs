using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PowerThreadPool;
using Powork.CustomEventArgs;
using Powork.Model;
using Powork.Network;
using Powork.Repository;

namespace Powork
{
    public static class GlobalVariables
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
        public delegate void UserListChangedEventHandler(object sender, EventArgs e);
        public static event UserListChangedEventHandler UserListChanged;
        public delegate void GetShareInfoEventHandler(object sender, EventArgs e);
        public static event GetShareInfoEventHandler GetShareInfo;
        public delegate void GetFileEventHandler(object sender, EventArgs e);
        public static event GetFileEventHandler GetFile;
        public delegate void StartGetFileEventHandler(object sender, EventArgs e);
        public static event StartGetFileEventHandler StartGetFile;
        public delegate void GetUserMessageEventHandler(object sender, MessageEventArgs e);
        public static event GetUserMessageEventHandler GetUserMessage;
        public delegate void GetTeamMessageEventHandler(object sender, MessageEventArgs e);
        public static event GetTeamMessageEventHandler GetTeamMessage;

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
        public static List<User> SelfInfo
        {
            get
            {
                return UserRepository.SelectUserByIp(LocalIP.ToString());
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = ni.GetIPProperties();
                    if (ipProperties.GatewayAddresses.Count > 0)
                    {
                        foreach (var ip in ipProperties.UnicastAddresses)
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
