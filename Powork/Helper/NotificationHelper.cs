using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using Powork.Model;
using Powork.Repository;
using Powork.Service;
using Powork.View;
using Powork.ViewModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;

namespace Powork.Helper
{
    public static class NotificationHelper
    {
        public static INavigationService NavigationService { get; set; }
        public static void ShowNotification(TCPMessage userMessage, Team team = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                if (!WindowHelper.IsWindowActive(mainWindow))
                {
                    int maxLineCount = 4;
                    int lineCount = 0;

                    int fileIndex = 0;
                    List<string> fileIDList = new List<string>();

                    ToastContentBuilder builder = new ToastContentBuilder()
                        .AddArgument("userIP", userMessage.SenderIP)
                        .AddArgument("userName", userMessage.SenderName);
                    if (userMessage.Type == MessageType.UserMessage)
                    {
                        builder.AddText($"New message from {userMessage.SenderName}");
                        ++lineCount;
                    }
                    else if (userMessage.Type == MessageType.TeamMessage)
                    {
                        string teamName = team == null ? "Unknown" : TeamRepository.SelectTeam(userMessage.TeamID).Name;
                        builder.AddText($"New message from {userMessage.SenderName} ({teamName})");
                        ++lineCount;
                    }
                    builder.AddInputTextBox("input");

                    foreach (TCPMessageBody messageBody in userMessage.MessageBody)
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
                            if (lineCount == maxLineCount - 1)
                            {
                                continue;
                            }
                            ++lineCount;
                            if (lineCount == maxLineCount - 1)
                            {
                                builder.AddText($"File {++fileIndex}: {messageBody.Content}......");
                            }
                            else
                            {
                                builder.AddText($"File {++fileIndex}: {messageBody.Content}");
                            }
                            fileIDList.Add(messageBody.ID);
                        }
                    }

                    if (fileIDList.Count > 0)
                    {
                        builder.AddButton(new ToastButton()
                                .SetContent($"Download")
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
                bool hasActionArgs = args.TryGetValue("action", out string value);
                if (hasActionArgs && value  == "Send")
                {
                    toastArgs.UserInput.TryGetValue("input", out object obj);
                    string input = (string)obj;

                    // Send
                }
                else if (hasActionArgs && value == "Download")
                {
                    string fileIDStr = args.Get("file");
                    List<string> fileIDList = fileIDStr.Split("|").ToList();

                    // Download
                }
                else
                {
                    Application.Current.MainWindow.Show();
                    args.TryGetValue("userIP", out string userIP);
                    args.TryGetValue("userName", out string userName);
                    bool hasTeamIDArgs = args.TryGetValue("teamID", out string teamID);

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
