using System.Diagnostics;
using System.Text.RegularExpressions;

namespace STOLON
{
    public sealed class Shell : Service, ISingletonDependency
    {
        private readonly IRichLogger _logger;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        private readonly List<ShellRegion> _regions;
        private readonly Vector2 _origin;

        internal IReadOnlyList<ShellRegion> Regions => _regions;

        public static string[] Words { get; } = ["write", "read", "region", "shell", "stolon"]; // temp for autocomplete tests.

        //internal Vector2 Pos { get; }

        public bool HasInputLine
        {
            get => EnsureLastRegionIsTextRegion().HasInputLine;
            set => EnsureLastRegionIsTextRegion().HasInputLine = value;
        }

        public const int RegionClearance = 5;

        public Shell(IRichLogger logger, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input) : base(null)
        {
            _logger = logger;
            _fonts = fonts;
            _input = input;
            _textures = textures;

            _regions = new List<ShellRegion>();

            _origin = new Vector2(10, 0);
        }

        private TextShellRegion EnsureLastRegionIsTextRegion() => EnsureLastRegionIs<TextShellRegion>(() => new TextShellRegion(this, _logger, _fonts, _input, _textures));

        private TRegion EnsureLastRegionIs<TRegion>(Func<TRegion> regionFactory) where TRegion : TextShellRegion
        {
            if (_regions.LastOrDefault() is TRegion t) return t;
            else
            {
                TRegion newRegion = regionFactory.Invoke();

                _regions.Add(newRegion);

                return newRegion;
            }
        }

        internal Vector2 GetRegionPos(ShellRegion region) // VERY SLOW, make regioninfo record and do in Update().
        {
            float x = _origin.X;
            float y = STOLON.V_HEIGHT - RegionClearance;

            foreach (ShellRegion r in _regions)
            {
                y -= r.Height + RegionClearance;
                if (r == region)
                {
                    return new Vector2(x, y);
                }
                y += r.VerticalOverlap;
            }

            throw new InvalidOperationException();
        }

        //internal ShellRegion GetRegionUnderMouse()
        //{

        //}

        public override void Update(int elapsedMilliseconds)
        {
            foreach (ShellRegion region in _regions)
            {
                region.Update(elapsedMilliseconds);
            }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            foreach (ShellRegion region in _regions)
            {
                region.Draw(drawingContext);
            }
        }

        private bool IsLastSectionAcceptingInput()
        {
            if (_regions.Count == 0) return false;
            switch (_regions[^1])
            {
                case TextShellRegion textRegion:
                    return textRegion.HasInputLine;
                default:
                    return false;
            }
        }

        public void Write<T>(T item) => EnsureLastRegionIsTextRegion().Write(item);
        public void Write(string str) => EnsureLastRegionIsTextRegion().Write(str);
        public void WriteTexture(Texture2D texture)
        {
            bool reAddInputLine = false;

            if (IsLastSectionAcceptingInput())
            {
                (_regions[^1] as TextShellRegion).HasInputLine = false;
                reAddInputLine = true;
            }

            _regions.Add(new ImageShellRegion(this, texture));

            if (reAddInputLine)
            {
                HasInputLine = true;
            }
        }

        public void WriteLine<T>(T item) => EnsureLastRegionIsTextRegion().WriteLine(item);
        public void WriteLine(string str) => EnsureLastRegionIsTextRegion().WriteLine(str);

        public void AppendLine<T>(T item) => EnsureLastRegionIsTextRegion().AppendLine(item);
        public void AppendLine(string str) => EnsureLastRegionIsTextRegion().AppendLine(str);

        //public void Append<T>(T item) => EnsureLastRegionIsText().Append(item);
        //public void Append(string str) => EnsureLastRegionIsText().Append(str);

        public void Input<T>(T item) => EnsureLastRegionIsTextRegion().Input(item);
        public void Input(string str) => EnsureLastRegionIsTextRegion().Input(str);
    }
}
