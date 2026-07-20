using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class LayeredTextureShellRegion : ShellRegion
    {
        private int _width;
        public override int Width => _width;

        private int _height;
        public override int Height => _height;

        public ReadOnlyCollection<Texture2D> Textures { get; }

        private bool[] _drawConfig;
        public bool[] DrawConfig => _drawConfig;

        public LayeredTextureShellRegion(Shell shell, Texture2D[] textures) : this(shell, textures, ReadOnlySpan<int>.Empty) { }
        public LayeredTextureShellRegion(Shell shell, Texture2D[] textures, ReadOnlySpan<int> drawnIndexes) : base(shell)
        {
            if (textures.Length == 0)
            {
                throw new ArgumentException("Cannot layer textureless array", nameof(textures));
            }

            _width = textures[0].Width;
            _height = textures[0].Height;

            Textures = textures.AsReadOnly();
            foreach (Texture2D texture in textures)
                ThrowIfInvalidTexture(texture);

            _drawConfig = new bool[textures.Length];
            foreach (int i in drawnIndexes)
                _drawConfig[i] = true;
        }

        private void ThrowIfInvalidTexture(Texture2D texture)
        {
            if (texture.Height != _height || texture.Width != _width)
            {
                throw new ArgumentException($"Invalid texture dimensions for texture '{texture.Name}'.");
            }
        }

        public void DrawAll()
        {
            Array.Fill(_drawConfig, true);
        }

        public void DrawNone()
        {
            Array.Clear(_drawConfig);
        }

        public void DrawIndexes(params ReadOnlySpan<int> indexes)
        {
            Array.Clear(_drawConfig);
            for (int i = 0; i < indexes.Length; i++)
            {
                DrawConfig[indexes[i]] = true;
            }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                if (_drawConfig[i])
                {
                    drawingContext.Draw(Textures[i], Position, scale: 0.75f);
                }
            }
        }
    }
}
