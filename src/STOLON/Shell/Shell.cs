using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class Shell : IComponent
    {
        private readonly IRichLogger _logger;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;
        private readonly Kernel _kernel;

        private readonly List<ShellRegion> _regions;
        private readonly Vector2 _origin;

        internal IReadOnlyList<ShellRegion> Regions => _regions;

        public static string[] Words { get; } = ["write", "read", "region", "shell", "stolon"]; // temp for autocomplete tests.

        private string[]? _autocompletions;
        private int _selectedAutocompletion;
        private Rectangle _autocompletionRect;
        private Line[]? _autocompletionDividerLines;
        private string[]? _displayedAutocompletions;

        private ShellCharacterInfo? _cursor; // null when out of bounds of any region.

        public bool HasInputLine
        {
            get => EnsureLastRegionIsTextRegion().HasInputLine;
            set => EnsureLastRegionIsTextRegion().HasInputLine = value;
        }

        public Font2D Font { get; }

        public const int RegionClearance = 5;
        public const int CursorHeight = 8; // size of cursor texture, cursor in texture is one pixel shorter.

        public Shell(IRichLogger logger, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Kernel kernel)
        {
            _logger = logger;
            _fonts = fonts;
            _input = input;
            _textures = textures;
            _kernel = kernel;

            _regions = new List<ShellRegion>();

            _origin = new Vector2(10, 0);

            _autocompletions = null;

            Font = fonts.Medium;

            STOLON.Instance.Window.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, InputKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Up:
                    if (_selectedAutocompletion != 0)
                        _selectedAutocompletion--;
                    break;
                case Keys.Down:
                    if (_selectedAutocompletion != _autocompletions.Length - 1)
                        _selectedAutocompletion++;
                    break;
                case Keys.Tab:
                    if (_autocompletions is not null)
                    {
                        AutoComplete(_selectedAutocompletion);
                    }
                    break;
                default: return;
            }

            if (_autocompletions is null || _autocompletions.Length == 0) _selectedAutocompletion = -1;
            else
                _selectedAutocompletion = Math.Clamp(_selectedAutocompletion, 0, _autocompletions.Length - 1);
        }

        private TextShellRegion EnsureLastRegionIsTextRegion() => EnsureLastRegionIs<TextShellRegion>(() => new TextShellRegion(this, _logger, Font, _input, _textures));

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
            float y = STOLON.VHeight - RegionClearance;

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

        private bool TryGetCursor([NotNullWhen(true)] out ShellCharacterInfo? cursor)
        {
            ShellCharacterInfo? result = null;

#if DEBUG

            foreach (ShellRegion region in _regions)
            {
                if (region is TextShellRegion textRegion)
                {
                    if (textRegion.Cursor.IsOnText)
                    {
                        Debug.Assert(!result.HasValue, "Cursor cannot be on multiple textregion's at once.");

                        result = textRegion.Cursor;
                    }
                }
            }

#else

            foreach (ShellRegion region in _regions)
            {
                if (region is TextShellRegion textRegion)
                {
                    if (textRegion.Cursor.IsOnText)
                    {
                        result = textRegion.Cursor;
                        break;
                    }
                }
            }

#endif

            if (result.HasValue)
            {
                cursor = result.Value;
                return true;
            }

            cursor = null;
            return false;
        }

        //internal ShellRegion GetRegionUnderMouse()
        //{

        //}

        private string GetAutoCompleteTarget()
        {
            Debug.Assert(_cursor is not null, $"{nameof(_cursor)} is null while {nameof(GetAutoCompleteTarget)} got called.");

            return _cursor.Value.Region.GetInput().Split(' ').Last();
        }

        private void AutoComplete(int index)
        {
            Debug.Assert(_autocompletions is not null, $"{nameof(_autocompletions)} is null while {nameof(AutoComplete)} got called.");

            string target = GetAutoCompleteTarget();

            ((TextShellRegion)_regions.Last()).Input(_autocompletions[index][target.Length..]);
        }

        public void Update(int elapsedMilliseconds)
        {
            foreach (ShellRegion region in _regions)
            {
                region.Update(elapsedMilliseconds);
            }

            if (_autocompletions is not null)
            {
                if (_input.IsMouseOn(this) && _input.IsClicked(MouseButton.Left) && _autocompletionRect.Contains(_input.Mouse.Position))
                {
                    for (int i = 0; i < _autocompletionDividerLines.Length; i++)
                    {
                        Line line = _autocompletionDividerLines[i];

                        if (_input.Mouse.Position.Y >= line.Start.Y)
                        {
                            AutoComplete(i);

                            break;
                        }
                    }
                }
            }

            if (TryGetCursor(out _cursor))
            {
                _autocompletions = Autocomplete.Complete(GetAutoCompleteTarget(), Words).Options.Take(3).ToArray();

                if (_autocompletions.Length == 0) _selectedAutocompletion = -1;
                else _selectedAutocompletion = Math.Clamp(_selectedAutocompletion, 0, _autocompletions.Length - 1);

                if (_autocompletions.Length > 0)
                {
                    int visibleOptions = Math.Min(3, _autocompletions.Length);
                    int entryHeight = (int)(Font.Dimensions.Y + 2);
                    int rectHeight = visibleOptions * entryHeight;
                    int rectWidth = (int)(10 * (Font.Dimensions.X + 4) + 4);

                    Point rectPos = (_cursor.Value.Region.GetCursorScreenPos() + new Vector2(0, Shell.CursorHeight + 2)).ToPoint();

                    _autocompletionRect = new Rectangle(rectPos, new Point(rectWidth, rectHeight));

                    _autocompletionDividerLines = new Line[visibleOptions];
                    for (int i = 0; i < visibleOptions; i++)
                    {
                        _autocompletionDividerLines[i] = Line.CreateHorizontal(rectPos.X, rectPos.X + rectWidth, (rectPos.Y + entryHeight * i) + 1);
                    }

                    _autocompletionDividerLines = _autocompletionDividerLines.Reverse().ToArray();

                    _displayedAutocompletions = new string[visibleOptions];
                    for (int i = 0; i < visibleOptions; i++)
                    {
                        _displayedAutocompletions[i] = _selectedAutocompletion == i ? "> " + _autocompletions[i] : _autocompletions[i];
                    }
                }
            }
            else
            {
                _autocompletions = null;
            }
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.RegisterDraw(this, STOLON.Bounds);

            foreach (ShellRegion region in _regions)
            {
                region.Draw(drawingContext);
            }

            if (_autocompletions is not null && _autocompletions.Length != 0)
            {
                drawingContext.DrawArea(_autocompletionRect, Color.Black);
                drawingContext.DrawRectangle(_autocompletionRect, thickness: 1);

                for (int i = 0; i < _autocompletionDividerLines!.Length; i++)
                {
                    drawingContext.DrawLine(_autocompletionDividerLines![i], thickness: 1);
                    drawingContext.DrawString(Font, _displayedAutocompletions![i], _autocompletionDividerLines![i].Start.ToVector2() + new Vector2(5, 0));
                }
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

            _regions.Add(new WindowShellRegion(this, _kernel, _textures, _fonts, _input, new ImageWindow(_kernel, _textures, _fonts, _input, texture)
            {
                IsDrawnByKernel = false,
            }));

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
