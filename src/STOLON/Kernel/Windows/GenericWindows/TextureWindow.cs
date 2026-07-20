using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class TextureWindow : Window
    {
        public Texture2D Texture { get; }

        private Vector2 _texturePos;

        public TextureWindow(WindowDependencies deps, EntityDefinition entity, int mipmapSize = 128) : this(deps, entity.Mipmaps[mipmapSize], entity.Name)
        {

        }

        public TextureWindow(WindowDependencies deps, Texture2D texture, string? name = null) : base(deps, texture.Width, texture.Height)
        {
            Texture = texture;
            Name = name is null ? string.Empty : ("Img: " + name);

            MaxSize = Texture.Bounds.Size;
            IsResizable = true;

            _texturePos = Vector2.Zero;

            AddButton(new CloseWindowButton(deps.Textures));
            AddButton(new ToggleLockWindowButton(deps.Textures));
        }

        protected override void OnBoundsChanged()
        {
            _texturePos = Centering.Center(new Point(Texture.Width, Texture.Height), new Rectangle(0, 0, InnerBounds.Width, InnerBounds.Height));
            NumberHelper.OnPixel(ref _texturePos);
        }

        protected override void UpdateContents(int elapsedMilliseconds) { }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(Texture, _texturePos);
        }
    }
}
