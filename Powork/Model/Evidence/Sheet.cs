using CommunityToolkit.Mvvm.ComponentModel;
using Powork.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public class Sheet : ObservableObject
    {
        private TestingPageViewModel parent;
        public Sheet(TestingPageViewModel parent) 
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

        private ObservableCollection<string> rowTitleList;
        public ObservableCollection<string> RowTitleList
        {
            get { return rowTitleList; }
            set
            {
                SetProperty<ObservableCollection<string>>(ref rowTitleList, value);
                parent.SheetModel = parent.SheetModel;
            }
        }
        private ObservableCollection<Column> columnList;
        public ObservableCollection<Column> ColumnList
        {
            get { return columnList; }
            set
            {
                SetProperty<ObservableCollection<Column>>(ref columnList, value);
                parent.SheetModel = parent.SheetModel;
            }
        }
    }
}
