using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model.Evidence
{
    public enum Type 
    {
        EmptyRectangle,
        FilledRectangle,
        Line,
    }
    public class Shape
    {
        public Type Type { get; set; }
        public string Content { get; set; }
        public ShapePosition Position { get; set; }
    }
}
