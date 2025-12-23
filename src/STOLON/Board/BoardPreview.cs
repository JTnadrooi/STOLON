using Autofac;

namespace STOLON
{
    public class BoardPreview : IGraphic
    {
        public BoardState SourceState { get; }
        public Rectangle Bounds { get; private set; }
        public Vector2 Pos
        {
            get => _pos;
            set
            {
                _pos = value;
                Bounds = new Rectangle(value.ToPoint(), Dimensions);
            }
        }
        public Point Dimensions => new Point(SourceState.Dimensions.X * TILE_SIZE, SourceState.Dimensions.Y * TILE_SIZE);

        private Texture2D _tileTexure;
        private Vector2 _pos;

        public const int TILE_SIZE = 16;

        private readonly Font2D _font;

        public BoardPreview(BoardState state, Font2D font)
        {
            SourceState = state;

            _font = font;

            _tileTexure = STOLON.Services.Resolve<ITexture2DCollection>()["Debug\\temp-" + TILE_SIZE];
        }
        public void Draw(DrawingContext drawingContext)
        {
            for (int x = 0; x < SourceState.Dimensions.X; x++)
                for (int y = 0; y < SourceState.Dimensions.Y; y++)
                {
                    //drawingContext.Draw(_tileTexure, Pos + new Vector2(x, y) * TILE_SIZE);
                    drawingContext.DrawString(_font, "?", Pos + new Vector2(x, y) * TILE_SIZE + new Vector2(7, 3));
                }
            drawingContext.DrawRectangle(Bounds, Color.White, 2);
        }
    }
}
