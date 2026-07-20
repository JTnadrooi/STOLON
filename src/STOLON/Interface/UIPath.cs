using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public readonly struct UIPath : IEnumerable<string>
    {
        public string TopId => _segments[0];

        public string ParentId => _segments[^1];

        public string DestinationId => _segments.Last();

        public int Length => _segments.Length;

        private readonly string[] _segments;
        public ReadOnlySpan<string> Segments => _segments;

        public string this[int index] => _segments[index];

        public UIPath(IEnumerable<string> segments) => _segments = segments.ToArray();

        public IEnumerator<string> GetEnumerator() => (IEnumerator<string>)_segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"[{_segments.ToJoinedString(">")}]";
        public override int GetHashCode() => _segments.ToJoinedString(string.Empty).GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj) => obj.GetHashCode() == GetHashCode();

        public static UIPath TopPath { get; } = new UIPath([UIElement.TopId]);
    }
}
