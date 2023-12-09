using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using PowerThreadPool;
using System.Windows;

namespace Powork.Network
{

    public class TcpServerClient
    {
        private TcpListener tcpListener;
        private PowerPool powerPool;

        public TcpServerClient(int port, PowerPool powerPool)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            this.powerPool = powerPool;
        }

        public void StartListening(Action<NetworkStream, string> onReceive)
        {
            tcpListener.Start();
            powerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    while (!tcpListener.Pending())
                    {
                        Thread.Sleep(100);
                        powerPool.StopIfRequested();
                    }
                    TcpClient client = tcpListener.AcceptTcpClient();
                    onReceive(client.GetStream(), ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    client.Dispose();

                    if (powerPool.CheckIfRequestedStop())
                    {
                        tcpListener.Dispose();
                        return;
                    }
                }
            });
        }

        public void SendMessage(string message, string ipAddress, int port)
        {
            TcpClient tcpClient = new TcpClient(ipAddress, port);
            NetworkStream stream = tcpClient.GetStream();
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
            stream.Dispose();
            tcpClient.Dispose();
        }

        public void SendFile(string filePath, string ipAddress, int port)
        {
            TcpClient tcpClient = new TcpClient(ipAddress, port);
            NetworkStream stream = tcpClient.GetStream();
            byte[] fileBytes = File.ReadAllBytes(filePath);
            stream.Write(fileBytes, 0, fileBytes.Length);
            stream.Dispose();
            tcpClient.Dispose();
        }
    }
}
