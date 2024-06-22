using System.Collections.Concurrent;
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
using Powork.Repository;

namespace Powork.Network
{

    public class TcpServerClient
    {
        private readonly TcpListener _tcpListener;

        public ConcurrentDictionary<string, string> savePathDict = new ConcurrentDictionary<string, string>();

        public TcpServerClient(int port)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public void StartListening(Action<TcpClient, string> onReceive)
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

                    GlobalVariables.PowerPool.QueueWorkItem(() =>
                    {
                        TcpClient client = _tcpListener.AcceptTcpClient();
                        string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        if (GlobalVariables.SelfInfo == null || ip == GlobalVariables.SelfInfo.IP)
                        {
                            client.Close();
                            return;
                        }

                        onReceive(client, ip);

                        client.Close();
                    });

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
                    DelaySendingMessageRepository.InsertMessage(ipAddress, message);
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

        public void SendFile(string requestID, string filePath, string guid, string ipAddress, int port, string relativePath = "", int fileCount = 1, long totalSize = -1, bool isFolder = false, string folderName = "")
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
                    FileMessage getFileMessage = new FileMessage()
                    {
                        RequestID = requestID,
                        Type = MessageType.FileInfo,
                        SenderIP = GlobalVariables.SelfInfo.IP,
                        MessageBody = messageBody,
                        FileCount = fileCount,
                        TotalSize = totalSize,
                        IsFolder = isFolder,
                        FolderName = folderName,
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
            GlobalVariables.TcpServerClient.SendFileFinish(requestID, guid, ipAddress, port);
        }

        public void SendFileFinish(string requestID, string guid, string ipAddress, int port)
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
                FileMessage getFileMessage = new FileMessage()
                {
                    RequestID = requestID,
                    Type = MessageType.FileInfo,
                    SenderIP = GlobalVariables.SelfInfo.IP,
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
                FileMessage getFileMessage = new FileMessage()
                {
                    RequestID = Guid.NewGuid().ToString(),
                    Type = MessageType.FileRequest,
                    SenderIP = GlobalVariables.SelfInfo.IP,
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
                TeamMessage tcpMessage = new TeamMessage()
                {
                    Type = MessageType.TeamInfoRequest,
                    SenderIP = GlobalVariables.SelfInfo.IP,
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
        }

        internal void SendTeamInfo(string teamID, string teamName, string lastModifiedTime, List<User> users, string senderIP)
        {
            TCPMessageBody tcpMessageBody;
            TeamMessage teamMessage = new TeamMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = new List<TCPMessageBody>(),
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.TeamInfo,
            };

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.TeamID, teamID));
            teamMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.TeamName, teamName));
            teamMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, string>(MessageBodyContentKey.LastModifiedTime, lastModifiedTime));
            teamMessage.MessageBody.Add(tcpMessageBody);

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<User>>(MessageBodyContentKey.Members, users));
            teamMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(teamMessage);

#pragma warning disable CS4014
            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
#pragma warning restore CS4014
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
                    TCPMessageBase getShareInfoMessage = new TCPMessageBase()
                    {
                        Type = MessageType.ShareInfoRequest,
                        SenderIP = GlobalVariables.SelfInfo.IP,
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
            TCPMessageBase tcpMessage = new TCPMessageBase
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = new List<TCPMessageBody>(),
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.ShareInfo,
            };

            tcpMessageBody = new TCPMessageBody();
            tcpMessageBody.Content = JsonConvert.SerializeObject(new KeyValuePair<string, List<ShareInfo>>(MessageBodyContentKey.ShareInfos, shareInfos));
            tcpMessage.MessageBody.Add(tcpMessageBody);

            string message = JsonConvert.SerializeObject(tcpMessage);

#pragma warning disable CS4014
            GlobalVariables.TcpServerClient.SendMessage(message, senderIP, GlobalVariables.TcpPort);
#pragma warning restore CS4014
        }
    }
}
