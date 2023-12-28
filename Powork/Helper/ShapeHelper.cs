using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Powork.Helper
{
    public static class ShapeHelper
    {
        public static void CreateLine(XSSFSheet sheet, int dx1, int dy1, int dx2, int dy2, int col1, int row1, int col2, int row2)
        {
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            XSSFClientAnchor anchor = new XSSFClientAnchor(dx1, dy1, dx2, dy2, col1, row1, col2, row2);
            XSSFSimpleShape shape = drawing.CreateSimpleShape(anchor);
            shape.ShapeType = (int)ShapeTypes.Line;
            shape.SetLineStyleColor(255, 0, 0);
            shape.LineWidth = 2;
        }

        public static void CreateBorderRectangle(XSSFSheet sheet, int dx1, int dy1, int dx2, int dy2, int col1, int row1, int col2, int row2)
        {
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            XSSFClientAnchor anchor = new XSSFClientAnchor(dx1, dy1, dx2, dy2, col1, row1, col2, row2);
            XSSFSimpleShape shape = drawing.CreateSimpleShape(anchor);
            shape.ShapeType = (int)ShapeTypes.Rectangle;
            shape.IsNoFill = true;
            shape.SetLineStyleColor(255, 0, 0);
            shape.LineWidth = 2;
        }

        public static void CreateRectangleWithText(XSSFSheet sheet, int dx1, int dy1, int dx2, int dy2, int col1, int row1, int col2, int row2, string text)
        {
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            XSSFClientAnchor anchor = new XSSFClientAnchor(dx1, dy1, dx2, dy2, col1, row1, col2, row2);
            XSSFSimpleShape shape = drawing.CreateSimpleShape(anchor);
            shape.ShapeType = (int)ShapeTypes.Rectangle;
            shape.SetFillColor(240, 248, 255);
            shape.SetLineStyleColor(0, 0, 255);
            shape.LineWidth = 2;
            shape.SetText(text);
        }
    }
}
