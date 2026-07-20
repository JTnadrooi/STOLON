using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class TextureShellRegion : ShellRegion
    {
        public Texture2D Texture { get; set; }

        public override int Height => Texture.Height;

        public override int Width => Texture.Width;

        public TextureShellRegion(Shell shell, Texture2D texture) : base(shell)
        {
            Texture = texture;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(Texture, Position);
        }
    }
}
