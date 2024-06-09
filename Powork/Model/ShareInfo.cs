using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model
{
    internal class ShareInfo
    {
        public string Guid { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
        public string ShareTime { get; set; }
        public string CreateTime { get; set; }
        public string LastModifiedTime { get; set; }
    }
}
