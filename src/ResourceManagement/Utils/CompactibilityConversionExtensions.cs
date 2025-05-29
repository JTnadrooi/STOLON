using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SizeF = MonoGame.Extended.SizeF;
using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace STOLON
{
    public static class CompactibilityConversionExtensions
    {
        public static SizeF ToSizeF(this Point point) => new SizeF(point.X, point.Y);
        public static SizeF ToSizeF(this Vector2 vector) => new SizeF(vector.X, vector.Y);
        public static Point ToPoint(this SizeF size) => new Point((int)size.Width, (int)size.Height);
        public static Vector2 ToVector(this SizeF size) => new Vector2(size.Width, size.Height);
    }
}
