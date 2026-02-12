using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class ImageWindow : Window
    {
        public Texture2D Image { get; }

        private Vector2 _imgPos;

        public ImageWindow(Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Texture2D image)
            : base(kernel, textures, fonts, input, image.Width, image.Height)
        {
            Image = image;
            Name = "Img: North";

            MaxSize = Image.Bounds.Size;
            IsResizable = true;

            _imgPos = Vector2.Zero;

            AddButton(new CloseWindowButton(textures));
            AddButton(new ToggleLockWindowButton(textures));
        }

        protected override void OnBoundsChanged()
        {
            _imgPos = Centering.Center(new Point(Image.Width, Image.Height), new Rectangle(0, 0, InnerBounds.Width, InnerBounds.Height));
            _imgPos = _imgPos.PixelLock();
        }

        protected override void UpdateContents(int elapsedMilliseconds) { }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(Image, _imgPos);
        }
    }
}
