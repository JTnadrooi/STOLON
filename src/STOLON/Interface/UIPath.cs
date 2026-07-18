using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    /// <summary>
    /// The user interface for the <see cref="Environment"/>.
    /// </summary>
    [Dependency(ServiceLifetime.Singleton)]
    public static class Interface
    {
        public const int LineWidth = 2;
    }

    public readonly struct UIPath : IEnumerable<string>
    {
        public string TopId => _segments[0];
        public string ParentId => _segments[^1];
        public string DestinationId => _segments.Last();
        public int Length => _segments.Length;
        public ReadOnlySpan<string> Segments => _segments;
        private readonly string[] _segments;
        public UIPath(IEnumerable<string> segments) => _segments = segments.ToArray();
        public string this[int index] => _segments[index];
        public IEnumerator<string> GetEnumerator() => (IEnumerator<string>)_segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => $"[{_segments.ToJoinedString(">")}]";
        public override int GetHashCode() => _segments.ToJoinedString(string.Empty).GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj) => obj.GetHashCode() == GetHashCode();

        public static UIPath TopPath { get; } = new UIPath([UIElement.TopId]);
    }
}
