using Powork.Constant;

namespace Powork.Model
{
    public class FileMessage : TCPMessageBase
    {
        public FileMessage()
        {
            MessageBody = new List<TCPMessageBody>();
        }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
        public bool IsFolder { get; set; }
        public string FolderName { get; set; }
    }
}
