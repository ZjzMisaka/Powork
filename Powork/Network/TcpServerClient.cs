using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using PowerThreadPool;
using System.Windows;
using Powork.Model;
using Newtonsoft.Json;

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

        public bool SendMessage(string message, string ipAddress, int port)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                }
            }
        }

        public void SendFile(string filePath, string ipAddress, int port, string relativePath = "")
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, port);

                using (NetworkStream stream = tcpClient.GetStream())
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    FilePack filePack = new FilePack()
                    {
                        Name = new DirectoryInfo(filePath).Name,
                        RelativePath = relativePath,
                    };

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        filePack.Buffer = buffer;
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(filePack));
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void RequestFile(string guid, string ipAddress, int port)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();

                List<UserMessageBody> messageBody = [new UserMessageBody() { Content = guid }];
                UserMessage getFileMessage = new UserMessage()
                {
                    Type = MessageType.FileRequest,
                    IP = GlobalVariables.SelfInfo[0].IP,
                    MessageBody = messageBody,
                };

                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getFileMessage));
                stream.Write(bytes, 0, bytes.Length);
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                }
            }
        }
    }
}
