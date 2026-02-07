using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class VectorPointExtensions
    {
        public static Point Clamp(this in Point point, Point min, Point max)
        {
            return new Point(
                Math.Clamp(point.X, min.X, max.X),
                Math.Clamp(point.Y, min.Y, max.Y)
            );
        }
    }
}
