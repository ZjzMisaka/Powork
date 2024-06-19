using Powork.Constant;

namespace Powork.Model
{
    public class UserMessage : TCPMessageBase
    {
        public UserMessage()
        {
            MessageBody = new List<TCPMessageBody>();
        }
        public int ID { get; set; }
    }
}
