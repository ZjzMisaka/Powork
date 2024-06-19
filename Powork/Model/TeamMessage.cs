using Powork.Constant;

namespace Powork.Model
{
    public class TeamMessage : TCPMessageBase
    {
        public TeamMessage()
        {
            MessageBody = new List<TCPMessageBody>();
        }
        public string TeamID { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public int ID { get; set; }
    }
}
