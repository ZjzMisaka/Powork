using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Powork.Model.Evidence
{
    public class Block
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ImageSource ImageSource { get; set; }
        public List<Shape> ShapeList { get; set; }
    }
}
