using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Model;
using Powork.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Powork.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        private PowerPool powerPool = null;

        private UdpBroadcaster udpBroadcaster;
        private TcpServerClient tcpServerClient;

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowClosingCommand { get; set; }
        public ICommand WindowClosedCommand { get; set; }

        public MainWindowViewModel()
        {
            powerPool = new PowerPool();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(WindowClosing);
            WindowClosedCommand = new RelayCommand(WindowClosed);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            while (udpBroadcaster == null)
            {
                try
                {
                    udpBroadcaster = new UdpBroadcaster(GlobalVariables.UdpPort, powerPool);
                }
                catch
                {
                    ++GlobalVariables.UdpPort;
                }
            }
            
            udpBroadcaster.StartBroadcasting();
            udpBroadcaster.ListenForBroadcasts((udpBroadcastMessage) =>
            {
                if (udpBroadcastMessage.IPEndPoint.Address == GlobalVariables.LocalIP)
                {
                    return;
                }
                MessageBox.Show($"Received broadcast from {udpBroadcastMessage.IPEndPoint.Address}");
            });

            while (tcpServerClient == null)
            {
                try
                {
                    tcpServerClient = new TcpServerClient(GlobalVariables.TcpPort, powerPool);
                }
                catch
                {
                    ++GlobalVariables.TcpPort;
                }
            }
            tcpServerClient.StartListening(stream =>
            {
                using var reader = new StreamReader(stream);
                var message = reader.ReadToEnd();

                MessageBox.Show($"Received message: {message}");
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
