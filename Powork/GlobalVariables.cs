using PowerThreadPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Powork.Model;
using Powork.Repository;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PowerThreadPool.EventArguments;
using Powork.Network;
using System.Net.NetworkInformation;

namespace Powork
{
    public static class GlobalVariables
    {
        private static IPAddress localIP = GetLocalIPAddress();
        public static IPAddress LocalIP { get => localIP; }
        public static int UdpPort { get; } = 1096;
        public static int TcpPort { get; } = 614;
        public static string DbName { get; } = "PoworkDB";
        public static TcpServerClient TcpServerClient { get; set; }
        private static ObservableCollection<User> userList;
        public static ObservableCollection<User> UserList 
        { 
            get => userList;
            set
            {
                userList = value;
                if (UserListChanged != null)
                {
                    UserListChanged.Invoke(userList, new EventArgs());
                }
            }
        }
        public delegate void UserListChangedEventHandler(object sender, EventArgs e);
        public static event UserListChangedEventHandler UserListChanged;
        public delegate void GetShareInfoEventHandler(object sender, EventArgs e);
        public static event GetShareInfoEventHandler GetShareInfo;
        public delegate void GetFileEventHandler(object sender, EventArgs e);
        public static event GetFileEventHandler GetFile;
        public delegate void GetMessageEventHandler(object sender, EventArgs e);
        public static event GetMessageEventHandler GetMessage;

        public static void InvokeGetShareInfoEvent(List<ShareInfo> shareInfos)
        {
            if (GetShareInfo != null)
            {
                GetShareInfo.Invoke(shareInfos, new EventArgs());
            }
        }

        public static void InvokeGetFileEvent(FileInfo fileInfo)
        {
            if (GetFile != null)
            {
                GetFile.Invoke(fileInfo, new EventArgs());
            }
        }
        public static void InvokeGetMessageEvent(TCPMessage userMessage)
        {
            if (GetMessage != null)
            {
                GetMessage.Invoke(userMessage, new EventArgs());
            }
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
