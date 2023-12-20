using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public class Column
    {
        public string Name { get; set; }
        public List<Block> BlockList { get; set; }

        public int GetIndex(XSSFSheet nowSheet)
        {
            XSSFRow row = (XSSFRow)nowSheet.GetRow(1);
            if (row == null)
            {
                return -1;
            }

            for (int i = 0; i < row.Cells.Count; ++i)
            {
                if (row.Cells[i].CellType.ToString() != "String")
                {
                    continue;
                }
                string str = row.Cells[i].StringCellValue;
                if (str == Name)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
