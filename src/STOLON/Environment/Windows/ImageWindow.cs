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
        public string? Title { get; }

        public Point Dimensions => Image.Bounds.Size + new Point(Border.AddedWidth, Border.AddedHeight);

        public ImageWindow(ITexture2DCollection textures, Kernel kernel, Texture2D image, string? title = null) : base(kernel, textures, image.Width, image.Height)
        {
            Title = title;
            Image = image;

            AddButton(new CloseWindowButton(textures));
            AddButton(new ToggleLockWindowButton(textures));
        }

        protected override void UpdateContents(int elapsedMilliseconds)
        {

        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(Image, Vector2.Zero + new Vector2(20));
        }
    }
}
