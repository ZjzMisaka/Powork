using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PowerThreadPool;
using Powork.Model;
using Newtonsoft.Json;

namespace Powork.Network
{
    public class UdpBroadcaster
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;
        private PowerPool powerPool;

        public UdpBroadcaster(int port, PowerPool powerPool)
        {
            udpClient = new UdpClient(port);
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);

            this.powerPool = powerPool;
        }

        public void StartBroadcasting()
        {
            powerPool.QueueWorkItem(() => 
            {
                while (true)
                {
                    List<User> userList = GlobalVariables.SelfInfo;
                    if (userList.Count == 1)
                    {
                        udpClient.Client.ReceiveTimeout = 100;
                        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userList[0]));
                        udpClient.Send(bytes, bytes.Length, endPoint);
                    }
                   
                    Thread.Sleep(1000);

                    powerPool.StopIfRequested();
                }
            });
        }

        public void ListenForBroadcasts(Action<User> onReceive)
        {
            powerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    try
                    {
                        udpClient.Client.ReceiveTimeout = 100;
                        byte[] receivedBytes = udpClient.Receive(ref endPoint);
                        string message = Encoding.UTF8.GetString(receivedBytes);
                        User user = JsonConvert.DeserializeObject<User>(message);
                        onReceive(user);
                    }
                    catch
                    {
                    }
                    powerPool.StopIfRequested();
                }
            });
        }
    }
}
