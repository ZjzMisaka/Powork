using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Model
{
    internal enum MessageType
    {
        Text,
        File,
        Picture
    }
    internal class UserMessage
    {
        public UserMessage() 
        {
            MessageBody = new List<UserMessageBody>();
        }
        public string IP { get; set; }
        public string Name { get; set; }
        public List<UserMessageBody> MessageBody { get; set; }
        public MessageType Type { get; set; }
        public string Time { get; set; }
    }
    internal class UserMessageBody
    {
        public string Content { get; set; }
        public MessageType Type { get; set; }
    }
}
