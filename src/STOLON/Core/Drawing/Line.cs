using System.Runtime.CompilerServices;

namespace STOLON
{
    public readonly struct Line : IEquatable<Line> // does not implement IDrawable, see DrawLine().
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;

        public Line(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public Line(float startX, float startY, float endX, float endY)
            : this(new Vector2(startX, startY), new Vector2(endX, endY)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line Offset(Vector2 amount)
        {
            return new Line(Start + amount, End + amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLength() => Vector2.Distance(Start, End); // not calling it Length() like with Vector2.Length()

        public bool IsNear(Vector2 vector, int threshold = 10)
        {
            Vector2 lineDirection = End - Start;
            float projectionLength = Vector2.Dot(vector - Start, lineDirection) / lineDirection.Length();

            if (projectionLength < 0) projectionLength = 0;
            else if (projectionLength > lineDirection.Length()) projectionLength = lineDirection.Length();

            return Vector2.Distance(vector, Start + projectionLength * Vector2.Normalize(lineDirection)) <= threshold;
        }

        public static Line CreateHorizontal(float startX, float endX, float y)
        {
            return new Line(startX, y, endX, y);
        }

        public static Line CreateVertical(float x, float startY, float endY)
        {
            return new Line(x, startY, x, endY);
        }

        public bool Equals(Line other) => Start == other.Start && End == other.End;

        public override bool Equals(object? obj) => obj is Line other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Start, End);

        public static bool operator ==(Line left, Line right) => left.Equals(right);

        public static bool operator !=(Line left, Line right) => !(left == right);

        public override string ToString() => $"{{{Start} to {End}}}";
    }
}
