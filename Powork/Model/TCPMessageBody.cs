namespace Powork.Model
{
    public enum ContentType
    {
        Text = 0,
        File = 1,
        Picture = 2,
    }

    public class TCPMessageBody
    {
        public string Content { get; set; }
        public string ID { get; set; }
        public ContentType Type { get; set; }
    }
}
