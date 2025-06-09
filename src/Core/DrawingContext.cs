using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class DrawingContext : IDisposable
    {
        private SpriteBatch _batch;
        private bool _disposedValue;

        public SpriteBatch SpriteBatch => _batch;

        public DrawingContext()
        {
            _batch = new SpriteBatch(STOLON.Instance.GraphicsDevice);
        }

        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null)
            => _batch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        public void End() => _batch.End();

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color? color = null)
            => _batch.Draw(texture, destinationRectangle, color ?? Color.White);
        public void Draw(Texture2D texture, Vector2 position, Color? color = null)
            => _batch.Draw(texture, position, color ?? Color.White);

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
            => _batch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            => _batch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);




        //public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null, Color? color = null)
        //=> _batch.Draw(texture, destinationRectangle, sourceRectangle, color ?? Color.White);
        //public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) => _batch.Draw(texture, position, sourceRectangle, color);
        //public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 1f)
        //    => _batch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
            => _batch.DrawString(spriteFont, text, position, color);
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
            => _batch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            => _batch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _batch.DrawLine(x1, y1, x2, y2, color, thickness, layerDepth);
        public void DrawLine(Vector2 point1, Vector2 point2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _batch.DrawLine(point1, point2, color, thickness, layerDepth);
        public void DrawRectangle(RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0f)
            => _batch.DrawRectangle(rectangle, color, thickness, layerDepth);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _batch.Dispose();
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
