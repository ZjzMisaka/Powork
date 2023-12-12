using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PowerThreadPool;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Powork.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        private PowerPool powerPool = null;

        private UdpBroadcaster udpBroadcaster;

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MainWindowViewModel()
        {
            powerPool = new PowerPool();

            CommonRepository.CreateDatabase();
            CommonRepository.CreateTable();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            udpBroadcaster = new UdpBroadcaster(GlobalVariables.UdpPort, powerPool);
            GlobalVariables.UserList = new ObservableCollection<User>(UserRepository.SelectUser());

            udpBroadcaster.StartBroadcasting();
            udpBroadcaster.ListenForBroadcasts((user) =>
            {
                if (user.IP == GlobalVariables.LocalIP.ToString())
                {
                    return;
                }

                if (UserRepository.SelectUserByIpName(user.IP, user.Name).Count == 0)
                {
                    User insertUser = new User()
                    {
                        Name = user.Name,
                        IP = user.IP,
                        GroupName = user.GroupName
                    };
                    UserRepository.InsertUser(insertUser);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GlobalVariables.UserList.Add(insertUser);
                    });
                }
            });

            GlobalVariables.TcpServerClient = new TcpServerClient(GlobalVariables.TcpPort, powerPool);
            GlobalVariables.TcpServerClient.StartListening((stream, ip) =>
            {
                if (ip == GlobalVariables.SelfInfo[0].IP)
                {
                    return;
                }

                using var reader = new StreamReader(stream);
                string message = reader.ReadToEnd();
                UserMessage userMessage = JsonConvert.DeserializeObject<UserMessage>(message);
                if (userMessage.Type == MessageType.Message)
                {
                    UserMessageHelper.ConvertImageInMessage(userMessage);
                    GlobalVariables.InvokeGetMessageEvent(userMessage);
                }
                else if (userMessage.Type == MessageType.FileRequest)
                {
                    string guid = userMessage.MessageBody[0].Content;
                    string path = FileRepository.SelectFile(guid);
                    GlobalVariables.TcpServerClient.SendFile(path, userMessage.IP, guid, GlobalVariables.TcpPort);
                }
                else if (userMessage.Type == MessageType.FileInfo)
                {
                    string json = userMessage.MessageBody[0].Content;
                    Model.FileInfo fileInfo = JsonConvert.DeserializeObject<Model.FileInfo>(json);
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", fileInfo.RelativePath);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    try
                    {
                        string receivedFilePath = Path.Combine(path, fileInfo.Name);
                        using (var fileStream = new FileStream(receivedFilePath, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                            }
                        }

                        MessageBox.Show("File received successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            });
        }

        private void WindowClosing(CancelEventArgs eventArgs)
        {
            if (powerPool.Stop())
            {
                powerPool.Dispose();
            }
        }

        private void WindowClosed()
        {
        }
    }
}
