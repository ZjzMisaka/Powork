﻿using System;
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
        Message = 0,
        Error = 1,
        FileRequest = 2,
        FileInfo = 3,
    }
    public class UserMessage
    {
        public UserMessage() 
        {
            MessageBody = new List<UserMessageBody>();
        }
        /// <summary>
        /// Sender IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Sender name
        /// </summary>
        public string Name { get; set; }
        public List<UserMessageBody> MessageBody { get; set; }
        public MessageType Type { get; set; }
        public string Time { get; set; }
        public int ID { get; set; }
    }
    public class UserMessageBody
    {
        public string Content { get; set; }
        public ContentType Type { get; set; }
    }
}
