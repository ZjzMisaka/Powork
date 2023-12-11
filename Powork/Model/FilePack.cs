using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model
{
    public class FilePack
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public byte[] Buffer { get; set; }
    }
}
