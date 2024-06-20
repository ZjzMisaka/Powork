using System.Windows;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using Ookii.Dialogs.Wpf;
using Powork.Model;
using Powork.Repository;
using Powork.Service;
using Powork.View;
using Powork.ViewModel;

namespace Powork.Helper
{
    public static class NotificationHelper
    {
        public static INavigationService NavigationService { get; set; }
        public static MainWindowViewModel MainWindowViewModel { get; set; }
        public static void ShowNotification(TCPMessageBase tcpMessage, Team team = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                if (!WindowHelper.IsWindowActive(mainWindow))
                {
                    int maxLineCount = 4;
                    int lineCount = 0;

                    List<string> fileIDList = new List<string>();

                    ToastContentBuilder builder = new ToastContentBuilder()
                        .AddArgument("userIP", tcpMessage.SenderIP)
                        .AddArgument("userName", tcpMessage.SenderName);
                    if (tcpMessage.Type == MessageType.UserMessage)
                    {
                        builder.AddText($"New message from {tcpMessage.SenderName}");
                        ++lineCount;
                    }
                    else if (tcpMessage.Type == MessageType.TeamMessage)
                    {
                        builder.AddArgument("TeamID", ((TeamMessage)tcpMessage).TeamID);
                        string teamName = team == null ? "Unknown" : TeamRepository.SelectTeam(((TeamMessage)tcpMessage).TeamID).Name;
                        builder.AddText($"New message from {tcpMessage.SenderName} ({teamName})");
                        ++lineCount;
                    }
                    builder.AddInputTextBox("input");

                    foreach (TCPMessageBody messageBody in tcpMessage.MessageBody)
                    {
                        if (messageBody.Type == ContentType.Text)
                        {
                            if (lineCount == maxLineCount - 1)
                            {
                                continue;
                            }
                            ++lineCount;
                            if (lineCount == maxLineCount - 1)
                            {
                                builder.AddText(messageBody.Content + "......");
                            }
                            else
                            {
                                builder.AddText(messageBody.Content);
                            }
                        }
                        else if (messageBody.Type == ContentType.Picture)
                        {
                            builder.AddInlineImage(new Uri(messageBody.Content));
                        }
                        else if (messageBody.Type == ContentType.File)
                        {
                            fileIDList.Add(messageBody.ID);
                        }
                    }

                    if (fileIDList.Count > 0)
                    {
                        builder.AddButton(new ToastButton()
                                .SetContent($"Download {fileIDList.Count} file{(fileIDList.Count == 1 ? "" : "s")}")
                                .AddArgument("action", "Download")
                                .AddArgument("file", string.Join("|", fileIDList)));
                    }

                    builder.AddButton(new ToastButton()
                            .SetContent("Send")
                            .AddArgument("action", "Send"));
                    builder.Show();
                }
            }, DispatcherPriority.Normal);
        }

        public static void ToastNotificationManagerCompatActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                args.TryGetValue("userIP", out string userIP);
                args.TryGetValue("userName", out string userName);
                bool hasTeamIDArgs = args.TryGetValue("teamID", out string teamID);

                bool hasActionArgs = args.TryGetValue("action", out string value);
                if (hasActionArgs && value == "Send")
                {
                    toastArgs.UserInput.TryGetValue("input", out object obj);
                    string input = (string)obj;

                    if (!hasTeamIDArgs)
                    {
                        User user = new User()
                        {
                            IP = userIP,
                            Name = userName,
                        };
                        MessageHelper.SendUserMessage(input, user);
                    }
                    else
                    {
                        Team team = new Team()
                        {
                            ID = teamID,
                        };
                        MessageHelper.SendTeamMessage(input, team);
                    }
                }
                else if (hasActionArgs && value == "Download")
                {
                    string fileIDStr = args.Get("file");
                    List<string> fileIDList = fileIDStr.Split("|").ToList();

                    foreach (string fileID in fileIDList)
                    {
                        var fbd = new VistaFolderBrowserDialog
                        {
                            SelectedPath = AppDomain.CurrentDomain.BaseDirectory,
                            Description = "Select a folder",
                            UseDescriptionForTitle = true
                        };

                        bool result = (bool)fbd.ShowDialog();

                        if (result && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            GlobalVariables.TcpServerClient.RequestFile(fileID, userIP, GlobalVariables.TcpPort, fbd.SelectedPath);
                        }
                    }
                }
                else
                {
                    Application.Current.MainWindow.Show();
                    MainWindowViewModel.Topmost = true;
                    MainWindowViewModel.Topmost = false;

                    if (!hasTeamIDArgs)
                    {
                        NavigationService.Navigate(typeof(MessagePage), new MessagePageViewModel(ServiceLocator.GetService<INavigationService>(), userIP, userName));
                    }
                    else
                    {
                        NavigationService.Navigate(typeof(TeamPage), new TeamPageViewModel(teamID));
                    }
                }
            });
        }
    }
}
