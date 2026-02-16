namespace STOLON
{
    public static class Value2Extensions
    {
        public static SizeF ToSizeF(this Point point) => new SizeF(point.X, point.Y);
        public static SizeF ToSizeF(this Vector2 vector) => new SizeF(vector.X, vector.Y);
        public static Point ToPoint(this SizeF size) => new Point((int)size.Width, (int)size.Height);
        public static Vector2 ToVector(this SizeF size) => new Vector2(size.Width, size.Height);
        public static float Size(this Vector2 vector) => vector.X * vector.Y;
        public static int Size(this Point point) => point.X * point.Y;
    }
}
