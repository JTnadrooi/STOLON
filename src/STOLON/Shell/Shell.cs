using NAudio.CoreAudioApi;
using System.Text.RegularExpressions;

namespace STOLON
{
    public class Shell : Service, ISingletonDependency
    {
        private readonly struct CharacterInfo
        {
            public static Shell? s_shell;

            private readonly int _index;

            public readonly int Index
            {
                get => _index < 0 ? throw new InvalidOperationException() : _index;
            }

            public readonly bool IsPostText { get; }

            public readonly bool IsOnText => IsPostText || Index != -1;
            public readonly bool IsOnCharacter => Index != -1;
            public readonly int ClampedPos => IsPostText ? s_shell._text.Length - 1 : Index;

            public CharacterInfo(int index) : this(index, false)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, s_shell._text.Length);
            }

            private CharacterInfo(int index, bool isPostText)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, -1);

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
                            return CharacterInfo.PostText;
                        }
                        else
                        {
                            return new CharacterInfo(s_shell._text.Length + amount); // amount is negative here.
                        }
                    }
                    else
                    {
                        int newPos = Index + amount;

                        if (newPos >= s_shell._text.Length)
                        {
                            return CharacterInfo.PostText;
                        }
                        else return new CharacterInfo(newPos);
                    }
                }
                else
                {
                    int newPos = Index + amount;

                    if (s_shell.IsValidCursorIndex(newPos))
                    {
                        return new CharacterInfo(newPos);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid newPos.");
                    }
                }
            }

            public static CharacterInfo PostText { get; } = new CharacterInfo(-1, true);
            public static CharacterInfo OutOfBounds { get; } = new CharacterInfo(-1, false);

            public static NormalizedRange GetRange(CharacterInfo info1, CharacterInfo info2)
            {
                return NormalizedRange.GetFromValues(info1.ClampedPos, info2.ClampedPos);
            }

            public static explicit operator int(CharacterInfo src)
            {
                return src.Index;
            }
        }

        private readonly IRichLogger _logger;
        private readonly Environment _environment;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        public bool IsFocus { get; set; }

        private string _text;
        private List<string> _lines;
        private Font2D _font;
        private Vector2 _textScale;
        private Vector2 _debugCursorPos;
        private Vector2 _textPos;

        private const char NewLine = '\n';

        private CharacterInfo _cursor;
        private CharacterInfo _lastClickCursor;
        private int _cursorLifetime; // resets when a new cursor is placed with the mouse.

        private bool _cursorSelecting;
        private NormalizedRange _cursorSelection;

        public Shell(IRichLogger logger, Environment environment, IFont2DCollection fonts, IInputManager input, ITexture2DCollection textures) : base(null)
        {
            CharacterInfo.s_shell = this;

            _logger = logger;
            _environment = environment;
            _fonts = fonts;
            _input = input;
            _textures = textures;

            _text = string.Empty;
            _lines = new List<string>();
            _font = _fonts.Medium;
            _textScale = Vector2.One;
            _debugCursorPos = Vector2.Zero;

            STOLON.Instance.Window.TextInput += OnTextInput;
            STOLON.Instance.Window.KeyDown += OnKeyDown;

            IsFocus = true;
        }

        private bool IsValidCursorIndex(int index)
        {
            return index >= 0 && index < _text.Length;
        }

        public void WriteLine(string text)
        {
            Write(text + NewLine);
        }

        public void Write(char character) => Write(character.ToString());
        public void Write(string str)
        {
            if (str.Contains('\r')) throw new ArgumentException("Cannot write invalid newline. ('\\r'.)", nameof(str)); // newline is \n char

            _text += str;

            UpdateText();
        }

        public void PutAtCursor(char character) => PutAtCursor(character.ToString());
        public void PutAtCursor(string str)
        {
            if (!_cursor.IsOnText) throw new InvalidOperationException();

            if (_cursor.IsPostText) Write(str);
            else
            {
                Put(str, _cursor.Index);
                _cursor = _cursor.Offset(str.Length);
            }
        }

        public void Put(string str, int pos)
        {
            _text = _text.Insert(pos, str);

            UpdateText();
        }

        public bool RemoveAtCursor()
        {
            if (!_cursor.IsOnText) throw new InvalidOperationException();

            if (_text.Length == 0) return false;

            if (_cursor.IsPostText)
            {
                RemoveAt(_text.Length - 1);
            }
            else
            {
                int newPos = _cursor.Index - 1;

                if (!IsValidCursorIndex(newPos)) return false;

                RemoveAt(newPos);
                _cursor = new CharacterInfo(newPos);
            }

            return true;
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
            _textPos = new Vector2(10, STOLON.V_HEIGHT - _font.Dimensions.Y - _font.Dimensions.Y * _lines.Count);
            _cursorLifetime++;

            #region HANDLE_MOUSE

            if (_input.IsPressed(MouseButton.Left))
            {
                _cursor = GetCharacterPosAt(_input.VirtualMousePos, true);
                //Console.WriteLine(GetCharacterIndexAt(_input.VirtualMousePos, false));
                _cursorSelecting = true;
                if (_cursor.IsOnText && _lastClickCursor.IsOnText)
                    _cursorSelection = CharacterInfo.GetRange(_cursor, _lastClickCursor);
            }
            else
            {
                _cursorSelecting = false;
            }

            if (_input.IsClicked(MouseButton.Left))
            {
                _lastClickCursor = _cursor;
                _cursorLifetime = 0;

                if (_cursorSelection.Lenght > 0) _cursorSelection = NormalizedRange.Empty;
            }

            #endregion

            #region HANDLE_ARROWS


            #endregion
        }

        private CharacterInfo GetCharacterPosAt(Vector2 pos, bool clamp = false)
        {
            int charWidth = (int)_font.Dimensions.X;
            int charHeight = (int)_font.Dimensions.Y;

            int charIndexOnLine = (int)((pos.X - _textPos.X) / charWidth);

            int charLineIndex = (int)((pos.Y - _textPos.Y) / charHeight);
            charLineIndex = (_lines.Count - 1) - charLineIndex; // invert it. (text is top down)

            if (clamp) charLineIndex = Math.Clamp(charLineIndex, 0, _lines.Count - 1); // clamp y
            else if (charLineIndex >= _lines.Count || charLineIndex < 0) return CharacterInfo.OutOfBounds;

            string charLine = _lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _lines.Count - 1 && charIndexOnLine > charLine.Length) // why I need this check with x but not y remains a mystery.
                {
                    return CharacterInfo.PostText;
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, charLine.Length == 0 ? 0 : (charLine.Length - 1)); // clamp x, ?: because of empty lines, remove the -1 and when selecting lines, the cursor will be placed after the newline.
            }
            else if (charIndexOnLine > charLine.Length || charIndexOnLine < 0) return CharacterInfo.OutOfBounds;

            int result = charIndexOnLine;

            for (int lineIndex = 0; lineIndex < charLineIndex; lineIndex++)
                result += _lines[lineIndex].Length; // newline is already in line.

            if (result == _text.Length) return CharacterInfo.PostText;

            //result = Math.Clamp(result, 0, _text.Length - 1); // because adding line lenghts requires this to prevent ex.

            //Console.WriteLine($"{charLineIndex}:{charIndexOnLine} = {result}");
            //Console.WriteLine($"out of {_text.Length}");
            //Console.WriteLine($"char {_text[result]}");

            return new CharacterInfo(result);
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

            float x = _textPos.X + charIndexOnLine * _font.Dimensions.X;
            float y = _textPos.Y + (_lines.Count - charLine - 1) * _font.Dimensions.Y;

            return new Vector2(x, y);
        }

        private Vector2 GetCursorScreenPos()
        {
            if (_cursor.IsPostText)
            {
                if (_text.Length == 0) return _textPos; //  + new Vector2(0, -_font.Dimensions.Y);

                char selectedChar = _text[_text.Length - 1];
                if (selectedChar == NewLine) return _textPos + new Vector2(0, -_font.Dimensions.Y + (GetCharacterScreenPosAt(_text.Length - 1) - _textPos).Y);
                else return GetCharacterScreenPosAt(_text.Length - 1) + new Vector2(_font.Dimensions.X, 0);
            }
            else return GetCharacterScreenPosAt(_cursor.Index);
        }

        private void OnKeyDown(object? sender, InputKeyEventArgs e)
        {
            if (!_cursor.IsOnText) return;

            switch (e.Key)
            {
                case Keys.Left:
                    _cursor = _cursor.Offset(-1, true);
                    break;
                case Keys.Right:
                    _cursor = _cursor.Offset(1, true);
                    break;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (!_cursor.IsOnText) return;

            switch (e.Character)
            {
                case '\b':
                    RemoveAtCursor();
                    break;
                case '\r':
                    PutAtCursor(NewLine);
                    return;
                default:
                    PutAtCursor(e.Character);
                    break;
            }

            _cursorSelection = NormalizedRange.Empty;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawString(_font, ReplaceAt(_text, _cursorSelection, '_'), _textPos, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0)
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

        #endregion
    }
}
