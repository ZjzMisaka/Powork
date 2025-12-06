using Powork.Constant;

namespace Powork.Model
{
    public enum MessageType
    {
        UserMessage = 0,
        TeamMessage = 1,
        Error = 2,
        FileRequest = 3,
        FileInfo = 4,
        TeamInfoRequest = 5,
        TeamInfo = 6,
        ShareInfoRequest = 7,
        ShareInfo = 8,
        ProjectListRequest = 9,
        ProjectListResponse = 10,
        ScheduleRequest = 11,
        ScheduleResponse = 12,
    }

    public class TCPMessageBase
    {
        public TCPMessageBase()
        {
            MessageBody = new List<TCPMessageBody>();
        }
        public string RequestID { get; set; }
        /// <summary>
        /// Sender IP
        /// </summary>
        public string SenderIP { get; set; }
        /// <summary>
        /// Sender name
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// team id
        /// </summary>
        public List<TCPMessageBody> MessageBody { get; set; }
        public MessageType Type { get; set; }
        public string Time { get; set; } = DateTime.Now.ToString(Format.DateTimeFormatWithMilliseconds);
    }
}
