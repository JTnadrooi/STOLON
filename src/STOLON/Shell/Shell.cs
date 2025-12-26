using System.Security.Claims;
using System.Text.RegularExpressions;

namespace STOLON
{
    public class Shell : Service, ISingletonDependency
    {
        //[Flags]
        //public enum GetCharacterIndexAtNotOnTextDomain
        //{
        //    None = 0,
        //    Above = 1,
        //    Below = 2,
        //    Left = 4,
        //    Right = 8,
        //}

        public readonly record struct GetCharacterIndexAtReturnArgs(int Pos, bool PostText) // "just use a tuple" dont want to
        {
            public static implicit operator int(GetCharacterIndexAtReturnArgs src)
            {
                return src.Pos;
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

        private int _cursorIndex;
        private int _cursorLastClickIndex;
        private int _cursorLifetime; // resets when a new cursor is placed with the mouse.
        private bool _cursorPosText;

        private bool _cursorSelecting;
        private NormalizedRange _cursorSelection;

        public Shell(IRichLogger logger, Environment environment, IFont2DCollection fonts, IInputManager input, ITexture2DCollection textures) : base(null)
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
            _debugCursorPos = Vector2.Zero;

            STOLON.Instance.Window.TextInput += OnTextInput;

            IsFocus = true;
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

            if (_input.IsPressed(MouseButton.Left))
            {
                (_cursorIndex, _cursorPosText) = GetCharacterIndexAt(_input.VirtualMousePos, true);
                Console.WriteLine(GetCharacterIndexAt(_input.VirtualMousePos, false));
                _cursorSelecting = true;
                if (_cursorIndex != -1 && _cursorLastClickIndex != -1)
                    _cursorSelection = NormalizedRange.GetFromValues(_cursorLastClickIndex, _cursorIndex);
            }
            else
            {
                _cursorSelecting = false;
            }

            if (_input.IsClicked(MouseButton.Left))
            {
                _cursorLastClickIndex = _cursorIndex;
                _cursorLifetime = 0;

                if (_cursorSelection.Lenght > 0) _cursorSelection = NormalizedRange.Empty;
            }
        }

        private GetCharacterIndexAtReturnArgs GetCharacterIndexAt(Vector2 pos, bool clamp = false)
        {
            int charWidth = (int)_font.Dimensions.X;
            int charHeight = (int)_font.Dimensions.Y;

            int charIndexOnLine = (int)((pos.X - _textPos.X) / charWidth);

            int charLineIndex = (int)((pos.Y - _textPos.Y) / charHeight);
            charLineIndex = (_lines.Count - 1) - charLineIndex; // invert it. (text is top down)

            if (clamp) charLineIndex = Math.Clamp(charLineIndex, 0, _lines.Count - 1); // clamp y
            else if (charLineIndex >= _lines.Count || charLineIndex < 0) return new GetCharacterIndexAtReturnArgs(-1, false);

            string charLine = _lines[charLineIndex];

            if (clamp)
            {
                if (charLineIndex == _lines.Count - 1 && charIndexOnLine > charLine.Length) // why I need this check with x but not y remains a mystery.
                {
                    return new GetCharacterIndexAtReturnArgs(clamp ? (_text.Length - 1) : -1, true);
                }
                charIndexOnLine = Math.Clamp(charIndexOnLine, 0, charLine.Length == 0 ? 0 : (charLine.Length - 1)); // clamp x, ?: because of empty lines, remove the -1 and when selecting lines, the cursor will be placed after the newline.
            }
            else if (charIndexOnLine > charLine.Length || charIndexOnLine < 0) return new GetCharacterIndexAtReturnArgs(-1, false);

            int result = charIndexOnLine;

            for (int lineIndex = 0; lineIndex < charLineIndex; lineIndex++)
                result += _lines[lineIndex].Length; // newline is already in line.

            if (result == _text.Length) return new GetCharacterIndexAtReturnArgs(clamp ? (_text.Length - 1) : -1, true);

            //result = Math.Clamp(result, 0, _text.Length - 1); // because adding line lenghts requires this to prevent ex.

            //Console.WriteLine($"{charLineIndex}:{charIndexOnLine} = {result}");
            //Console.WriteLine($"out of {_text.Length}");
            //Console.WriteLine($"char {_text[result]}");

            return new GetCharacterIndexAtReturnArgs(result, false);
        }

        private Vector2 GetCharacterPosAt(int charIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(charIndex, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(charIndex, _text.Length);

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

        private Vector2 GetCursorPos()
        {

            if (_cursorPosText)
            {
                char selectedChar = _text[_text.Length - 1];
                Console.WriteLine(selectedChar == NewLine);
                if (selectedChar == NewLine) return _textPos + new Vector2(0, -_font.Dimensions.Y + (GetCharacterPosAt(_text.Length - 1) - _textPos).Y);
                else return GetCharacterPosAt(_text.Length - 1) + new Vector2(_font.Dimensions.X, 0);
            }
            else return GetCharacterPosAt(_cursorIndex);
        }


        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            switch (e.Character)
            {
                case '\b':
                    _text = _text[..^1];
                    break;
                case '\r':
                    Write(NewLine);
                    return;
                default:
                    Write(e.Character);
                    break;
            }

            _cursorSelection = NormalizedRange.Empty;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawString(_font, ReplaceAt(_text, _cursorSelection, '_'), _textPos, scale: _textScale);
            if (((int)(_cursorLifetime * 0.03f)) % 2 == 0)
                drawingContext.Draw(_textures["UI\\cursor"], GetCursorPos() + new Vector2(0, 2));
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
