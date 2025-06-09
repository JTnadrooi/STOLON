using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class DrawingContext : IDisposable
    {
        private bool _disposedValue;

        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<string, GameEffect> _effects;

        private RenderTarget2D _vrt1;
        private RenderTarget2D _vrt2;
        private RenderTarget2D _rt1;
        private RenderTarget2D _rt2;

        private Matrix _invertYMatrix;

        public ReadOnlyDictionary<string, GameEffect> Effects => _effects.AsReadOnly();
        public Matrix InvertYMatrix => _invertYMatrix;
        //public SpriteBatch SpriteBatch => _spriteBatch;

        public DrawingContext()
        {
            STOLON.Debug.Log(">[s]initialising drawing context");
            _spriteBatch = new SpriteBatch(STOLON.Instance.GraphicsDevice);
            _graphics = STOLON.Instance.GraphicsDevice;

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired(STOLON.Instance.DesiredDimensions);
            _rt2 = GetDesired(STOLON.Instance.DesiredDimensions);

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferHeight, 0);

            _effects = new Dictionary<string, GameEffect>();
            GameEffect[] tempEffects = STOLON.Scan<GameEffect>();
            STOLON.Debug.Log(">searching for effects");
            foreach (GameEffect effect in tempEffects)
            {
                STOLON.Debug.Log($"found effect with name \"{effect.Effect.Name}\".");
                _effects.Add(effect.Effect.Name["Effects\\".Length..], effect);
            }
            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }

        private RenderTarget2D GetVirtual() => new RenderTarget2D(_graphics, STOLON.Instance.VirtualDimensions.X, STOLON.Instance.VirtualDimensions.Y);
        private RenderTarget2D GetDesired(Point res) => new RenderTarget2D(_graphics, res.X, res.Y);

        public void BeginScene()
        {
            _graphics.SetRenderTarget(_vrt1);
            _graphics.Clear(STOLON.Instance.Color2);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        }

        public void UpdateResolution()
        {
            Point newRes = STOLON.Instance.DesiredDimensions;
            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, newRes.Y, 0);
            _rt1 = GetDesired(newRes);
            _rt2 = GetDesired(newRes);
            foreach (GameEffect effect in _effects.Values.Where(e => !e.Virtual)) effect.UpdateResolution(newRes);
            STOLON.Debug.Log($"updated fx pipeline res.");
        }
        public void DisableEffect(string name)
        {
            if (!_effects[name].Enabled)
            {
                STOLON.Debug.Log($"effect \"{name}\" already disabled.");
                return;
            }
            _effects[name].Enabled = false;
            STOLON.Debug.Log($"disabled effect with name \"{name}\".");
        }
        public bool IsEnabled(string name) => _effects[name].Enabled;
        public void EnableEffect(string name)
        {
            if (_effects[name].Enabled)
            {
                STOLON.Debug.Log($"effect \"{name}\" already enabled.");
                return;
            }
            _effects[name].Enabled = true;
            STOLON.Debug.Log($"enabled effect with name \"{name}\".");
        }

        public void EndScene()
        {
            _spriteBatch.End();

            RenderTarget2D finalVTarget = _vrt1;

            foreach (GameEffect effect in _effects.Values.Where(e => e.Virtual && e.Enabled)) // apply virtual effects.
            {
                _graphics.SetRenderTarget(_vrt2);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect.Effect);
                _spriteBatch.Draw(_vrt1, Vector2.Zero, Color.White);
                _spriteBatch.End();

                finalVTarget = _vrt2;
                (_vrt1, _vrt2) = (_vrt2, _vrt1);
            }

            _graphics.SetRenderTarget(_rt1); // draw and upscale to normal sized rt.
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            _spriteBatch.Draw(finalVTarget, new Rectangle(Point.Zero, _rt1.Bounds.Size), Color.White);
            _spriteBatch.End();

            RenderTarget2D finalTarget = _rt1;

            foreach (GameEffect effect in _effects.Values.Where(e => !e.Virtual && e.Enabled)) // apply normal effects.
            {
                _graphics.SetRenderTarget(_rt2);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect.Effect);
                _spriteBatch.Draw(_rt1, Vector2.Zero, Color.White);
                _spriteBatch.End();

                finalTarget = _rt2;
                (_rt1, _rt2) = (_rt2, _rt1);
            }


            //var viewport = STOLON.Instance.GraphicsDevice.Viewport;
            //_invertYMatrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographicOffCenter(0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferWidth, 0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferHeight, 0, 1);
            //_invertYMatrix = Matrix.CreateScale(1, 1, 1);
            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, _invertYMatrix);
            _spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        //public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null)
        //    => _batch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        //public void End() => _batch.End();

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color? color = null)
            => _spriteBatch.Draw(texture, destinationRectangle, color ?? Color.White);
        public void Draw(Texture2D texture, Vector2 position, Color? color = null)
            => _spriteBatch.Draw(texture, position, color ?? Color.White);

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
            => _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            => _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);

        //public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null, Color? color = null)
        //=> _batch.Draw(texture, destinationRectangle, sourceRectangle, color ?? Color.White);
        //public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) => _batch.Draw(texture, position, sourceRectangle, color);
        //public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 1f)
        //    => _batch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
            => _spriteBatch.DrawString(spriteFont, text, position, color);
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
            => _spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            => _spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawLine(x1, y1, x2, y2, color, thickness, layerDepth);
        public void DrawLine(Vector2 point1, Vector2 point2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawLine(point1, point2, color, thickness, layerDepth);
        public void DrawRectangle(RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawRectangle(rectangle, color, thickness, layerDepth);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _spriteBatch.Dispose();
                _vrt1.Dispose();
                _vrt2.Dispose();
                _rt1.Dispose();
                _rt2.Dispose();
                foreach (GameEffect effect in _effects.Values) (effect as IDisposable)?.Dispose();
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
