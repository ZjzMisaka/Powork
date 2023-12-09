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

namespace Powork
{
    static public class GlobalVariables
    {
        static private IPAddress localIP = GetLocalIPAddress();
        static internal IPAddress LocalIP { get => localIP; }
        static internal int UdpPort { get; set; } = 1096;
        static internal int TcpPort { get; set; } = 614;
        static internal string DbName { get; } = "PoworkDB";
        private static ObservableCollection<User> userList;
        internal static ObservableCollection<User> UserList 
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
        static internal List<User> SelfInfo 
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
