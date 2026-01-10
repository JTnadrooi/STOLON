using Microsoft.Xna.Framework.Graphics;

namespace STOLON
{
    public class ImageShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public override int Height => _window.Dimensions.Y;

        private ImageWindow _window;

        private Vector2 _borderCompensatingOffset;

        public ImageShellRegion(ITexture2DCollection textures, Kernel kernel, Shell shell, Texture2D texture, string? title = null) : base(shell)
        {
            _textures = textures;
            _kernel = kernel;

            _window = new ImageWindow(_textures, _kernel, texture, title);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _window.Position = this.Position;
            _window.Update(elapsedMilliseconds);
        }

        public override void Draw(DrawingContext drawingContext)
        {
            // window drawing is done by kernel.
        }
    }
}
