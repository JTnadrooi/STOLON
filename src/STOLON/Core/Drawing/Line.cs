namespace STOLON
{
    public readonly struct Line // does not implement IDrawable, see DrawLine().
    {
        public readonly Point Start;
        public readonly Point End;

        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }

        public Line(int startX, int startY, int endX, int endY)
            : this(new Point(startX, startY), new Point(endX, endY)) { }

        public Line Offset(Point amount)
        {
            return new Line(Start + amount, End + amount);
        }

        public bool IsNear(Vector2 vector, int threshold = 10)
        {
            Vector2 lineDirection = (End - Start).ToVector2();
            float projectionLength = Vector2.Dot(vector - Start.ToVector2(), lineDirection) / lineDirection.Length();

            if (projectionLength < 0) projectionLength = 0;
            else if (projectionLength > lineDirection.Length()) projectionLength = lineDirection.Length();

            return Vector2.Distance(vector, Start.ToVector2() + projectionLength * Vector2.Normalize(lineDirection)) <= threshold;
        }

        public static Line CreateHorizontal(int startX, int endX, int y)
        {
            return new Line(startX, y, endX, y);
        }

        public static Line CreateVertical(int x, int startY, int endY)
        {
            return new Line(x, startY, x, endY);
        }
    }
}
