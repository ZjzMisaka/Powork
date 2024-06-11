namespace Powork.Model
{
    public enum Status
    {
        Start = 0,
        SendFileFinish = 1,
        NoSuchFile = 2,
    }
    public class FileInfo
    {
        public Status Status { get; set; }
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string Guid { get; set; }
        public long Size { get; set; }
    }
}
