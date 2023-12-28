using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Powork.Model.Evidence
{
    public class ShapePosition
    {
        public int Col1 { get; set; }
        public int Col2 { get; set; }
        public int Row1 { get; set; }
        public int Row2 { get; set; }
        public int Dx1 { get; set; }
        public int Dy1 { get; set; }
        public int Dx2 { get; set; }
        public int Dy2 { get; set; }

        public int RowOffsetForImage { get; set; }
        public int ColOffsetForImage { get; set; }
    }
}
