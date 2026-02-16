using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    /// <summary>
    /// The user interface for the <see cref="Environment"/>.
    /// </summary>
    [Dependency(ServiceLifetime.Singleton)]
    public class Interface : IComponent
    {
        private readonly IRichLogger _logger;
        private readonly ITextframe _textframe;

        public DefaultDictionary<string, UIElementUpdateData> UpdateDump { get; }

        public const int LineWidth = 2;

        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public Interface(IRichLogger logger, ITextframe textframe)
        {
            _logger = logger;

            _logger.Log(">[s]contructing stolon ui");

            UpdateDump = new DefaultDictionary<string, UIElementUpdateData>(s => new UIElementUpdateData(false, null));

            _logger.Success();

            _textframe = textframe;
        }

        public void Update(int elapsedMilliseconds)
        {
            _textframe.Update(elapsedMilliseconds);
        }
        //public void PostUpdate(int elapsedMilliseconds)
        //{
        //    foreach (string item in Elements.Keys)
        //        if (Elements[item].Type == UIElementType.Listen)
        //        {
        //            if (_updateData[item].IsClicked)
        //                STOLON.Audio.Play(_updateData[item].ClickSound);
        //            if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
        //                if (updateData2.IsClicked) MenuPath = UIElement.GetParentPath(item);
        //        }
        //}
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public void Draw(DrawingContext drawingContext)
        {
            _textframe.Draw(drawingContext);
        }
    }

    public readonly struct UIPath : IEnumerable<string>
    {
        public string TopId => _segments[0];
        public string ParentId => _segments[^1];
        public string DestinationId => _segments.Last();
        public int Lenght => _segments.Length;
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
