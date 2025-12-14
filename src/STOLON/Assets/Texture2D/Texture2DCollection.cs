using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;

namespace STOLON
{
    public class Texture2DCollection : ResourceCollection<Texture2D>
    {
        private Texture2D? _pixel;

        public Texture2D Pixel => _pixel ?? throw new Exception();

        public Texture2DCollection() : base(new Texture2DResourceLoader()) { }

        public override void LoadResources()
        {
            _pixel = new Texture2D(STOLON.Instance.GraphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
            base.LoadResources();
        }

        public override void UnloadResources()
        {
            _pixel.Dispose();
            base.UnloadResources();
        }
    }
}
