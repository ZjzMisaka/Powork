using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using PowerThreadPool.Results;
using Powork.Constant;
using Powork.Model;
using Powork.Repository;

namespace Powork.Helper
{
    internal static class MessageHelper
    {
        internal static void ConvertImageInMessage(TCPMessageBase tcpMessage)
        {
            foreach (TCPMessageBody messageBody in tcpMessage.MessageBody)
            {
                if (messageBody.Type == ContentType.Picture)
                {
                    byte[] imageBytes = Convert.FromBase64String(messageBody.Content);

                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                    }

                    bitmapImage.Freeze();

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Location.Temp, DateTime.Now.ToString(Format.DateTimeFormat));
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string localFilePath = Path.Combine(directoryPath, DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + Guid.NewGuid() + ".png");

                    using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(fileStream);
                    }

                    messageBody.Content = localFilePath;
                }
            }
        }

        internal static async void SendUserMessage(string text, User nowUser)
        {
            List<TCPMessageBody> userMessageBodyList = new List<TCPMessageBody>();
            userMessageBodyList.Add(new TCPMessageBody()
            {
                Content = text,
                Type = ContentType.Text
            });

            UserMessage userMessage = new UserMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = userMessageBodyList,
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.UserMessage,
            };

            string message = JsonConvert.SerializeObject(userMessage);

            Task<ExecuteResult<Exception>> task = GlobalVariables.TcpServerClient.SendMessage(message, nowUser.IP, GlobalVariables.TcpPort);
            Exception ex = (await task).Result;

            MessageHelper.ConvertImageInMessage(userMessage);

            if (ex != null)
            {
                List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = Application.Current.FindResource("UserOffline").ToString() }];
                UserMessage errorMessage = new UserMessage()
                {
                    Type = MessageType.Error,
                    MessageBody = errorContent,
                };

                UserMessageRepository.InsertMessage(errorMessage, nowUser.IP, nowUser.Name);
            }
            UserMessageRepository.InsertMessage(userMessage, nowUser.IP, nowUser.Name);
        }

        internal static async void SendTeamMessage(string text, Team nowTeam)
        {
            List<TCPMessageBody> teamMessageBodyList = new List<TCPMessageBody>();
            teamMessageBodyList.Add(new TCPMessageBody()
            {
                Content = text,
                Type = ContentType.Text
            });
            TeamMessage teamMessage = new TeamMessage
            {
                SenderIP = GlobalVariables.LocalIP.ToString(),
                MessageBody = teamMessageBodyList,
                SenderName = GlobalVariables.SelfInfo.Name,
                Type = MessageType.TeamMessage,
                TeamID = nowTeam.ID,
                LastModifiedTime = DateTime.Parse(TeamRepository.SelectTeam(nowTeam.ID).LastModifiedTime)
            };

            string message = JsonConvert.SerializeObject(teamMessage);

            foreach (User member in TeamRepository.SelectTeamMember(nowTeam.ID))
            {
                if (member.IP == GlobalVariables.SelfInfo.IP && member.Name == GlobalVariables.SelfInfo.Name)
                {
                    continue;
                }

                Task<ExecuteResult<Exception>> task = GlobalVariables.TcpServerClient.SendMessage(message, member.IP, GlobalVariables.TcpPort);
                Exception ex = (await task).Result;
                if (ex != null)
                {
                    List<TCPMessageBody> errorContent = [new TCPMessageBody() { Content = string.Format(Application.Current.FindResource("TeamMemberOffline").ToString(), member.Name) }];
                    TeamMessage errorMessage = new TeamMessage()
                    {
                        Type = MessageType.Error,
                        MessageBody = errorContent,
                        TeamID = nowTeam.ID,
                    };

                    TeamMessageRepository.InsertMessage(errorMessage);
                }
            }
            MessageHelper.ConvertImageInMessage(teamMessage);
            TeamMessageRepository.InsertMessage(teamMessage);
        }
    }
}
