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

        public ImageWindow(Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Texture2D image)
            : base(kernel, textures, fonts, input, image.Width, image.Height)
        {
            Image = image;
            Name = "Img: North";

            AddButton(new CloseWindowButton(textures));
            AddButton(new ToggleLockWindowButton(textures));
        }

        protected override void UpdateContents(int elapsedMilliseconds)
        {

        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(Image, Vector2.Zero);
        }
    }
}
