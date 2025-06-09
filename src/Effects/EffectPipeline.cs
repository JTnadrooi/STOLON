using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MonoGame.Extended.ECS;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace STOLON
{
    public abstract class GameEffect
    {
        private bool _enabled = true;
        public virtual bool Enabled { get => _enabled; set => _enabled = value; }
        public abstract bool Virtual { get; }
        public abstract Effect Effect { get; }
        public virtual void UpdateResolution(Point newDesiredRes) { }
    }

    public class EffectPipeline : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<string, GameEffect> _effects;

        private RenderTarget2D _vrt1;
        private RenderTarget2D _vrt2;
        private RenderTarget2D _rt1;
        private RenderTarget2D _rt2;

        private Matrix _invertYMatrix;

        public ReadOnlyDictionary<string, GameEffect> Effects => _effects.AsReadOnly();

        public EffectPipeline()
        {
            STOLON.Debug.Log(">[s]initialising effect pipeline");
            _spriteBatch = STOLON.Instance.DrawingContext.SpriteBatch;
            _graphics = STOLON.Instance.GraphicsDevice;

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired(STOLON.Instance.DesiredDimensions);
            _rt2 = GetDesired(STOLON.Instance.DesiredDimensions);

            _invertYMatrix = Matrix.CreateScale(1, -1, 1);

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
            _graphics.Clear(Color.Transparent);
        }

        public void UpdateResolution()
        {
            Point newRes = STOLON.Instance.DesiredDimensions;
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

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferHeight, 0);
            //var viewport = STOLON.Instance.GraphicsDevice.Viewport;
            //_invertYMatrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographicOffCenter(0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferWidth, 0, STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferHeight, 0, 1);
            _invertYMatrix = Matrix.CreateScale(1, 1, 1);
            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, _invertYMatrix);
            _spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        public void Dispose()
        {
            _vrt1.Dispose();
            _vrt2.Dispose();
            _rt1.Dispose();
            _rt2.Dispose();
            foreach (GameEffect effect in _effects.Values) (effect as IDisposable)?.Dispose();
        }
    }

    public class StolonReplaceColorEffect : GameEffect
    {
        public override Effect Effect { get; }
        public override bool Virtual => true;
        public StolonReplaceColorEffect()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("Effects\\apply_palette");
            Effect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
            Effect.Parameters["color1"].SetValue(STOLON.Instance.Color1.ToVector4());
            Effect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());
            Effect.Parameters["color2"].SetValue(STOLON.Instance.Color2.ToVector4());
        }
        public override void UpdateResolution(Point newDesiredRes) { }
    }
    public class CRTEffect : GameEffect
    {
        public override Effect Effect { get; }
        public override bool Virtual => false;
        public CRTEffect()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("Effects\\crt");
            Effect.Parameters["brightboost"].SetValue(0.92f);
            Effect.Parameters["textureSize"].SetValue(STOLON.Instance.DesiredDimensions.ToVector2());
            Effect.Parameters["outputSize"].SetValue(STOLON.Instance.DesiredDimensions.ToVector2());
        }
        public override void UpdateResolution(Point newDesiredRes)
        {
            Effect.Parameters["textureSize"].SetValue(newDesiredRes.ToVector2());
            Effect.Parameters["outputSize"].SetValue(newDesiredRes.ToVector2());
        }
    }
}
