using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using PowerThreadPool.Options;
using PowerThreadPool.Results;
using Powork.Constant;
using Powork.Model;

namespace Powork.Network
{

    public class TcpServerClient
    {
        private readonly TcpListener _tcpListener;

        public Dictionary<string, string> savePathDict = new Dictionary<string, string>();

        public TcpServerClient(int port)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public void StartListening(Action<NetworkStream, string> onReceive)
        {
            _tcpListener.Start();
            GlobalVariables.PowerPool.QueueWorkItem(() =>
            {
                while (true)
                {
                    while (!_tcpListener.Pending())
                    {
                        Thread.Sleep(100);
                        GlobalVariables.PowerPool.StopIfRequested();
                    }
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    onReceive(client.GetStream(), ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    client.Dispose();

                    if (GlobalVariables.PowerPool.CheckIfRequestedStop())
                    {
                        _tcpListener.Dispose();
                        return;
                    }
                }
            }, new WorkOption()
            {
                LongRunning = true
            });
        }

        public async Task<ExecuteResult<Exception>> SendMessage(string message, string ipAddress, int port)
        {
            string id = GlobalVariables.PowerPool.QueueWorkItem<Exception>(() =>
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
            }, new WorkOption<Exception>()
            {
                ShouldStoreResult = true,
            });

            return await GlobalVariables.PowerPool.FetchAsync<Exception>(id, true);
        }

        public string SendFile(string filePath, string guid, string ipAddress, int port, string relativePath = "")
        {
            string sendFileWorkID = GlobalVariables.PowerPool.QueueWorkItem(() =>
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
                            Status = Model.Status.SendFileStart,
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
            });
            return sendFileWorkID;
        }

        public async void SendFileFinish(string filePath, string guid, string ipAddress, int port, List<string> sendFileWorkIDList)
        {
            await GlobalVariables.PowerPool.WaitAsync(sendFileWorkIDList);
            GlobalVariables.PowerPool.QueueWorkItem(() =>
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
            });
        }

        public void RequestFile(string guid, string ipAddress, int port, string savePath)
        {
            GlobalVariables.PowerPool.QueueWorkItem(() =>
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
            });
        }

        public void RequestTeamInfo(string teamID, string ipAddress, int port)
        {
            GlobalVariables.PowerPool.QueueWorkItem(() =>
            {
                TcpClient tcpClient = null;
                NetworkStream stream = null;

                try
                {
                    tcpClient = new TcpClient(ipAddress, port);
                    stream = tcpClient.GetStream();

                    List<TCPMessageBody> messageBody = [new TCPMessageBody() { Content = teamID }];
                    TCPMessage tcpMessage = new TCPMessage()
                    {
                        Type = MessageType.TeamInfoRequest,
                        SenderIP = GlobalVariables.SelfInfo[0].IP,
                        MessageBody = messageBody,
                    };

                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tcpMessage));
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
            });
        }

        internal void SendTeamInfo(string teamID, string teamName, DateTime lastModifiedTime, List<User> users, string senderIP)
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
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.TeamID, teamID));
            userMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.TeamName, teamName));
            userMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.LastModifiedTime, lastModifiedTime.ToString(Format.DateTimeFormatWithMilliseconds)));
            userMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<User>>(MessageBodyContentKey.Members, users));
            userMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(userMessage);

            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
        }

        public void RequestShareInfo(string ipAddress, int port, string name)
        {
            GlobalVariables.PowerPool.QueueWorkItem(() =>
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
                    MessageBox.Show($"User {name} not online");
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
            });
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
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<ShareInfo>>(MessageBodyContentKey.ShareInfos, shareInfos));
            userMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(userMessage);

            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
        }
    }
}
