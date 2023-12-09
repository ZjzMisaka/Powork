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

namespace Powork
{
    static public class GlobalVariables
    {
        static private IPAddress localIP = GetLocalIPAddress();
        static public IPAddress LocalIP { get => localIP; }
        static public int UdpPort { get; } = 1096;
        static public int TcpPort { get; } = 614;
        static public string DbName { get; } = "PoworkDB";
        static public TcpServerClient TcpServerClient { get; set; }
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
        static public event UserListChangedEventHandler UserListChanged;
        static public List<User> SelfInfo 
        { 
            get
            { 
                return UserRepository.SelectUserByIp(LocalIP.ToString());
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
