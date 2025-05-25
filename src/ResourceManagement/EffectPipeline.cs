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
        public void SetParameters();
    }

    public class EffectPipeline : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly RenderTarget2D _sceneTarget;
        private readonly Dictionary<string, IEffect> _effects;
        private readonly RenderTarget2D _rt1;
        private readonly RenderTarget2D _rt2;

        public EffectPipeline(GraphicsDevice graphics, SpriteBatch sb, int w, int h)
        {
            STOLON.Debug.Log(">[s]initialising effect pipeline");
            STOLON.Debug.Log(">searching for effects");
            _graphics = graphics;
            _spriteBatch = sb;
            _rt1 = new RenderTarget2D(graphics, w, h);
            _rt2 = new RenderTarget2D(graphics, w, h);
            _sceneTarget = _rt1;
            _effects = new Dictionary<string, IEffect>();
            IEffect[] tempEffects = STOLON.Scan<IEffect>();
            foreach (IEffect effect in tempEffects)
            {
                STOLON.Debug.Log($"found effect with name \"{effect.Effect.Name}\".");
                _effects.Add(effect.Effect.Name, effect);
            }
            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }

        public void BeginScene()
        {
            _graphics.SetRenderTarget(_sceneTarget);
            _graphics.Clear(Color.Transparent);
        }
        public void EndScene()
        {
            RenderTarget2D src = _sceneTarget;
            RenderTarget2D dst = _rt2;

            foreach (IEffect effect in _effects.Values)
            {
                effect.SetParameters();

                _graphics.SetRenderTarget(dst);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect.Effect);
                _spriteBatch.Draw(src, Vector2.Zero, Color.White);
                _spriteBatch.End();

                RenderTarget2D tmp = src;
                src = dst;
                dst = tmp;
            }

            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            _spriteBatch.Draw(src, new Rectangle(Point.Zero, STOLON.Instance.DesiredDimensions), Color.White);
            _spriteBatch.End();
        }

        public void Dispose()
        {
            _rt1.Dispose();
            _rt2.Dispose();
            foreach (IEffect post in _effects.Values) (post as IDisposable)?.Dispose();
        }
    }

    public class StolonReplaceColorEffect : IEffect
    {
        public Effect Effect { get; }
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
