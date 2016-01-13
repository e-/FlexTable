using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;

namespace FlexTable.Crayon
{
    public class Const
    {
        public const Double PaddingLeft = 0;
        public const Double PaddingTop = 30;
        public const Double PaddingRight = 10;
        public const Double PaddingBottom = 0;

        public const Double HorizontalAxisHeight = 25;
        public const Double HorizontalAxisLabelHeight = 20;
        public const Double VerticalAxisWidth = 40;
        public const Double VerticalAxisLabelWidth = 20;

        public const Double LegendPatchWidth = 20;
        public const Double LegendPatchHeight = 20;
        public const Double LegendPatchSpace = 10;

        public const Double MinimumLegendWidth = 100;

        public const Double DragToFilterThreshold = 40;
        public const Double StrikeThroughMinWidth = 30;
        public const Double StrikeThroughMaxHeight = 15;

        public const Double MinimumHandleHeight = 50;

        public const Double HighlightedBarWidthRatio = 0.6;

        public static Boolean IsStrikeThrough(Rect rect)
        {
            return rect.Width >= StrikeThroughMinWidth && rect.Height <= StrikeThroughMaxHeight;
        }

        public static Boolean IsIntersected(Rect r1, Rect r2)
        {
         /*   Point p1 = new Point(r1.X, r1.Y),
                  p2 = new Point(r1.X + r1.Width, r1.Y),
                  p3 = new Point(r1.X, r1.Y + r1.Height),
                  p4 = new Point(r1.X + r1.Width, r1.Y + r1.Height);
                  */
            return
                !((r1.X <= r2.X && r1.X + r1.Width <= r2.X) ||
                (r1.Y <= r2.Y && r1.Y + r1.Height <= r2.Y) ||
                (r1.X >= r2.X + r2.Width && r1.X + r1.Width >= r2.X + r2.Width) ||
                (r1.Y >= r2.Y + r2.Height && r1.Y + r1.Height >= r2.Y + r2.Height));
        }
    }
}
