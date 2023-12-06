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
        private TcpClient tcpClient;
        private PowerPool powerPool;

        public TcpServerClient(int port, PowerPool powerPool)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            this.powerPool = powerPool;
        }

        public void StartListening(Action<NetworkStream> onReceive)
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
                    var client = tcpListener.AcceptTcpClient();
                    onReceive(client.GetStream());

                    powerPool.StopIfRequested();
                }
            });
        }

        public void SendMessage(string message, string ipAddress, int port)
        {
            tcpClient = new TcpClient(ipAddress, port);
            var stream = tcpClient.GetStream();
            var bytes = Encoding.UTF8.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }

        public void SendFile(string filePath, string ipAddress, int port)
        {
            tcpClient = new TcpClient(ipAddress, port);
            var stream = tcpClient.GetStream();
            var fileBytes = File.ReadAllBytes(filePath);
            stream.Write(fileBytes, 0, fileBytes.Length);
        }
    }
}
