using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STOLON
{
    public readonly struct ShellCharacterInfo : IEquatable<ShellCharacterInfo>
    {
        private readonly TextShellRegion _region;

        private readonly int _index;

        public readonly int Index
        {
            get => _index < 0 ? throw new InvalidOperationException() : _index;
        }

        public readonly TextShellRegion Region => _region;

        public readonly bool IsPostText { get; }

        public readonly bool IsOnText => IsPostText || _index != -1;
        public readonly bool IsOnCharacter => _index != -1;
        public readonly int ClampedIndex => IsPostText ? _region.Text.Length - 1 : Index;
        public readonly int BorderingIndex => IsPostText ? _region.Text.Length : Index;

        public ShellCharacterInfo(TextShellRegion region, int index) : this(region, index, false)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _region.Text.Length);
        }

        private ShellCharacterInfo(TextShellRegion region, int index, bool isPostText)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, -1);

            _region = region;
            _index = index;
            IsPostText = isPostText;
        }

        public bool TryOffset(int amount, [NotNullWhen(true)] out ShellCharacterInfo? characterInfo, bool allowPostText = false)
        {
            characterInfo = null;

            if (!allowPostText && IsPostText) return false;

            if (amount == 0)
            {
                characterInfo = this;
                return true;
            }

            if (allowPostText)
            {
                if (IsPostText)
                {
                    if (amount > 0)
                    {
                        characterInfo = ShellCharacterInfo.GetPostText(_region);
                        return true;
                    }
                    else
                    {
                        characterInfo = new ShellCharacterInfo(_region, _region.Text.Length + amount); // amount is negative here.
                        return true;
                    }
                }
                else
                {
                    int newPos = Index + amount;

                    if (newPos >= _region.Text.Length)
                    {
                        characterInfo = ShellCharacterInfo.GetPostText(_region);
                        return true;
                    }
                    characterInfo = new ShellCharacterInfo(_region, newPos);
                    return true;
                }
            }
            else
            {
                characterInfo = new ShellCharacterInfo(_region, Index + amount);
                return true;
            }
        }

        public ShellCharacterInfo Offset(int amount, bool allowPostText = false)
        {
            //if (!allowPostText && IsPostText) throw new InvalidOperationException($"Cannot offset post text pos if '{nameof(allowPostText)}' is false.");

            if (TryOffset(amount, out ShellCharacterInfo? characterInfo, allowPostText)) return characterInfo.Value;
            else throw new InvalidOperationException($"Offset '{this}' by {amount} on text lenght of '{_region.Text.Length}' failed.");
        }

        public bool IsValidCursorInfo()
        {
            if (IsPostText) return _region.HasInputLine;
            if (!IsOnCharacter) throw new InvalidOperationException("Can't check if valid if pos isn't on a character.");

            return !_region.ReadonlyRange.Contains(Index);
        }

        public static ShellCharacterInfo GetPostText(TextShellRegion region) => new ShellCharacterInfo(region, -1, true);
        public static ShellCharacterInfo GetOutOfBounds(TextShellRegion region) => new ShellCharacterInfo(region, -1, false);

        public static NormalizedRange GetRange(ShellCharacterInfo info1, ShellCharacterInfo info2)
        {
            NormalizedRange temp = NormalizedRange.FromValues(info1.ClampedIndex, info2.ClampedIndex);

            if (info1.IsPostText ^ info2.IsPostText) return new NormalizedRange(temp.Start, info1._region.Text.Length);

            return temp;
        }

        public static explicit operator int(ShellCharacterInfo src)
        {
            return src.Index;
        }

        public static bool operator ==(ShellCharacterInfo item1, ShellCharacterInfo item2) => item1.Equals(item2);
        public static bool operator !=(ShellCharacterInfo item1, ShellCharacterInfo item2) => !item1.Equals(item2);

        public override bool Equals(object? obj)
        {
            return obj is ShellCharacterInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(_index, IsPostText), _region);
        }

        public bool Equals(ShellCharacterInfo other)
        {
            return _index == other._index && IsPostText == other.IsPostText && _region == other._region;
        }
    }

    public class TextShellRegion : ShellRegion
    {
        private readonly IRichLogger _logger;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        public bool IsFocus { get; set; }

        public override int Height => _height;
        public override int Width => STOLON.VWidth;

        public override int VerticalOverlap => (int)(_lines.Last().Length == 0 ? 13 : 0);

        public Font2D Font { get; }

        private string _text;
        public string Text => _text;
        private List<string> _lines;
        private Vector2 _textScale;
        private int _height;

        private const char NewLine = '\n';
        private const string InputLinePrefix = "> ";

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

        private ShellCharacterInfo _cursor;
        public ShellCharacterInfo Cursor
        {
            get => _cursor;
            set
            {
                if (!value.IsOnText)
                {
                    Debug.Assert(value == ShellCharacterInfo.GetOutOfBounds(this));
                    _cursor = value;
                }
                else if (value.IsValidCursorInfo()) _cursor = value;
                else if (_hasInputLine && value.IsPostText) throw new InvalidOperationException($"Cursor cannot be placed post text if '{nameof(_hasInputLine)}' is true.");
                else if (!value.IsOnText) throw new InvalidOperationException($"Cursor '{value}' is not on text.");
                else throw new InvalidOperationException($"Cursor cannot be placed at '{value}'.");
            }
        }

        private ShellCharacterInfo _lastClickCursor;
        private int _cursorLifetime; // resets when a new cursor is placed with the mouse.

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

                    Cursor = ShellCharacterInfo.GetOutOfBounds(this);
                    _readonlyRange = new NormalizedRange(0, _text.Length);

                    UpdateText();
                    //_hasInputLine = false;
                }
                else
                {
                    Debug.Assert(!Cursor.IsOnText, "Cursor is on text even though there is no input line.");

                    _hasInputLine = true;

                    Input(InputLinePrefix); // append to prevent _readonlyRange from extending.
                    ExtendReadonlyRange(InputLinePrefix.Length);
                }
            }
        }

        private bool _cursorSelecting;
        private NormalizedRange _cursorSelection;

        private const bool AllowCursorSelect = false;

        public TextShellRegion(Shell shell, IRichLogger logger, Font2D font, IInputManager input, ITexture2DCollection textures) : base(shell)
        {
            _logger = logger;
            _input = input;
            _textures = textures;

            _text = string.Empty;
            _lines = new List<string>();
            Font = font;
            _textScale = Vector2.One;
            ReadonlyRange = NormalizedRange.Empty;

            STOLON.Instance.Window.TextInput += OnTextInput;
            STOLON.Instance.Window.KeyDown += OnKeyDown;

            _lastClickCursor = ShellCharacterInfo.GetOutOfBounds(this);
            Cursor = ShellCharacterInfo.GetOutOfBounds(this);
            ReadonlyRange = NormalizedRange.Empty;

            IsFocus = true;
        }

        private bool TrySetCursor(ShellCharacterInfo newPos)
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

            if (Cursor.IsOnText)
                Cursor = Cursor.Offset(str.Length, true);
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

            if (Cursor.IsOnText)
                Cursor = Cursor.Offset(line.Length, true);
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

        #endregion

        private void Put<T>(T item, ShellCharacterInfo pos) => Put(item.ToString()!, pos);
        private void Put(string str, ShellCharacterInfo pos)
        {
            if (!pos.IsOnText) throw new InvalidOperationException();

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

        public void RemoveAt(int pos)
        {
            _text = _text.Remove(pos, 1);

            UpdateText();
        }

        public void UpdateText()
        {
            _lines = SplitWithNewline(_text).ToList();
            if (_lines.Last().EndsWith(NewLine)) _lines.Add(string.Empty);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _textScale = Vector2.One;
            _cursorLifetime++;
            _height = (int)(_lines.Count * Font.Dimensions.Y);
            //if (_lines.Last().Length == 0) _height -= (int)_font.Dimensions.Y;

            #region HANDLE_MOUSE

            if (_input.IsMouseOn<Shell>() && _input.IsPressed(MouseButton.Left))
            {
                ShellCharacterInfo character = GetCharacterInfoAt(_input.Mouse.Position, true);

                if (!TrySetCursor(character))
                {
                    Cursor = ShellCharacterInfo.GetOutOfBounds(this);
                }
                //Console.WriteLine(GetCharacterIndexAt(_input.Mouse.Position, false));

                if (AllowCursorSelect)
                {
                    _cursorSelecting = true;

                    if (character.IsOnText && _lastClickCursor.IsOnText)
                        _cursorSelection = ShellCharacterInfo.GetRange(character, _lastClickCursor);
                }
            }
            else
            {
                _cursorSelecting = false;
            }

            if (_input.IsMouseOn<Shell>() && _input.IsClicked(MouseButton.Left))
            {
                _lastClickCursor = GetCharacterInfoAt(_input.Mouse.Position, true);
                if (_lastClickCursor.IsValidCursorInfo())
                    _cursorLifetime = 0;

                if (_cursorSelection.Lenght > 0) _cursorSelection = NormalizedRange.Empty;
            }

            if (!_input.IsMouseOn<Shell>() && _input.IsClicked(MouseButton.Left))
            {
                Cursor = ShellCharacterInfo.GetOutOfBounds(this);
            }

            #endregion

            #region HANDLE_ARROWS

            // done in event handler.

            #endregion
        }

        private ShellCharacterInfo GetCharacterInfoAt(Vector2 pos, bool clamp = false)
        {
            int charWidth = (int)Font.Dimensions.X;
            int charHeight = (int)Font.Dimensions.Y;

            int charIndexOnLine = (int)((pos.X - Position.X) / charWidth);

            int charLineIndex = (int)((pos.Y - Position.Y) / charHeight);
            charLineIndex = (_lines.Count - 1) - charLineIndex; // invert it. (text is top down)

            if (clamp)
            {
                if (charLineIndex < 0) // pretext check.
                {
                    return new ShellCharacterInfo(this, 0);
                }
                charLineIndex = Math.Clamp(charLineIndex, 0, _lines.Count - 1); // clamp y
            }
            else if (charLineIndex >= _lines.Count || charLineIndex < 0) return ShellCharacterInfo.GetOutOfBounds(this);

            string charLine = _lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _lines.Count - 1 && charIndexOnLine > charLine.Length) // posttext check.
                {
                    return ShellCharacterInfo.GetPostText(this);
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, charLine.Length == 0 ? 0 : (charLine.Length - 1)); // clamp x, ?: because of empty lines, remove the -1 and when selecting lines, the cursor will be placed after the newline.
            }
            else if (charIndexOnLine > charLine.Length || charIndexOnLine < 0) return ShellCharacterInfo.GetOutOfBounds(this);

            int result = charIndexOnLine;

            for (int lineIndex = 0; lineIndex < charLineIndex; lineIndex++)
                result += _lines[lineIndex].Length; // newline is already in line.

            if (result == _text.Length) return ShellCharacterInfo.GetPostText(this);

            //Console.WriteLine($"{charLineIndex}:{charIndexOnLine} = {result}");
            //Console.WriteLine($"out of {_text.Length}");
            //Console.WriteLine($"char {_text[result]}");

            return new ShellCharacterInfo(this, result);
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
            if (!Cursor.IsOnText) throw new InvalidOperationException($"Cursor is not on text.");

            if (Cursor.IsPostText)
            {
                if (_text.Length == 0) return Position; //  + new Vector2(0, -_font.Dimensions.Y);

                char selectedChar = _text[_text.Length - 1];
                if (selectedChar == NewLine) return Position + new Vector2(0, -Font.Dimensions.Y + (GetCharacterScreenPosAt(_text.Length - 1) - Position).Y);
                else return GetCharacterScreenPosAt(_text.Length - 1) + new Vector2(Font.Dimensions.X, 0);
            }
            else return GetCharacterScreenPosAt(Cursor.Index);
        }

        #region INPUT_EVENTS

        private void OnKeyDown(object? sender, InputKeyEventArgs e)
        {
            if (!Cursor.IsOnText) return;
            switch (e.Key)
            {
                case Keys.Left:
                    TrySetCursor(Cursor.Offset(-1, true));
                    break;
                case Keys.Right:
                    TrySetCursor(Cursor.Offset(1, true));
                    break;
                default: return;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (!Cursor.IsOnText) return;

            void PutAndOffset(char c)
            {
                Put(c, Cursor);
                if (!Cursor.IsPostText) Cursor = Cursor.Offset(1, true);
            }

            switch (e.Character)
            {
                case '\b':
                    if (_text.Length == 0) break;

                    if (Cursor.IsPostText)
                    {
                        if (!ReadonlyRange.Contains(_text.Length - 1)) RemoveAt(_text.Length - 1);
                    }
                    else
                    {
                        int newPos = Cursor.Index - 1;

                        if (!ReadonlyRange.Contains(newPos))
                        {
                            RemoveAt(newPos);
                            Cursor = new ShellCharacterInfo(this, newPos);
                        }
                    }

                    break;
                case '\r':
                    //PutAndOffset(NewLine);

                    //WriteLine(GetInput());
                    //ClearInput();
                    return;
                case '\t':
                    string input = GetInput().Split(" ").Last();

                    //Console.WriteLine(Autocomplete.Complete(input, Shell.Words).Options.ToJoinedString(", "));
                    Input(Autocomplete.Complete(input, Shell.Words).BestOption?[input.Length..] ?? string.Empty);
                    return;
                default:
                    PutAndOffset(e.Character);
                    break;
            }

            _cursorSelection = NormalizedRange.Empty;
        }

        #endregion

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawString(Font, ReplaceAt(_text, _cursorSelection, '_'), Position, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0 && Cursor.IsOnText)
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

        static string[] SplitWithNewline(string input)
        {
            return Regex.Split(input, @"(?<=\n)(?=\S)|(?<=\n)(?=\n)");
        }

        public static string InsertLine(string input, int lineNumber, string lineToInsert, char newLine)
        {
            string[] lines = input.Split(newLine, StringSplitOptions.RemoveEmptyEntries);

            if (lineNumber < 1 || lineNumber > lines.Length + 1)
                throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number is out of range.");

            string result = string.Join(newLine,
                new string[] { string.Join(newLine, lines.Take(lineNumber - 1)) }
                .Concat(new[] { lineToInsert })
                .Concat(lines.Skip(lineNumber - 1)));

            return result;
        }

        #endregion
    }
}
