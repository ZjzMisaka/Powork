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
                    string path = FileRepository.SelectFile(userMessage.MessageBody[0].Content);
                    GlobalVariables.TcpServerClient.SendFile(path, userMessage.IP, GlobalVariables.TcpPort);
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
