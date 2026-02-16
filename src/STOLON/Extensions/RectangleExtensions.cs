using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class RectangleExtensions
    {
        public static Rectangle ClampRectangle(this in Rectangle rect, Sides draggedSides, Point minSize, Point maxSize)
        {
            int x = rect.X;
            int y = rect.Y;
            int width = rect.Width;
            int height = rect.Height;

            int originalRight = rect.Right;
            int originalBottom = rect.Bottom; // why i dont need to invert this is a mystery im gonna solve later (it works now)

            if (width < minSize.X)
            {
                width = minSize.X;

                if ((draggedSides & Sides.Left) != 0)
                {
                    x = originalRight - width;
                }
            }

            if (height < minSize.Y)
            {
                height = minSize.Y;

                if ((draggedSides & Sides.Bottom) != 0)
                {
                    y = originalBottom - height;
                }
            }

            if (width > maxSize.X)
            {
                width = maxSize.X;

                if ((draggedSides & Sides.Left) != 0)
                {
                    x = originalRight - width;
                }
            }

            if (height > maxSize.Y)
            {
                height = maxSize.Y;

                if ((draggedSides & Sides.Bottom) != 0)
                {
                    y = originalBottom - height;
                }
            }

            return new Rectangle(x, y, width, height);
        }

        public static Rectangle ClampRectangle(this in Rectangle rect, Sides draggedSides, int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            return rect.ClampRectangle(draggedSides, new Point(minWidth, minHeight), new Point(maxWidth, maxHeight));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle At(this in Rectangle rect, in Point pos)
        {
            return new Rectangle(pos.X, pos.Y, rect.Width, rect.Height);
        }

        public static bool SamePrintAs(this in Rectangle first, Rectangle second)
        {
            return first.Height == second.Height && first.Width == second.Width;
        }

        public static Line[] ToLines(this in Rectangle rectangle)
        {
            Line[] lines = new Line[4];

            Point topLeft = new Point(rectangle.Left, rectangle.Top);
            Point topRight = new Point(rectangle.Right, rectangle.Top);
            Point bottomLeft = new Point(rectangle.Left, rectangle.Bottom);
            Point bottomRight = new Point(rectangle.Right, rectangle.Bottom);

            lines[0] = new Line(topLeft, bottomLeft);       //left
            lines[1] = new Line(topLeft, topRight);         //top
            lines[2] = new Line(topRight, bottomRight);     //right
            lines[3] = new Line(bottomLeft, bottomRight);   //bottom

            //DO NOT CHANGE ORDER EVER

            return lines;
        }
    }
}
