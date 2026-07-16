using AsitLib.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace STOLON
{
    public readonly struct TextPosition : IEquatable<TextPosition>
    {
        private readonly TextShellRegion _region;

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
        /// Attempts to offset this <see cref="TextPosition"/> by a specified <paramref name="amount"/>.
        /// </summary>
        /// <param name="amount">The amount of characters to offset this <see cref="TextPosition"/>.</param>
        /// <param name="position">When this method returns, contains the offset <see cref="TextPosition"/>.</param>
        public readonly bool TryOffset(int amount, [NotNullWhen(true)] out TextPosition? position)
        {
            if (amount == 0)
            {
                position = this;
                return true;
            }

            if (IsPostText)
            {
                if (amount > 0)
                {
                    position = TextPosition.GetPostText(_region);
                    return true;
                }
                else
                {
                    position = new TextPosition(_region, _region.Text.Length + amount); // amount is negative here.
                    return true;
                }
            }
            else
            {
                int newPos = Index + amount;

                if (newPos >= _region.Text.Length)
                {
                    position = TextPosition.GetPostText(_region);
                    return true;
                }
                position = new TextPosition(_region, newPos);
                return true;
            }
        }

        /// <summary>
        /// Offset this <see cref="TextPosition"/> by a specified <paramref name="amount"/>.
        /// </summary>
        /// <param name="amount">The amount of characters to offset this <see cref="TextPosition"/>.</param>
        /// <returns>This <see cref="TextPosition"/> offset by a specified <paramref name="amount"/>.</returns>
        public TextPosition Offset(int amount)
        {
            if (TryOffset(amount, out TextPosition? characterInfo)) return characterInfo.Value;
            else throw new InvalidOperationException($"Offset '{this}' by {amount} on text length of '{_region.Text.Length}' failed.");
        }

        /// <summary>
        /// Gets if this <see cref="TextPosition"/> is valid as cursor position. 
        /// Returns <see langword="false"/> if <see cref="IsOnCharacter"/> is <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if this <see cref="TextPosition"/> is valid as cursor position, otherwise <see langword="false"/>.</returns>
        public bool IsValidCursorInfo()
        {
            if (IsPostText) return _region.HasInputLine;

            return !_region.ReadonlyRange.Contains(Index);
        }

        /// <summary>
        /// Create a new post text <see cref="TextPosition"/>.
        /// </summary>
        public static TextPosition GetPostText(TextShellRegion region) => new TextPosition(region, -1, true);

        public static NormalizedRange GetRange(TextPosition pos1, TextPosition pos2)
        {
            NormalizedRange temp = NormalizedRange.FromValues(pos1.ClampedIndex, pos2.ClampedIndex);

            if (pos1.IsPostText ^ pos2.IsPostText) return new NormalizedRange(temp.Start, pos1._region.Text.Length);

            return temp;
        }

        public static explicit operator int(TextPosition src)
        {
            return src.Index;
        }

        public static bool operator ==(TextPosition item1, TextPosition item2) => item1.Equals(item2);
        public static bool operator !=(TextPosition item1, TextPosition item2) => !item1.Equals(item2);

        public override bool Equals(object? obj)
        {
            return obj is TextPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(_index, IsPostText), _region);
        }

        public bool Equals(TextPosition other)
        {
            return _index == other._index && IsPostText == other.IsPostText && _region == other._region;
        }
    }

    public class TextShellRegion : ShellRegion
    {
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

        private string _text;
        public string Text => _text;

        private NormalizedRange _readonlyRange;
        public NormalizedRange ReadonlyRange
        {
            get
            {
                return _readonlyRange;
            }
            set
            {
                ValidateTextRange(value);

                _readonlyRange = value;
            }
        }

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
                    _lines = _lines[..^1];
                    _text = _lines.ToJoinedString();

                    _hasInputLine = false;

                    Cursor = null;
                    _readonlyRange = new NormalizedRange(0, _text.Length);

                    UpdateText();
                }
                else
                {
                    Debug.Assert(Cursor is null, "Cursor is on text even though there is no input line.");

                    _hasInputLine = true;

                    Input(InputLinePrefix); // append to prevent _readonlyRange from extending.
                    ExtendReadonlyRange(InputLinePrefix.Length);
                }
            }
        }

        private NormalizedRange _cursorSelection;
        private TextPosition? _lastClickCursor;
        private int _cursorLifetime; // resets when a new cursor is placed with the mouse.
        private List<string> _lines;
        private Vector2 _textScale;

        private const char NewLine = '\n';
        private const string InputLinePrefix = "> ";
        private const bool AllowCursorSelect = false;

        public TextShellRegion(Shell shell, CommandManager commandManager, IRichLogger logger, Font2D font, IInputManager input, ITexture2DCollection textures) : base(shell)
        {
            _logger = logger;
            _input = input;
            _textures = textures;
            _commandManager = commandManager;

            _text = string.Empty;
            _lines = new List<string>();
            Font = font;
            _textScale = Vector2.One;
            ReadonlyRange = NormalizedRange.Empty;

            STOLON.Instance.Window.TextInput += OnTextInput;
            STOLON.Instance.Window.KeyDown += OnKeyDown;

            _lastClickCursor = null;
            Cursor = null;
            ReadonlyRange = NormalizedRange.Empty;
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

        private void ValidateTextRange(NormalizedRange range)
        {
            if (range.IsNegative) throw new InvalidOperationException("negative range");
            if (range.End > _text.Length) throw new InvalidOperationException("maxend");
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

            string line = _lines[^2];

            if (!line.EndsWith(NewLine)) throw new InvalidObjectException("Invalid line found (missing newline).");

            _lines[^2] = // second last
                _lines[^2][..^1] + // remove newline.
                str + // add str.
                NewLine; // re-add newline.

            _text = _lines.ToJoinedString();

            UpdateText();

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

            string line = str + NewLine;

            _lines.Insert(_lines.Count - 1, line);

            _text = _lines.ToJoinedString();

            UpdateText();

            if (Cursor is not null)
                Cursor = Cursor.Value.Offset(line.Length);
            ExtendReadonlyRange(line.Length);
        }


        public void AppendLine<T>(T item) => AppendLine(item.ToString()!);
        public void AppendLine(string str)
        {
            Append(str + NewLine);
        }

        public void Append<T>(T item) => Append(item.ToString()!);
        public void Append(string str)
        {
            if (str.Contains('\r')) throw new ArgumentException("Cannot write invalid newline. ('\\r'.)", nameof(str)); // newline is \n char

            Put(str, _text.Length);
        }

        public void Input<T>(T item) => Input(item.ToString()!);
        public void Input(string str)
        {
            if (!_hasInputLine) throw new InvalidOperationException("No input is allowed at this time.");

            if (str.Contains('\r')) throw new ArgumentException("Cannot write invalid newline. ('\\r'.)", nameof(str));

            Put(str, _text.Length);
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
            if (pos.IsPostText) Put(str, _text.Length);
            else Put(str, pos.ClampedIndex);
        }

        public void Put<T>(T item, int pos) => Put(item.ToString()!, pos);
        public void Put(string str, int pos)
        {
            _text = _text.Insert(pos, str);

            UpdateText();
        }

        public string GetInput()
        {
            if (!_hasInputLine) throw new InvalidOperationException("Cannot get input for textregion without an input line.");

            string inputLine = _lines[^1];

            return inputLine[InputLinePrefix.Length..];
        }

        private void ClearInput()
        {
            if (!_hasInputLine) throw new InvalidOperationException("Cannot clear input for textregion without an input line.");

            _lines[^1] = InputLinePrefix;

            _text = _lines.ToJoinedString();

            UpdateText();
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
            _text = _text.Remove(pos, 1);

            UpdateText();
        }

        public void UpdateText()
        {
            _lines = SplitWithNewline(_text).ToList();
            if (_lines.Last().EndsWith(NewLine)) _lines.Add(string.Empty);

            _verticalOverlap = (int)(_lines.Last().Length == 0 ? 13 : 0);

            Shell.UpdateRegionPositions(false);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _textScale = Vector2.One;
            _cursorLifetime++;
            _height = (int)(_lines.Count * Font.Dimensions.Y);
            //if (_lines.Last().Length == 0) _height -= (int)_font.Dimensions.Y;

            #region HANDLE_MOUSE

            TextPosition? mouseTextPos;

            // TryGetTextPosition may throw an error when there is no text and it tries create a TextPosition instance with pos 0.
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

            #region HANDLE_ARROWS

            // done in event handler.

            #endregion
        }

        private bool TryGetTextPosition(Vector2 screenPos, [NotNullWhen(true)] out TextPosition? position, bool clamp = false)
        {
            int charWidth = (int)Font.Dimensions.X;
            int charHeight = (int)Font.Dimensions.Y;

            int charIndexOnLine = (int)((screenPos.X - Position.X) / charWidth);

            int charLineIndex = (int)((screenPos.Y - Position.Y) / charHeight);
            charLineIndex = (_lines.Count - 1) - charLineIndex; // invert it. (text is top down)

            if (clamp)
            {
                if (charLineIndex < 0) // pretext check.
                {
                    position = new TextPosition(this, 0);
                    return true;
                }
                charLineIndex = Math.Clamp(charLineIndex, 0, _lines.Count - 1); // clamp y
            }
            else if (charLineIndex >= _lines.Count || charLineIndex < 0)
            {
                position = null;
                return false;
            }

            string charLine = _lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _lines.Count - 1 && charIndexOnLine > charLine.Length) // posttext check.
                {
                    position = TextPosition.GetPostText(this);
                    return true;
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, charLine.Length == 0 ? 0 : (charLine.Length - 1)); // clamp x, ?: because of empty lines, remove the -1 and when selecting lines, the cursor will be placed after the newline.
            }
            else if (charIndexOnLine > charLine.Length || charIndexOnLine < 0)
            {
                position = null;
                return false;
            }

            int result = charIndexOnLine;

            for (int lineIndex = 0; lineIndex < charLineIndex; lineIndex++)
                result += _lines[lineIndex].Length; // newline is already in line.

            if (result == _text.Length)
            {
                position = TextPosition.GetPostText(this);
                return true;
            }

            //Console.WriteLine($"{charLineIndex}:{charIndexOnLine} = {result}");
            //Console.WriteLine($"out of {_text.Length}");
            //Console.WriteLine($"char {_text[result]}");

            position = new TextPosition(this, result);
            return true;
        }

        private Vector2 GetCharacterScreenPosAt(int charIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(charIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(charIndex, _text.Length);

            int charLine = 0;
            int charIndexOnLine = 0;
            int seenChars = 0;

            for (int lineIndex = 0; lineIndex < _lines.Count; lineIndex++)
            {
                int lineLength = _lines[lineIndex].Length;

                if (charIndex < seenChars + lineLength)
                {
                    charLine = lineIndex;
                    charIndexOnLine = charIndex - seenChars;
                    break;
                }

                seenChars += lineLength;
            }

            float x = Position.X + charIndexOnLine * Font.Dimensions.X;
            float y = Position.Y + (_lines.Count - charLine - 1) * Font.Dimensions.Y;

            return new Vector2(x, y);
        }

        public Vector2 GetCursorScreenPos()
        {
            if (Cursor is null) throw new InvalidOperationException($"Cannot get cursor screen position; cursor is not on text.");

            if (Cursor.Value.IsPostText)
            {
                if (_text.Length == 0) return Position; //  + new Vector2(0, -_font.Dimensions.Y);

                char selectedChar = _text[_text.Length - 1];
                if (selectedChar == NewLine) return Position + new Vector2(0, -Font.Dimensions.Y + (GetCharacterScreenPosAt(_text.Length - 1) - Position).Y);
                else return GetCharacterScreenPosAt(_text.Length - 1) + new Vector2(Font.Dimensions.X, 0);
            }
            else return GetCharacterScreenPosAt(Cursor.Value.Index);
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
                    if (_text.Length == 0) break;

                    if (Cursor.Value.IsPostText)
                    {
                        if (!ReadonlyRange.Contains(_text.Length - 1)) RemoveAt(_text.Length - 1);
                    }
                    else
                    {
                        int newPos = Cursor.Value.Index - 1;

                        if (!ReadonlyRange.Contains(newPos))
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
            drawingContext.DrawString(Font, ReplaceAt(_text, _cursorSelection, '_'), Position, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0 && Cursor is not null)
                drawingContext.Draw(_textures["UI\\cursor"], GetCursorScreenPos() + new Vector2(0, 2));
            //drawingContext.DrawLine(_input.Mouse.Position, _input.Mouse.Position);
        }

        #region TEMP_UTILS

        private static string ReplaceAt(string input, int index, char newChar)
        {
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        private static string ReplaceAt(string input, Range range, char replacement)
        {
            (int offset, int length) = range.GetOffsetAndLength(input.Length);
            Span<char> span = input.ToCharArray().AsSpan();

            for (int i = offset; i < offset + length; i++)
            {
                if (span[i] != NewLine)
                {
                    span[i] = replacement;
                }
            }

            return new string(span);
        }

        private static string[] SplitWithNewline(string input)
        {
            return Regex.Split(input, @"(?<=\n)(?=\S)|(?<=\n)(?=\n)");
        }

        private static string InsertLine(string input, int lineNumber, string lineToInsert, char newLine)
        {
            string[] lines = input.Split(newLine, StringSplitOptions.RemoveEmptyEntries);

            if (lineNumber < 1 || lineNumber > lines.Length + 1)
                throw new ArgumentOutOfRangeException(nameof(lineNumber), "'lineNumber' is out of range.");

            string result = string.Join(newLine,
                new string[] { string.Join(newLine, lines.Take(lineNumber - 1)) }
                .Concat(new[] { lineToInsert })
                .Concat(lines.Skip(lineNumber - 1)));

            return result;
        }

        #endregion
    }
}
