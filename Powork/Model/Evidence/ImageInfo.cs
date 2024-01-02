using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.POIFS.Properties;
using NPOI.XSSF.UserModel;
using Powork.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Powork.Model.Evidence
{
    public class ImageInfo
    {
        public double WidthInExcel { get; set; }
        public double HeightInExcel { get; set; }
        public XSSFClientAnchor Anchor { get; set; }
    }
}
