namespace STOLON
{
    public class Shell : Service, ISingletonDependency
    {
        private readonly IRichLogger _logger;
        private readonly Environment _environment;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        public bool IsFocus { get; set; }

        private string _text;
        private Font2D _font;
        private Vector2 _textScale;
        private Vector2 _debugCursorPos;
        private Vector2 _textPos;

        private const string NewLine = "\n";

        private int _cursorIndex;
        private int _lineCount;
        private int _cursorLifetime;

        public Shell(IRichLogger logger, Environment environment, IFont2DCollection fonts, IInputManager input, ITexture2DCollection textures) : base(null)
        {
            _logger = logger;
            _environment = environment;
            _fonts = fonts;
            _input = input;
            _textures = textures;

            _text = string.Empty;
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
        public void Write(string text)
        {
            _text += text;
        }

        public override void Update(int elapsedMilliseconds)
        {
            _textScale = Vector2.One;
            _lineCount = _text.Count(c => c == '\n') + 1;
            _textPos = new Vector2(10, STOLON.V_HEIGHT - _font.Dimensions.Y - _font.Dimensions.Y * _lineCount);
            _cursorLifetime++;

            if (_input.IsClicked(MouseButton.Left))
            {
                _cursorIndex = GetCharacterIndexAt(_input.VirtualMousePos);
                _cursorLifetime = 0;

                Console.WriteLine(_cursorIndex);

                if (_cursorIndex != -1) _text = ReplaceAt(_text, _cursorIndex, '_');
            }
        }

        private int GetCharacterIndexAt(Vector2 pos)
        {
            int charWidth = (int)_font.Dimensions.X;

            int result = (int)((pos.X - _textPos.X) / charWidth);

            string firstLine = _text.Split(NewLine).First();

            if (result > firstLine.Length) return -1;
            if (result < 0) return -1;

            return result;
        }

        private Vector2 GetCharacterPosAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);

            int line = 1;

            return new Vector2(_textPos.X + index * _font.Dimensions.X, _textPos.Y + _lineCount * _font.Dimensions.Y - line * _font.Dimensions.Y);
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
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawString(_font, _text, _textPos, scale: _textScale);
            if (_cursorIndex > 0 && ((int)(_cursorLifetime * 0.03f)) % 2 == 0)
                drawingContext.Draw(_textures["UI\\cursor"], GetCharacterPosAt(_cursorIndex) + new Vector2(0, 2));
            //drawingContext.DrawLine(_input.VirtualMousePos, _input.VirtualMousePos);
        }

        #region TEMP_UTILS


        private static string ReplaceAt(string input, int index, char newChar)
        {
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        #endregion
    }
}
