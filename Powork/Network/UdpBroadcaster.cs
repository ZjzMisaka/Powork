using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PowerThreadPool;
using Powork.Model;

namespace Powork.Network
{
    public class UdpBroadcaster
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;
        private PowerPool powerPool;
        private Guid uniqueID;

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
                    var bytes = Encoding.UTF8.GetBytes("Powork");
                    udpClient.Send(bytes, bytes.Length, endPoint);
                    Thread.Sleep(1000);

                    powerPool.StopIfRequested();
                }
            });
        }

        public void ListenForBroadcasts(Action<UdpBroadcastMessage> onReceive)
        {
            powerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    try
                    {
                        udpClient.Client.ReceiveTimeout = 100;
                        var receivedBytes = udpClient.Receive(ref endPoint);
                        var message = Encoding.UTF8.GetString(receivedBytes);
                        onReceive(new UdpBroadcastMessage() { IPEndPoint = endPoint, UniqueID = GlobalVariables.UniqueID });
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
