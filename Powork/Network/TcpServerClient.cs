using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using PowerThreadPool;
using System.Windows;
using Powork.Model;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Shapes;
using PowerThreadPool.Options;

namespace Powork.Network
{

    public class TcpServerClient
    {
        private TcpListener tcpListener;
        private PowerPool powerPool;

        public Dictionary<string, string> savePathDict = new Dictionary<string, string>();

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
            }, new WorkOption()
            {
                LongRunning = true
            });
        }

        public Exception SendMessage(string message, string ipAddress, int port)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                int length = bytes.Length;
                byte[] lengthPrefix = BitConverter.GetBytes(length);
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                stream.Write(bytes, 0, bytes.Length);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
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

        public void SendFile(string filePath, string guid, string ipAddress, int port, string relativePath = "")
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, port);

                using (NetworkStream networkStream = tcpClient.GetStream())
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    Model.FileInfo fileInfo = new Model.FileInfo()
                    {
                        Guid = guid,
                        Status = Model.Status.Start,
                        Name = new DirectoryInfo(filePath).Name,
                        RelativePath = relativePath,
                        Size = new System.IO.FileInfo(filePath).Length
                    };
                    List<TCPMessageBody> messageBody = [new TCPMessageBody() { Content = JsonConvert.SerializeObject(fileInfo) }];
                    TCPMessage getFileMessage = new TCPMessage()
                    {
                        Type = MessageType.FileInfo,
                        SenderIP = GlobalVariables.SelfInfo[0].IP,
                        MessageBody = messageBody,
                    };
                    byte[] getFileMessageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getFileMessage));
                    int length = getFileMessageBytes.Length;
                    byte[] lengthPrefix = BitConverter.GetBytes(length);
                    networkStream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    networkStream.Write(getFileMessageBytes, 0, getFileMessageBytes.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        networkStream.Write(buffer, 0, bytesRead);
                    }
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SendFileFinish(string filePath, string guid, string ipAddress, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, port);

            using (NetworkStream networkStream = tcpClient.GetStream())
            {
                Model.FileInfo fileInfo = new Model.FileInfo()
                {
                    Guid = guid,
                    Status = Model.Status.SendFileFinish,
                };
                List<TCPMessageBody> messageBody = [new TCPMessageBody() { Content = JsonConvert.SerializeObject(fileInfo) }];
                TCPMessage getFileMessage = new TCPMessage()
                {
                    Type = MessageType.FileInfo,
                    SenderIP = GlobalVariables.SelfInfo[0].IP,
                    MessageBody = messageBody,
                };
                byte[] getFileMessageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getFileMessage));
                int length = getFileMessageBytes.Length;
                byte[] lengthPrefix = BitConverter.GetBytes(length);
                networkStream.Write(lengthPrefix, 0, lengthPrefix.Length);
                networkStream.Write(getFileMessageBytes, 0, getFileMessageBytes.Length);
            }
        }

        public void RequestFile(string guid, string ipAddress, int port, string savePath)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;

            savePathDict[guid] = savePath;

            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();

                List<TCPMessageBody> messageBody = [new TCPMessageBody() { Content = guid }];
                TCPMessage getFileMessage = new TCPMessage()
                {
                    Type = MessageType.FileRequest,
                    SenderIP = GlobalVariables.SelfInfo[0].IP,
                    MessageBody = messageBody,
                };

                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getFileMessage));
                int length = bytes.Length;
                byte[] lengthPrefix = BitConverter.GetBytes(length);
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
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

        public void RequestTeamInfo(string teamID, string ipAddress, int port)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;

            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();

                List<TCPMessageBody> messageBody = [new TCPMessageBody() { Content = teamID }];
                TCPMessage getFileMessage = new TCPMessage()
                {
                    Type = MessageType.TeamInfoRequest,
                    SenderIP = GlobalVariables.SelfInfo[0].IP,
                    MessageBody = messageBody,
                };

                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getFileMessage));
                int length = bytes.Length;
                byte[] lengthPrefix = BitConverter.GetBytes(length);
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
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

        internal void SendTeamInfo(string teamID, string teamName, List<User> users, string senderIP)
        {
            TCPMessageBody tcpMessageBody;
            TCPMessage userMessage = new TCPMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = new List<TCPMessageBody>(),
                SenderName = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.TeamInfo,
            };

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>("team id", teamID));
            userMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>("team name", teamName));
            userMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<User>>("members", users));
            userMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(userMessage);

            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
        }

        public void RequestShareInfo(string ipAddress, int port)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;

            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();

                List<TCPMessageBody> messageBody = [new TCPMessageBody()];
                TCPMessage getShareInfoMessage = new TCPMessage()
                {
                    Type = MessageType.ShareInfoRequest,
                    SenderIP = GlobalVariables.SelfInfo[0].IP,
                    MessageBody = messageBody,
                };

                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getShareInfoMessage));
                int length = bytes.Length;
                byte[] lengthPrefix = BitConverter.GetBytes(length);
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
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

        internal void SendShareInfo(List<ShareInfo> shareInfos, string senderIP)
        {
            TCPMessageBody tcpMessageBody;
            TCPMessage userMessage = new TCPMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = new List<TCPMessageBody>(),
                SenderName = GlobalVariables.SelfInfo[0].Name,
                Type = MessageType.ShareInfo,
            };

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<ShareInfo>>("share infos", shareInfos));
            userMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(userMessage);

            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
        }
    }
}
