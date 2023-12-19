using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public class Column
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<Block> BlockList { get; set; }
    }
}
