using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public class Sheet
    {
        public string Name { get; set; }
        public List<string> RowTitleList { get; set; }
        public List<Column> ColumnList { get; set; }
    }
}
