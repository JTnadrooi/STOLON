namespace STOLON
{
    public class Shell : Service, ISingletonDependency
    {
        private string _text;
        private Font2D _font;
        private Vector2 _textScale;
        private Vector2 _debugCursorPos;
        private Vector2 _textPos;

        private const string NewLine = "\n";

        public bool IsFocus { get; set; }

        private readonly IRichLogger _logger;
        private readonly Environment _environment;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;

        public Shell(IRichLogger logger, Environment environment, IFont2DCollection fonts, IInputManager input) : base(null)
        {
            _logger = logger;
            _environment = environment;
            _fonts = fonts;
            _input = input;

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
            int lines = _text.Count(c => c == '\n') + 1;
            _textPos = new Vector2(10, STOLON.V_HEIGHT - _font.Dimensions.Y - _font.Dimensions.Y * lines);


            if (_input.IsClicked(MouseButton.Left))
            {
                int index = GetCharacterIndexAt(_input.VirtualMousePos);

                Console.WriteLine(index);

                if (index != -1) _text = ReplaceAt(_text, index, '_');
            }
        }

        private int GetCharacterIndexAt(Vector2 pos)
        {
            int charWidth = (int)_font.Dimensions.X;
            //int charHeight = (int)_font.Dimensions.Y;

            int result = (int)((pos.X - _textPos.X) / charWidth);

            string firstLine = _text.Split(NewLine).First();

            if (result > firstLine.Length) return -1;

            return result;
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
            drawingContext.DrawLine(_input.VirtualMousePos, _input.VirtualMousePos);
        }

        #region TEMP_UTILS


        private static string ReplaceAt(string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        #endregion
    }
}
