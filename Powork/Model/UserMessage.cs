using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Model
{
    public enum MessageType
    {
        Text,
        File
    }
    internal class UserMessage
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public string Time { get; set; }
    }
}
