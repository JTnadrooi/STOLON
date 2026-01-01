namespace STOLON
{
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
}
