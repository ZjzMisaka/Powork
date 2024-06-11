using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Model
{
    public enum ContentType
    {
        Text = 0,
        File = 1,
        Picture = 2,
    }
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
    }
    public class TCPMessage
    {
        public TCPMessage() 
        {
            MessageBody = new List<TCPMessageBody>();
        }
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
        public string TeamID { get; set; }
        public List<TCPMessageBody> MessageBody { get; set; }
        public MessageType Type { get; set; }
        public string Time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        public int ID { get; set; }
    }
    public class TCPMessageBody
    {
        public string Content { get; set; }
        public string ID { get; set; }
        public ContentType Type { get; set; }
    }
}
