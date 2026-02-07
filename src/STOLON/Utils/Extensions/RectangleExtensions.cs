using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Rectangle ClampRectangle(this Rectangle rect, Sides draggedSides, int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            return rect.ClampRectangle(draggedSides, new Point(minWidth, minHeight), new Point(maxWidth, maxHeight));
        }
    }
}
