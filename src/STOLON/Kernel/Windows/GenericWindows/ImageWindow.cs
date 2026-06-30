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

        public ImageWindow(WindowDependencies deps, EntityDefinition entity, int mipmapSize = 128) : this(deps, entity.Mipmaps[mipmapSize], entity.Name)
        {

        }

        public ImageWindow(WindowDependencies deps, Texture2D image, string? name = null) : base(deps, image.Width, image.Height)
        {
            Image = image;
            Name = name is null ? string.Empty : ("Img: " + name);

            MaxSize = Image.Bounds.Size;
            IsResizable = true;

            _imgPos = Vector2.Zero;

            AddButton(new CloseWindowButton(deps.Textures));
            AddButton(new ToggleLockWindowButton(deps.Textures));
        }

        protected override void OnBoundsChanged()
        {
            _imgPos = Centering.Center(new Point(Image.Width, Image.Height), new Rectangle(0, 0, InnerBounds.Width, InnerBounds.Height));
            NumberHelper.OnPixel(ref _imgPos);
        }

        protected override void UpdateContents(int elapsedMilliseconds) { }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(Image, _imgPos);
        }
    }
}
