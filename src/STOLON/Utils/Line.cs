namespace STOLON
{
    public readonly struct Line
    {
        public Point Start { get; }
        public Point End { get; }
        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
        public void Offset(Point amount)
        {
            throw new NotImplementedException();
        }
        public bool IsNear(Vector2 vector, int threshold = 10)
        {
            Vector2 lineDirection = (End - Start).ToVector2();
            float projectionLength = Vector2.Dot(vector - Start.ToVector2(), lineDirection) / lineDirection.Length();

            if (projectionLength < 0) projectionLength = 0;
            else if (projectionLength > lineDirection.Length()) projectionLength = lineDirection.Length();

            return Vector2.Distance(vector, Start.ToVector2() + projectionLength * Vector2.Normalize(lineDirection)) <= threshold;
        }
    }
}
