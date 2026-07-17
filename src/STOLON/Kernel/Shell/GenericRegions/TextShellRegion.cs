using AsitLib.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace STOLON
{
    public readonly struct TextPosition : IEquatable<TextPosition>
    {
        private readonly int _index;
        public readonly int Index
        {
            get
            {
                if (_index < 0)
                {
                    Debug.Assert(!IsOnCharacter);
                    throw new InvalidOperationException("Cannot get Index when cursor is not on a character.");
                }
                else return _index;
            }
        }

        private readonly TextShellRegion _region;
        /// <summary>
        /// Gets the <see cref="TextShellRegion"/> this position is on.
        /// </summary>
        public readonly TextShellRegion Region => _region;

        /// <summary>
        /// Gets if this position is post text, but still on the text. <br/>
        /// <i>(Like when you put the cursor on the end of a word, its still on the word but "after" it.)</i>
        /// </summary>
        public readonly bool IsPostText { get; }

        /// <summary>
        /// Gets if this position is on a character. 
        /// <see langword="false"/> if <see cref="IsPostText"/> is <see langword="true"/> as even though the position is on the text, its not on a character.
        /// </summary>
        public readonly bool IsOnCharacter => _index != -1;

        /// <summary>
        /// Gets the <see cref="Index"/>, or the last valid character position if <see cref="IsPostText"/> is <see langword="true"/>.
        /// </summary>
        public readonly int ClampedIndex => IsPostText ? _region.Text.Length - 1 : Index;

        internal TextPosition(TextShellRegion region, int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, region.Text.Length);

            _region = region;
            _index = index;
            IsPostText = false;
        }

        private TextPosition(TextShellRegion region, int index, bool isPostText)
        {
            _region = region;
            _index = index;
            IsPostText = isPostText;
        }

        /// <summary>
        /// Offsets this <see cref="TextPosition"/> by the specified <paramref name="amount"/>.
        /// </summary>
        /// <param name="amount">The number of characters to move. Positive moves forward, negative moves backward.</param>
        /// <returns>
        /// A new <see cref="TextPosition"/> offset by <paramref name="amount"/>.
        /// If the result would go beyond the end of the text, a post‑text position is returned.
        /// </returns>
        public TextPosition Offset(int amount)
        {
            if (amount == 0) return this;

            if (IsPostText)
            {
                if (amount > 0)
                    return GetPostText(_region);
                else
                    return new TextPosition(_region, _region.Text.Length + amount); // may throw if index < 0
            }
            else
            {
                int newPos = Index + amount;
                if (newPos >= _region.Text.Length)
                    return GetPostText(_region);
                return new TextPosition(_region, newPos);
            }
        }

        /// <summary>
        /// Gets whether this <see cref="TextPosition"/> is valid as a cursor position.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the position is a valid cursor location; otherwise, <see langword="false"/>.
        /// A position is valid when:
        /// <list type="bullet">
        ///   <item>It is on a character whose index is at or after the readonly range start (<see cref="TextShellRegion.ReadonlyRange"/>), or</item>
        ///   <item>It is post‑text (not on a character) and the region has an input line (<see cref="TextShellRegion.HasInputLine"/>).</item>
        /// </list>
        /// </returns>
        public bool IsValidCursorInfo()
        {
            if (IsPostText) return _region.HasInputLine;
            return Index >= _region.ReadonlyRange.End;
        }

        public static TextPosition GetPostText(TextShellRegion region) => new TextPosition(region, -1, true);

        public static NormalizedRange GetRange(TextPosition pos1, TextPosition pos2)
        {
            NormalizedRange temp = NormalizedRange.FromValues(pos1.ClampedIndex, pos2.ClampedIndex);
            if (pos1.IsPostText ^ pos2.IsPostText) return new NormalizedRange(temp.Start, pos1._region.Text.Length);
            return temp;
        }

        public static explicit operator int(TextPosition src) => src.Index;
        public static bool operator ==(TextPosition item1, TextPosition item2) => item1.Equals(item2);
        public static bool operator !=(TextPosition item1, TextPosition item2) => !item1.Equals(item2);

        public override bool Equals(object? obj) => obj is TextPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(HashCode.Combine(_index, IsPostText), _region);
        public bool Equals(TextPosition other) => _index == other._index && IsPostText == other.IsPostText && _region == other._region;
    }

    public class TextShellRegion : ShellRegion
    {
        /// <summary>
        /// Manages the full text as a flat string and a list of lines (without newline characters).
        /// </summary>
        private class TextBuffer
        {
            private readonly StringBuilder _textBuilder;
            private string? _cachedText;
            private List<int>? _lineStarts; // start index of each line (first character of each line)

            public string Text => _cachedText ??= _textBuilder.ToString();
            public int Length => _textBuilder.Length;
            public int LineCount => _lineStarts.Count;

            public TextBuffer(string initialText = "")
            {
                _textBuilder = new StringBuilder(initialText);
                _cachedText = initialText;
                RebuildLineStarts();
            }

            private void RebuildLineStarts()
            {
                _lineStarts = new List<int> { 0 };
                for (int i = 0; i < _textBuilder.Length; i++)
                {
                    if (_textBuilder[i] == '\n')
                        _lineStarts.Add(i + 1);
                }
            }

            private void InvalidateCache()
            {
                _cachedText = null;
            }

            public void InsertText(int index, string text)
            {
                if (string.IsNullOrEmpty(text)) return;
                _textBuilder.Insert(index, text);
                InvalidateCache();
                RebuildLineStarts();
            }

            public void InsertChar(int index, char c)
            {
                _textBuilder.Insert(index, c);
                InvalidateCache();
                RebuildLineStarts();
            }

            public void DeleteText(int index, int length)
            {
                if (length == 0) return;
                if (index + length > _textBuilder.Length)
                    throw new ArgumentOutOfRangeException(nameof(length));
                _textBuilder.Remove(index, length);
                InvalidateCache();
                RebuildLineStarts();
            }

            public (int line, int column) GetLineColumn(int index)
            {
                // binary search to find the largest line start <= index
                int line = _lineStarts.BinarySearch(index);
                if (line < 0)
                {
                    // bitwise complement gives insertion point
                    line = ~line - 1;
                }
                // if index exactly equals a line start, we want that line, so BinarySearch returns that index directly.
                // in case of negative, we got the previous line.
                int column = index - _lineStarts[line];
                return (line, column);
            }

            public int GetIndexFromLineColumn(int line, int column)
            {
                return _lineStarts[line] + column;
            }

            public int GetLineStart(int line) => _lineStarts[line];

            public bool IsLastLineEmpty()
            {
                if (_textBuilder.Length == 0) return true;
                return _textBuilder[^1] == '\n'; // last line is empty if buffer is empty or ends with '\n' (since each '\n' starts a new line after it).
            }
        }

        private readonly IRichLogger _logger;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;
        private readonly CommandManager _commandManager;

        private int _height;
        public override int Height => _height;

        public override int Width => STOLON.VWidth;

        private int _verticalOverlap;
        public override int VerticalOverlap => _verticalOverlap;

        public Font2D Font { get; }

        public string Text => _buffer.Text;
        public NormalizedRange ReadonlyRange { get; private set; }

        private TextPosition? _cursor;
        public TextPosition? Cursor
        {
            get => _cursor;
            set
            {
                if (value is null || value.Value.IsValidCursorInfo()) _cursor = value;
                else if (_hasInputLine && value.Value.IsPostText) throw new InvalidOperationException($"Cursor cannot be placed post text if '{nameof(_hasInputLine)}' is true.");
                else throw new InvalidOperationException($"Cursor cannot be placed at '{value}'.");
            }
        }

        private bool _hasInputLine;
        public bool HasInputLine
        {
            get => _hasInputLine;
            set
            {
                if (value == _hasInputLine) return;

                if (_hasInputLine)
                {
                    // remove the input line (last line)
                    int startOfLastLine = _buffer.GetIndexFromLineColumn(_buffer.LineCount - 1, 0);
                    _buffer.DeleteText(startOfLastLine, _buffer.Length - startOfLastLine);
                    _hasInputLine = false;
                    Cursor = null;
                    ReadonlyRange = new NormalizedRange(0, _buffer.Length);
                    UpdateDisplayMetrics();
                }
                else
                {
                    Debug.Assert(Cursor is null, "Cursor is on text even though there is no input line.");
                    _hasInputLine = true;
                    Input(InputLinePrefix);
                    ExtendReadonlyRange(InputLinePrefix.Length);
                }
            }
        }

        private NormalizedRange _cursorSelection;
        private TextPosition? _lastClickCursor;
        private int _cursorLifetime;
        private Vector2 _textScale;
        private readonly TextBuffer _buffer;

        private const char NewLine = '\n';
        private const string InputLinePrefix = "> ";
        private const bool AllowCursorSelect = false;

        public TextShellRegion(Shell shell, CommandManager commandManager, IRichLogger logger, Font2D font, IInputManager input, ITexture2DCollection textures) : base(shell)
        {
            _logger = logger;
            _input = input;
            _textures = textures;
            _commandManager = commandManager;

            Font = font;
            _textScale = Vector2.One;

            _buffer = new TextBuffer();
            ReadonlyRange = NormalizedRange.Empty;

            STOLON.Instance.Window.TextInput += OnTextInput;
            STOLON.Instance.Window.KeyDown += OnKeyDown;

            _lastClickCursor = null;
            Cursor = null;
            UpdateDisplayMetrics();
        }

        private bool TrySetCursor(TextPosition newPos)
        {
            if (newPos.IsValidCursorInfo())
            {
                Cursor = newPos;
                return true;
            }
            return false;
        }

        private void ExtendReadonlyRange(int amount)
        {
            ReadonlyRange = new NormalizedRange(ReadonlyRange.Start, ReadonlyRange.End + amount);
        }

        #region WRITE_METHODS

        /// <summary>
        /// Writes the specified <paramref name="text"/> to the read‑only history.
        /// If an input line exists, the text is inserted just before it; otherwise it is appended to the end.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            int insertPos;
            if (!_hasInputLine)
            {
                insertPos = _buffer.Length;
            }
            else
            {
                // Insert before the last (input) line
                insertPos = _buffer.GetIndexFromLineColumn(_buffer.LineCount - 1, 0);
            }

            _buffer.InsertText(insertPos, text);
            UpdateDisplayMetrics();

            ExtendReadonlyRange(text.Length);
            if (Cursor.HasValue)
                Cursor = Cursor.Value.Offset(text.Length);
        }

        /// <summary>
        /// Writes the specified <paramref name="text"/> followed by a newline to the read‑only history.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteLine(string text) => Write(text + NewLine);

        /// <summary>
        /// Appends the specified <paramref name="text"/> to the current input line.
        /// This method does <strong>not</strong> extend the readonly range, leaving the text editable.
        /// </summary>
        /// <param name="text">The text to input.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is no input line (<see cref="HasInputLine"/> is <see langword="false"/>).</exception>
        public void Input(string text)
        {
            if (!_hasInputLine)
                throw new InvalidOperationException("No input line available.");

            if (string.IsNullOrEmpty(text)) return;

            _buffer.InsertText(_buffer.Length, text);
            UpdateDisplayMetrics();

            if (Cursor.HasValue)
                Cursor = Cursor.Value.Offset(text.Length);
        }

        #endregion

        public void Command(string command)
        {
            ClearInput();
            Input(command);
            command = GetInputAsProcessing();
            _commandManager.Execute(command);
        }

        public string GetInput()
        {
            if (!_hasInputLine) throw new InvalidOperationException("Cannot get input for textregion without an input line.");
            // compute input text by extracting from readonly end to end
            int start = ReadonlyRange.End;
            int length = _buffer.Length - start;
            return _buffer.Text.Substring(start, length);
        }

        private void ClearInput()
        {
            if (!_hasInputLine) throw new InvalidOperationException("Cannot clear input for textregion without an input line.");
            int deleteStart = ReadonlyRange.End;
            int deleteLength = _buffer.Length - deleteStart;
            if (deleteLength > 0)
                _buffer.DeleteText(deleteStart, deleteLength);
            UpdateDisplayMetrics();
        }

        public string GetInputAsProcessing()
        {
            string input = GetInput();
            ClearInput();
            WriteLine(InputLinePrefix + input);
            return input;
        }

        private void UpdateDisplayMetrics()
        {
            _height = (int)(_buffer.LineCount * Font.Dimensions.Y);
            _verticalOverlap = _buffer.LineCount > 0 && _buffer.IsLastLineEmpty() ? 13 : 0;
            Shell.UpdateRegionPositions(false);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _textScale = Vector2.One;
            _cursorLifetime++;

            #region HANDLE_MOUSE

            TextPosition? mouseTextPos;
            if (_hasInputLine)
                TryGetTextPosition(_input.Mouse.Position, out mouseTextPos, true);
            else
                mouseTextPos = null;

            if (_input.IsMouseOn<Shell>() && _input.IsPressed(MouseButton.Left))
            {
                if (AllowCursorSelect)
                {
                    if (mouseTextPos is not null && _lastClickCursor is not null)
                        _cursorSelection = TextPosition.GetRange(mouseTextPos.Value, _lastClickCursor.Value);
                }
            }

            if (_input.IsMouseOn<Shell>() && _input.IsClicked(MouseButton.Left))
            {
                if (mouseTextPos is null || !TrySetCursor(mouseTextPos.Value))
                {
                    Cursor = null;
                }
                else
                {
                    _lastClickCursor = mouseTextPos;
                    _cursorLifetime = 0;
                }
                if (_cursorSelection.Length > 0) _cursorSelection = NormalizedRange.Empty;
            }

            if (!_input.IsMouseOn<Shell>() && _input.IsClicked(MouseButton.Left))
            {
                Cursor = null;
            }

            #endregion
        }

        private bool TryGetTextPosition(Vector2 screenPos, [NotNullWhen(true)] out TextPosition? position, bool clamp = false)
        {
            int charWidth = (int)Font.Dimensions.X;
            int charHeight = (int)Font.Dimensions.Y;

            int charIndexOnLine = (int)((screenPos.X - Position.X) / charWidth);
            int charLineIndex = (int)((screenPos.Y - Position.Y) / charHeight);
            charLineIndex = (_buffer.LineCount - 1) - charLineIndex; // invert

            if (clamp)
            {
                if (charLineIndex < 0)
                {
                    position = new TextPosition(this, 0);
                    return true;
                }
                charLineIndex = Math.Clamp(charLineIndex, 0, _buffer.LineCount - 1);
            }
            else if (charLineIndex >= _buffer.LineCount || charLineIndex < 0)
            {
                position = null;
                return false;
            }

            // get line start and length without allocating a substring
            int lineStart = _buffer.GetLineStart(charLineIndex);
            int nextLineStart = (charLineIndex + 1 < _buffer.LineCount) ? _buffer.GetLineStart(charLineIndex + 1) : _buffer.Length;
            int lineLength = nextLineStart - lineStart - 1; // exclude newline character

            if (clamp)
            {
                if (charLineIndex == _buffer.LineCount - 1 && charIndexOnLine > lineLength)
                {
                    position = TextPosition.GetPostText(this);
                    return true;
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, lineLength == 0 ? 0 : lineLength - 1);
            }
            else if (charIndexOnLine > lineLength || charIndexOnLine < 0)
            {
                position = null;
                return false;
            }

            int result = lineStart + charIndexOnLine;
            if (result == _buffer.Length)
            {
                position = TextPosition.GetPostText(this);
                return true;
            }

            position = new TextPosition(this, result);
            return true;
        }

        private Vector2 GetCharacterScreenPosAt(int charIndex)
        {
            (int line, int col) = _buffer.GetLineColumn(charIndex);
            float x = Position.X + col * Font.Dimensions.X;
            float y = Position.Y + (_buffer.LineCount - line - 1) * Font.Dimensions.Y;
            return new Vector2(x, y);
        }

        public Vector2 GetCursorScreenPos()
        {
            if (Cursor is null) throw new InvalidOperationException($"Cannot get cursor screen position; cursor is not on text.");

            if (Cursor.Value.IsPostText)
            {
                if (_buffer.Length == 0) return Position;
                char selectedChar = _buffer.Text[^1];
                if (selectedChar == NewLine)
                    return Position + new Vector2(0, -Font.Dimensions.Y + (GetCharacterScreenPosAt(_buffer.Length - 1) - Position).Y);
                else
                    return GetCharacterScreenPosAt(_buffer.Length - 1) + new Vector2(Font.Dimensions.X, 0);
            }
            else
                return GetCharacterScreenPosAt(Cursor.Value.Index);
        }

        #region INPUT_EVENTS

        private void OnKeyDown(object? sender, InputKeyEventArgs e)
        {
            if (Cursor is null) return;
            switch (e.Key)
            {
                case Keys.Left:
                    TrySetCursor(Cursor.Value.Offset(-1));
                    break;
                case Keys.Right:
                    TrySetCursor(Cursor.Value.Offset(1));
                    break;
                default: return;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (Cursor is null) return;

            Shell.ScrollTo(int.MaxValue);

            switch (e.Character)
            {
                case '\b':
                    if (_buffer.Length == 0) break;
                    if (Cursor.Value.IsPostText)
                    {
                        int lastIndex = _buffer.Length - 1;
                        if (lastIndex >= ReadonlyRange.End)
                            _buffer.DeleteText(lastIndex, 1);
                    }
                    else
                    {
                        int newPos = Cursor.Value.Index - 1;
                        if (newPos >= ReadonlyRange.End)
                        {
                            _buffer.DeleteText(newPos, 1);
                            Cursor = new TextPosition(this, newPos);
                        }
                    }
                    break;
                case '\r':
                    string command = GetInputAsProcessing();
                    _commandManager.Execute(command);
                    break;
                default:
                    int insertPos = Cursor.Value.IsPostText ? _buffer.Length : Cursor.Value.Index;
                    _buffer.InsertChar(insertPos, e.Character);
                    if (Cursor.Value.IsPostText)
                        Cursor = TextPosition.GetPostText(this);
                    else
                        Cursor = new TextPosition(this, insertPos + 1);
                    break;
            }
            _cursorSelection = NormalizedRange.Empty;
            UpdateDisplayMetrics();
        }

        #endregion

        public override void Draw(DrawingContext drawingContext)
        {
            string displayText = _buffer.Text;
            if (_cursorSelection.Length > 0)
                displayText = ReplaceAt(displayText, _cursorSelection, '_');
            drawingContext.DrawString(Font, displayText, Position, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0 && Cursor is not null)
                drawingContext.Draw(_textures["UI\\cursor"], GetCursorScreenPos() + new Vector2(0, 2));
        }

        #region TEMP_UTILS

        private static string ReplaceAt(string input, Range range, char replacement)
        {
            (int offset, int length) = range.GetOffsetAndLength(input.Length);
            if (length == 0) return input;
            return string.Create(input.Length, (input, offset, length, replacement), static (span, state) =>
            {
                state.input.AsSpan().CopyTo(span);
                Span<char> slice = span.Slice(state.offset, state.length);
                for (int i = 0; i < slice.Length; i++)
                    if (slice[i] != '\n') slice[i] = state.replacement;
            });
        }

        #endregion
    }
}