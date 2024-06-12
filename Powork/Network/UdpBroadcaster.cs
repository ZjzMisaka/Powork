using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using PowerThreadPool.Options;
using Powork.Model;

namespace Powork.Network
{
    public class UdpBroadcaster
    {
        private readonly UdpClient _udpClient;
        private IPEndPoint _endPoint;

        public UdpBroadcaster(int port)
        {
            _udpClient = new UdpClient(port);
            _endPoint = new IPEndPoint(IPAddress.Broadcast, port);
        }

        public void StartBroadcasting()
        {
            GlobalVariables.PowerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    List<User> userList = GlobalVariables.SelfInfo;
                    if (userList.Count == 1)
                    {
                        _udpClient.Client.ReceiveTimeout = 100;
                        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userList[0]));
                        _udpClient.Send(bytes, bytes.Length, _endPoint);
                    }

                    Thread.Sleep(1000);

                    GlobalVariables.PowerPool.StopIfRequested();
                }
            }, new WorkOption<object>()
            {
                LongRunning = true
            });
        }

        public void ListenForBroadcasts(Action<User> onReceive)
        {
            GlobalVariables.PowerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    try
                    {
                        _udpClient.Client.ReceiveTimeout = 100;
                        byte[] receivedBytes = _udpClient.Receive(ref _endPoint);
                        string message = Encoding.UTF8.GetString(receivedBytes);
                        User user = JsonConvert.DeserializeObject<User>(message);
                        onReceive(user);
                    }
                    catch
                    {
                    }
                    GlobalVariables.PowerPool.StopIfRequested();
                }
            }, new WorkOption<object>()
            {
                LongRunning = true
            });
        }
    }
}
