using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace STOLON
{
    public abstract class ShellRegion
    {
        public abstract int Height { get; } // no buildin clearance svp

        protected Shell Shell { get; }

        protected Vector2 Pos => Shell.GetRegionPos(this);

        public virtual int VerticalOverlap => 0;

        public ShellRegion(Shell shell)
        {
            Shell = shell;
        }

        public virtual void Update(int elapsedMilliseconds) { }
        public virtual void Draw(DrawingContext drawingContext) { }
    }

    public class ImageShellRegion : ShellRegion
    {
        public override int Height => _texture.Height;

        private readonly Texture2D _texture;

        public ImageShellRegion(Shell shell, Texture2D texture) : base(shell)
        {
            _texture = texture;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(_texture, Pos);
        }
    }

    public class TextShellRegion : ShellRegion
    {
        private readonly struct CharacterInfo : IEquatable<CharacterInfo>
        {
            private readonly TextShellRegion _region;

            private readonly int _index;

            public readonly int Index
            {
                get => _index < 0 ? throw new InvalidOperationException() : _index;
            }

            public readonly bool IsPostText { get; }

            public readonly bool IsOnText => IsPostText || _index != -1;
            public readonly bool IsOnCharacter => _index != -1;
            public readonly int ClampedIndex => IsPostText ? _region._text.Length - 1 : Index;
            public readonly int BorderingIndex => IsPostText ? _region._text.Length : Index;

            public CharacterInfo(TextShellRegion region, int index) : this(region, index, false)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _region._text.Length);
            }

            private CharacterInfo(TextShellRegion region, int index, bool isPostText)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, -1);

                _region = region;
                _index = index;
                IsPostText = isPostText;
            }

            public CharacterInfo Offset(int amount, bool allowPostText = false)
            {
                if (!allowPostText && IsPostText) throw new InvalidOperationException($"Cannot offset post text pos if '{nameof(allowPostText)}' is false.");
                if (amount == 0) return this;

                if (allowPostText)
                {
                    if (IsPostText)
                    {
                        if (amount > 0)
                        {
                            return CharacterInfo.GetPostText(_region);
                        }
                        else
                        {
                            return new CharacterInfo(_region, _region._text.Length + amount); // amount is negative here.
                        }
                    }
                    else
                    {
                        int newPos = Index + amount;

                        if (newPos >= _region._text.Length)
                        {
                            return CharacterInfo.GetPostText(_region);
                        }
                        else return new CharacterInfo(_region, newPos);
                    }
                }
                else
                {
                    return new CharacterInfo(_region, Index + amount);
                }
            }

            public bool IsValidCursorInfo()
            {
                if (IsPostText) return _region._hasInputLine;
                if (!IsOnCharacter) throw new InvalidOperationException("Can't check if valid if pos isn't on a character.");

                return !_region.ReadonlyRange.Contains(Index);
            }

            public static CharacterInfo GetPostText(TextShellRegion region) => new CharacterInfo(region, -1, true);
            public static CharacterInfo GetOutOfBounds(TextShellRegion region) => new CharacterInfo(region, -1, false);

            public static NormalizedRange GetRange(CharacterInfo info1, CharacterInfo info2)
            {
                NormalizedRange temp = NormalizedRange.FromValues(info1.ClampedIndex, info2.ClampedIndex);

                if (info1.IsPostText ^ info2.IsPostText) return new NormalizedRange(temp.Start, info1._region._text.Length);

                return temp;
            }

            public static explicit operator int(CharacterInfo src)
            {
                return src.Index;
            }

            public static bool operator ==(CharacterInfo item1, CharacterInfo item2) => item1.Equals(item2);
            public static bool operator !=(CharacterInfo item1, CharacterInfo item2) => !item1.Equals(item2);

            public override bool Equals(object? obj)
            {
                return obj is CharacterInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(HashCode.Combine(_index, IsPostText), _region);
            }

            public bool Equals(CharacterInfo other)
            {
                return _index == other._index && IsPostText == other.IsPostText && _region == other._region;
            }
        }

        private readonly IRichLogger _logger;
        private readonly Environment _environment;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        public bool IsFocus { get; set; }

        public override int Height => _height;

        public override int VerticalOverlap => (int)(_lines.Last().Length == 0 ? 13 : 0);

        private string _text;
        private List<string> _lines;
        private Font2D _font;
        private Vector2 _textScale;
        private int _height;

        private const char NewLine = '\n';
        private const string InputLinePrefix = "> ";

        private NormalizedRange _readonlyRange;
        private NormalizedRange ReadonlyRange
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

        private CharacterInfo _cursor;
        private CharacterInfo Cursor
        {
            get => _cursor;
            set
            {
                if (!value.IsOnText)
                {
                    Debug.Assert(value == CharacterInfo.GetOutOfBounds(this));
                    _cursor = value;
                }
                else if (value.IsValidCursorInfo()) _cursor = value;
                else if (_hasInputLine && value.IsPostText) throw new InvalidOperationException($"Cursor cannot be placed post text if '{nameof(_hasInputLine)}' is true.");
                else if (!value.IsOnText) throw new InvalidOperationException($"Cursor '{value}' is not on text.");
                else throw new InvalidOperationException($"Cursor cannot be placed at '{value}'.");
            }
        }

        private CharacterInfo _lastClickCursor;
        private int _cursorLifetime; // resets when a new cursor is placed with the mouse.

        private bool _hasInputLine;
        public bool HasInputLine
        {
            get => _hasInputLine;
            set
            {
                if (value && !_hasInputLine)
                {
                    Debug.Assert(!Cursor.IsOnText, "Cursor is on text even though there is no input line.");

                    _hasInputLine = true;

                    Input(InputLinePrefix); // append to prevent _readonlyRange from extending.
                    ExtendReadonlyRange(InputLinePrefix.Length);
                }
                else if (!value && _hasInputLine)
                {
                    throw new NotImplementedException();
                    //_hasInputLine = false;
                }
            }
        }

        private bool _cursorSelecting;
        private NormalizedRange _cursorSelection;

        public TextShellRegion(Shell shell, IRichLogger logger, Environment environment, IFont2DCollection fonts, IInputManager input, ITexture2DCollection textures) : base(shell)
        {
            _logger = logger;
            _environment = environment;
            _fonts = fonts;
            _input = input;
            _textures = textures;

            _text = string.Empty;
            _lines = new List<string>();
            _font = _fonts.Medium;
            _textScale = Vector2.One;
            ReadonlyRange = NormalizedRange.Empty;

            STOLON.Instance.Window.TextInput += OnTextInput;
            STOLON.Instance.Window.KeyDown += OnKeyDown;

            _lastClickCursor = CharacterInfo.GetOutOfBounds(this);
            Cursor = CharacterInfo.GetOutOfBounds(this);
            ReadonlyRange = NormalizedRange.Empty;

            IsFocus = true;
        }

        private bool TrySetCursor(CharacterInfo newPos)
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

            string line = _lines[_lines.Count - 2];

            if (!line.EndsWith(NewLine)) throw new InvalidObjectException("Invalid line found (missing newline).");

            _lines[_lines.Count - 2] =
                _lines[_lines.Count - 2][..^1] + // remove newline.
                str + // add str.
                NewLine; // re-add newline.

            _text = _lines.ToJoinedString();

            UpdateText();

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

        private void Put<T>(T item, CharacterInfo pos) => Put(item.ToString()!, pos);
        private void Put(string str, CharacterInfo pos)
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
            _height = (int)(_lines.Count * _font.Dimensions.Y);
            //if (_lines.Last().Length == 0) _height -= (int)_font.Dimensions.Y;

            #region HANDLE_MOUSE

            if (_input.IsPressed(MouseButton.Left))
            {
                CharacterInfo character = GetCharacterInfoAt(_input.VirtualMousePos, true);

                if (!TrySetCursor(character))
                {
                    Cursor = CharacterInfo.GetOutOfBounds(this);
                }
                //Console.WriteLine(GetCharacterIndexAt(_input.VirtualMousePos, false));
                _cursorSelecting = true;
                if (character.IsOnText && _lastClickCursor.IsOnText)
                    _cursorSelection = CharacterInfo.GetRange(character, _lastClickCursor);
            }
            else
            {
                _cursorSelecting = false;
            }

            if (_input.IsClicked(MouseButton.Left))
            {
                _lastClickCursor = GetCharacterInfoAt(_input.VirtualMousePos, true);
                if (_lastClickCursor.IsValidCursorInfo())
                    _cursorLifetime = 0;

                if (_cursorSelection.Lenght > 0) _cursorSelection = NormalizedRange.Empty;
            }

            #endregion

            #region HANDLE_ARROWS

            // done in event handler.

            #endregion
        }

        private CharacterInfo GetCharacterInfoAt(Vector2 pos, bool clamp = false)
        {
            int charWidth = (int)_font.Dimensions.X;
            int charHeight = (int)_font.Dimensions.Y;

            int charIndexOnLine = (int)((pos.X - Pos.X) / charWidth);

            int charLineIndex = (int)((pos.Y - Pos.Y) / charHeight);
            charLineIndex = (_lines.Count - 1) - charLineIndex; // invert it. (text is top down)

            if (clamp) charLineIndex = Math.Clamp(charLineIndex, 0, _lines.Count - 1); // clamp y
            else if (charLineIndex >= _lines.Count || charLineIndex < 0) return CharacterInfo.GetOutOfBounds(this);

            string charLine = _lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _lines.Count - 1 && charIndexOnLine > charLine.Length) // why I need this check with x but not y remains a mystery.
                {
                    return CharacterInfo.GetPostText(this);
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, charLine.Length == 0 ? 0 : (charLine.Length - 1)); // clamp x, ?: because of empty lines, remove the -1 and when selecting lines, the cursor will be placed after the newline.
            }
            else if (charIndexOnLine > charLine.Length || charIndexOnLine < 0) return CharacterInfo.GetOutOfBounds(this);

            int result = charIndexOnLine;

            for (int lineIndex = 0; lineIndex < charLineIndex; lineIndex++)
                result += _lines[lineIndex].Length; // newline is already in line.

            if (result == _text.Length) return CharacterInfo.GetPostText(this);

            //result = Math.Clamp(result, 0, _text.Length - 1); // because adding line lenghts requires this to prevent ex.

            //Console.WriteLine($"{charLineIndex}:{charIndexOnLine} = {result}");
            //Console.WriteLine($"out of {_text.Length}");
            //Console.WriteLine($"char {_text[result]}");

            return new CharacterInfo(this, result);
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

            float x = Pos.X + charIndexOnLine * _font.Dimensions.X;
            float y = Pos.Y + (_lines.Count - charLine - 1) * _font.Dimensions.Y;

            return new Vector2(x, y);
        }

        private Vector2 GetCursorScreenPos()
        {
            if (!Cursor.IsOnText) throw new InvalidOperationException($"Cursor is not on text.");

            if (Cursor.IsPostText)
            {
                if (_text.Length == 0) return Pos; //  + new Vector2(0, -_font.Dimensions.Y);

                char selectedChar = _text[_text.Length - 1];
                if (selectedChar == NewLine) return Pos + new Vector2(0, -_font.Dimensions.Y + (GetCharacterScreenPosAt(_text.Length - 1) - Pos).Y);
                else return GetCharacterScreenPosAt(_text.Length - 1) + new Vector2(_font.Dimensions.X, 0);
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
                            Cursor = new CharacterInfo(this, newPos);
                        }
                    }

                    break;
                case '\r':
                    PutAndOffset(NewLine);
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
            drawingContext.DrawString(_font, ReplaceAt(_text, _cursorSelection, '_'), Pos, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0 && Cursor.IsOnText)
                drawingContext.Draw(_textures["UI\\cursor"], GetCursorScreenPos() + new Vector2(0, 2));
            //drawingContext.DrawLine(_input.VirtualMousePos, _input.VirtualMousePos);
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
