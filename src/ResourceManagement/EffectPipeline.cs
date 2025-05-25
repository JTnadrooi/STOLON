using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MonoGame.Extended.ECS;

namespace STOLON
{
    public interface IEffect
    {
        public Effect Effect { get; }
        public bool Virtual { get; }
        public void SetParameters();
    }

    public class EffectPipeline : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<string, IEffect> _effects;

        private RenderTarget2D _vrt1;
        private RenderTarget2D _vrt2;
        private RenderTarget2D _rt1;
        private RenderTarget2D _rt2;

        public EffectPipeline()
        {
            RenderTarget2D GetVirtual() => new RenderTarget2D(_graphics, STOLON.Instance.VirtualDimensions.X, STOLON.Instance.VirtualDimensions.Y);
            RenderTarget2D GetDesired() => new RenderTarget2D(_graphics, STOLON.Instance.DesiredDimensions.X, STOLON.Instance.DesiredDimensions.Y);

            STOLON.Debug.Log(">[s]initialising effect pipeline");
            _spriteBatch = STOLON.Instance.SpriteBatch;
            _graphics = STOLON.Instance.GraphicsDevice;

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired();
            _rt2 = GetDesired();

            _effects = new Dictionary<string, IEffect>();
            IEffect[] tempEffects = STOLON.Scan<IEffect>();
            foreach (IEffect effect in tempEffects)
            {
                STOLON.Debug.Log($"found effect with name \"{effect.Effect.Name}\".");
                _effects.Add(effect.Effect.Name, effect);
            }
            STOLON.Debug.Log(">searching for effects");
            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }

        public void BeginScene()
        {
            _graphics.SetRenderTarget(_vrt1);
            _graphics.Clear(Color.Transparent);
        }
        public void EndScene()
        {
            RenderTarget2D finalVTarget = _vrt1;
            foreach (IEffect effect in _effects.Values.Where(e => e.Virtual))
            {
                effect.SetParameters();

                _graphics.SetRenderTarget(_vrt2); // virtual
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect.Effect);
                _spriteBatch.Draw(_vrt1, Vector2.Zero, Color.White);
                _spriteBatch.End();

                finalVTarget = _vrt2;
                (_vrt1, _vrt2) = (_vrt2, _vrt1);
            }

            _graphics.SetRenderTarget(null); // non-virtual
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            _spriteBatch.Draw(finalVTarget, new Rectangle(Point.Zero, STOLON.Instance.DesiredDimensions), Color.White);
            _spriteBatch.End();
        }

        public void Dispose()
        {
            _vrt1.Dispose();
            _vrt2.Dispose();
            foreach (IEffect post in _effects.Values) (post as IDisposable)?.Dispose();
        }
    }

    public class StolonReplaceColorEffect : IEffect
    {
        public Effect Effect { get; }
        public bool Virtual => true;
        public StolonReplaceColorEffect()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("effects\\ReplaceColor");
            Effect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
            Effect.Parameters["color1"].SetValue(STOLON.Instance.Color1.ToVector4());
            Effect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());
            Effect.Parameters["color2"].SetValue(STOLON.Instance.Color2.ToVector4());
        }

        public void SetParameters()
        {
        }
    }
    public class CRTEffect : IEffect
    {
        public Effect Effect { get; }
        public bool Virtual => false;

        public CRTEffect()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("effects\\CRT-Lottes");
            Effect.Parameters["brightboost"].SetValue(0.92f);

            Effect.Parameters["textureSize"].SetValue(STOLON.Instance.VirtualDimensions.ToVector2());
            //Shader.Parameters["videoSize"].SetValue(STOLON.Instance.VirtualDimensions.ToVector2());
            Effect.Parameters["outputSize"].SetValue(STOLON.Instance.VirtualDimensions.ToVector2());
        }

        public void SetParameters()
        {
        }
    }
}
