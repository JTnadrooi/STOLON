using Microsoft.Xna.Framework.Graphics;

namespace STOLON
{
    public class ImageShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public override int Height => _window.OuterBounds.Height;

        private ImageWindow _window;

        private Vector2 _borderCompensatingOffset;

        public ImageShellRegion(Shell shell, Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Texture2D texture) : base(shell)
        {
            _textures = textures;
            _kernel = kernel;

            _window = new ImageWindow(_kernel, _textures, fonts, input, texture)
            {
                IsManaged = false
            };
        }

        public override void Update(int elapsedMilliseconds)
        {
            _window.Position = this.Position;
            _window.Update(elapsedMilliseconds);
        }

        public override void Draw(DrawingContext drawingContext)
        {
            _window.Draw(drawingContext);
            // window drawing is done by kernel.
        }
    }
}
