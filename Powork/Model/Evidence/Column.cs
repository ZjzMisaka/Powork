using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.XSSF.UserModel;
using Powork.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public class Column : ObservableObject
    {
        private TestingPageViewModel parent;
        public Column(TestingPageViewModel parent)
        {
            this.parent = parent;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                SetProperty<string>(ref name, value);
                parent.SheetModel = parent.SheetModel;
            }
        }

        private ObservableCollection<Block> blockList;
        public ObservableCollection<Block> BlockList
        {
            get { return blockList; }
            set
            {
                SetProperty<ObservableCollection<Block>>(ref blockList, value);
                parent.SheetModel = parent.SheetModel;
            }
        }

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
                    return row.Cells[i].ColumnIndex;
                }
            }

            return -1;
        }
    }
}
