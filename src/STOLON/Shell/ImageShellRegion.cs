namespace STOLON
{
    public class ImageShellRegion : ShellRegion
    {
        public override int Height => _texture.Height + _border.AddedHeight;

        private readonly Texture2D _texture;
        private readonly string? _title;

        private Rectangle _bounds;
        private Border _border;

        private Vector2 _borderCompensatingOffset;

        public ImageShellRegion(ITexture2DCollection textures, Shell shell, Texture2D texture, string? title = null) : base(shell)
        {
            _texture = texture;
            _border = new Border(textures["UI\\Borders\\shell_image-border"], 7, 1, 1, 1);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _borderCompensatingOffset = _border.GetCompensatingOffset();
            _bounds = new Rectangle((Pos + _borderCompensatingOffset).ToPoint(), _texture.Bounds.Size);

        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(_texture, Pos + _borderCompensatingOffset);
            //drawingContext.DrawRectangle(_bounds, thickness: 1);
            drawingContext.DrawBorderAround(_border, _bounds);
        }
    }
}
