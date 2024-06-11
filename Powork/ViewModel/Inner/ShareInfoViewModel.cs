using Powork.Model;

namespace Powork.ViewModel.Inner
{
    public class ShareInfoViewModel
    {
        public ShareInfoViewModel(ShareInfo shareInfo)
        {
            Guid = shareInfo.Guid;
            Path = shareInfo.Path;
            Name = shareInfo.Name;
            Extension = shareInfo.Extension;
            Type = shareInfo.Type;
            Size = shareInfo.Size;
            ShareTime = shareInfo.ShareTime;
            CreateTime = shareInfo.CreateTime;
            LastModifiedTime = shareInfo.LastModifiedTime;
        }

        public string Guid { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string ShareTime { get; set; }
        public string CreateTime { get; set; }
        public string LastModifiedTime { get; set; }
        public bool IsSelected { get; set; }
    }
}
