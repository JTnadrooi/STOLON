namespace STOLON
{
    public class BoardsGraphic : IUpdatable, IGraphic
    {
        public const int TILE_SIZE = 128;

        public readonly record struct BoardTemplate(string Name, string Description, Texture2D Texture)
        {
            public static BoardTemplate GetEmpty(ITexture2DCollection textures) => new BoardTemplate("[REDACTED]", "[REDACTED]", textures["UI\\profile_question-128"]);
        }

        public readonly record struct OptionDrawData(string Title, string Description, Texture2D Texture, Rectangle Bounds);

        public BoardTemplate[] Boards { get; }

        private OptionDrawData[] _optionDraws;
        private Vector2 _pos;

        private const int OPTION_SPACING = 10;
        private const int OPTION_TILE_SIZE = TILE_SIZE;
        private const int DIV_LINE_LENGHT = 100;
        public const int BOXED_TEXT_DIV_CLEARANCE = 32;
        private const float LERP_FACTOR = 0.15f;

        private float _scrollOffset;
        private float _targetScroll;

        private int _selectedIndex;
        private Rectangle _viewport;

        private readonly IInputManager _input;
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;

        public BoardsGraphic(IInputManager inputManager, ITexture2DCollection textures, IFont2DCollection fonts)
        {
            _input = inputManager;
            _textures = textures;
            _fonts = fonts;

            Boards = [
                new BoardTemplate("STAGE", "STOLON test level.", _textures["UI\\profile_question-128"]),
                    BoardTemplate.GetEmpty(textures),
                    BoardTemplate.GetEmpty(textures),
                ];
            _optionDraws = new OptionDrawData[Boards.Length];
            _pos = new Vector2(TILE_SIZE * 3, 0);
            _scrollOffset = 0;
            _targetScroll = 0;
            _selectedIndex = 0;
            //_viewport = new Rectangle(TILE_SIZE * 3, 0, TILE_SIZE * 2, ROSTER_BOTTOM_LINE);
            _viewport = default;

            throw new NotImplementedException();
        }

        public void Update(int elapsedMilliseconds)
        {
            int scrollDelta = Math.Sign(_input.MouseScrollDelta);
            if (scrollDelta != 0) _selectedIndex = Math.Clamp(_selectedIndex - scrollDelta, 0, _optionDraws.Length - 1);

            float viewportCenter = TILE_SIZE * 3 + (TILE_SIZE * 2) / 2f;
            float optionCenter = _pos.X + _selectedIndex * (OPTION_TILE_SIZE + OPTION_SPACING) + OPTION_TILE_SIZE / 2f;

            _targetScroll = optionCenter - viewportCenter;

            _scrollOffset = MathHelper.Lerp(_scrollOffset, _targetScroll, LERP_FACTOR);
            for (int i = 0; i < Boards.Length; i++)
            {
                _optionDraws[i] = new OptionDrawData(
                    Boards[i].Name,
                    Boards[i].Description,
                    Boards[i].Texture,
                    //new Rectangle(
                    //    (int)(_pos.X + i * (OPTION_TILE_SIZE + OPTION_SPACING) - _scrollOffset),
                    //    (int)_pos.Y + ROSTER_BOTTOM_LINE - TILE_SIZE - BOXED_TEXT_DIV_CLEARANCE / 2,
                    //    OPTION_TILE_SIZE,
                    //    OPTION_TILE_SIZE
                    //)
                    default
                );

                if (_input.IsClicked(MouseButton.Left) && _optionDraws[i].Bounds.Contains(_input.VirtualMousePos) && _viewport.Contains(_input.VirtualMousePos))
                {
                    _selectedIndex = i;
                }
            }
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.SetScissorArea(_viewport);

            for (int i = 0; i < _optionDraws.Length; i++)
                if (_viewport.Intersects(_optionDraws[i].Bounds))
                {
                    drawingContext.Draw(_optionDraws[i].Texture, _optionDraws[i].Bounds.Location.ToVector2());
                    drawingContext.DrawRectangle(_optionDraws[i].Bounds, Color.White);
                    drawingContext.DrawString(_fonts.Medium,
                        _optionDraws[i].Title.ToString().ToUpper(),
                        _optionDraws[i].Bounds.Location.ToVector2() + Centering.CenterX(
                            (int)_fonts.Medium.FastMeasure(_optionDraws[i].Title).X,
                            -_fonts.Medium.CoreFont.LineHeight,
                            TILE_SIZE
                        )
                    );
                    drawingContext.DrawHorizontalLine(_optionDraws[i].Bounds.Location.ToVector2() + new Vector2((TILE_SIZE - DIV_LINE_LENGHT) / 2f, -_fonts.Medium.CoreFont.LineHeight), DIV_LINE_LENGHT, thickness: 1);
                    drawingContext.DrawString(_fonts.Small,
                        _optionDraws[i].Description.ToUpper(),
                        _optionDraws[i].Bounds.Location.ToVector2() + Centering.CenterX(
                            (int)_fonts.Small.FastMeasure(_optionDraws[i].Description).X,
                            -_fonts.Medium.CoreFont.LineHeight * 2,
                            TILE_SIZE
                        )
                    );
                }

            drawingContext.ResetScissorArea();
        }
    }
}
