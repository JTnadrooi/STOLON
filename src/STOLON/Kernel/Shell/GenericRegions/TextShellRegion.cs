using AsitLib.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
            private string _text;
            private List<string> _lines;

            public string Text => _text;
            public IReadOnlyList<string> Lines => _lines;
            public int Length => _text.Length;
            public int LineCount => _lines.Count;

            public TextBuffer(string initialText = "")
            {
                _text = initialText;
                _lines = _text.Split('\n').ToList();
            }

            private void RebuildLines() => _lines = _text.Split('\n').ToList();

            public void InsertText(int index, string text)
            {
                _text = _text.Insert(index, text);
                RebuildLines();
            }

            public void DeleteText(int index, int length)
            {
                if (index + length > _text.Length)
                    throw new ArgumentOutOfRangeException(nameof(length));
                _text = _text.Remove(index, length);
                RebuildLines();
            }

            public (int line, int column) GetLineColumn(int index)
            {
                int current = 0;
                for (int i = 0; i < _lines.Count; i++)
                {
                    int lineLen = _lines[i].Length;
                    if (index <= current + lineLen)
                        return (i, index - current);
                    current += lineLen + 1; // +1 for the newline
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            public int GetIndexFromLineColumn(int line, int column)
            {
                int index = 0;
                for (int i = 0; i < line; i++)
                    index += _lines[i].Length + 1;
                index += column;
                return index;
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

        public void Write<T>(T item) => Write(item.ToString()!);
        public void Write(string str)
        {
            if (!_hasInputLine)
            {
                Append(str);
                ExtendReadonlyRange(str.Length);
                return;
            }

            int inputLineIndex = _buffer.LineCount - 1;
            if (inputLineIndex == 0)
            {
                // no history line, insert a new line at the beginning
                _buffer.InsertText(0, str + NewLine);
            }
            else
            {
                // append to the line before the input line
                int targetLineIndex = inputLineIndex - 1;
                int insertPos = _buffer.GetIndexFromLineColumn(targetLineIndex, _buffer.Lines[targetLineIndex].Length);
                _buffer.InsertText(insertPos, str);
            }
            UpdateDisplayMetrics();
            if (Cursor is not null)
                Cursor = Cursor.Value.Offset(str.Length);
            ExtendReadonlyRange(str.Length);
        }

        public void WriteLine<T>(T item) => WriteLine(item.ToString()!);
        public void WriteLine(string str)
        {
            if (!_hasInputLine)
            {
                AppendLine(str);
                ExtendReadonlyRange(str.Length + 1);
                return;
            }

            int inputLineIndex = _buffer.LineCount - 1;
            int insertPos = _buffer.GetIndexFromLineColumn(inputLineIndex, 0); // start of input line
            _buffer.InsertText(insertPos, str + NewLine);
            UpdateDisplayMetrics();
            if (Cursor is not null)
                Cursor = Cursor.Value.Offset(str.Length + 1);
            ExtendReadonlyRange(str.Length + 1);
        }

        public void AppendLine<T>(T item) => AppendLine(item.ToString()!);
        public void AppendLine(string str) => Append(str + NewLine);

        public void Append<T>(T item) => Append(item.ToString()!);
        public void Append(string str)
        {
            if (str.Contains('\r')) throw new ArgumentException("Cannot write invalid newline. ('\\r'.)", nameof(str));
            Put(str, _buffer.Length);
        }

        public void Input<T>(T item) => Input(item.ToString()!);
        public void Input(string str)
        {
            if (!_hasInputLine) throw new InvalidOperationException("No input is allowed at this time.");
            if (str.Contains('\r')) throw new ArgumentException("Cannot write invalid newline. ('\\r'.)", nameof(str));
            Put(str, _buffer.Length);
        }

        public void SimulateUserCommand(string command)
        {
            ClearInput();
            Input(command);
            command = GetInputAsProcessing();
            _commandManager.Execute(command);
        }

        #endregion

        private void Put<T>(T item, TextPosition pos) => Put(item.ToString()!, pos);
        private void Put(string str, TextPosition pos)
        {
            if (pos.IsPostText) Put(str, _buffer.Length);
            else Put(str, pos.ClampedIndex);
        }

        public void Put<T>(T item, int pos) => Put(item.ToString()!, pos);
        public void Put(string str, int pos)
        {
            _buffer.InsertText(pos, str);
            UpdateDisplayMetrics();
        }

        public string GetInput()
        {
            if (!_hasInputLine) throw new InvalidOperationException("Cannot get input for textregion without an input line.");
            string lastLine = _buffer.Lines[^1];
            return lastLine[InputLinePrefix.Length..];
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

        public void RemoveAt(int pos)
        {
            _buffer.DeleteText(pos, 1);
            UpdateDisplayMetrics();
        }

        private void UpdateDisplayMetrics()
        {
            _height = (int)(_buffer.LineCount * Font.Dimensions.Y);
            _verticalOverlap = (int)(_buffer.Lines.Count > 0 && _buffer.Lines[^1].Length == 0 ? 13 : 0);
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

            string line = _buffer.Lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _buffer.LineCount - 1 && charIndexOnLine > line.Length)
                {
                    position = TextPosition.GetPostText(this);
                    return true;
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, line.Length == 0 ? 0 : line.Length - 1);
            }
            else if (charIndexOnLine > line.Length || charIndexOnLine < 0)
            {
                position = null;
                return false;
            }

            int result = _buffer.GetIndexFromLineColumn(charLineIndex, charIndexOnLine);
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

            void PutAndOffsetCursor(char c)
            {
                Put(c, Cursor.Value);
                if (!Cursor.Value.IsPostText) Cursor = Cursor.Value.Offset(1);
            }

            Shell.ScrollTo(int.MaxValue);

            switch (e.Character)
            {
                case '\b':
                    if (_buffer.Length == 0) break;
                    if (Cursor.Value.IsPostText)
                    {
                        int lastIndex = _buffer.Length - 1;
                        if (lastIndex >= ReadonlyRange.End)
                            RemoveAt(lastIndex);
                    }
                    else
                    {
                        int newPos = Cursor.Value.Index - 1;
                        if (newPos >= ReadonlyRange.End)
                        {
                            RemoveAt(newPos);
                            Cursor = new TextPosition(this, newPos);
                        }
                    }
                    break;
                case '\r':
                    string command = GetInputAsProcessing();
                    _commandManager.Execute(command);
                    break;
                default:
                    PutAndOffsetCursor(e.Character);
                    break;
            }
            _cursorSelection = NormalizedRange.Empty;
        }

        #endregion

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawString(Font, ReplaceAt(_buffer.Text, _cursorSelection, '_'), Position, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0 && Cursor is not null)
                drawingContext.Draw(_textures["UI\\cursor"], GetCursorScreenPos() + new Vector2(0, 2));
        }

        #region TEMP_UTILS

        private static string ReplaceAt(string input, Range range, char replacement)
        {
            (int offset, int length) = range.GetOffsetAndLength(input.Length);
            Span<char> span = input.ToCharArray().AsSpan();
            for (int i = offset; i < offset + length; i++)
                if (span[i] != NewLine) span[i] = replacement;
            return new string(span);
        }

        #endregion
    }
}