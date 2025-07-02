using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

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

        private Texture2DAtlas _ditherAtlas;

        public const int DITHER_FRAME_COUNT = 5;
        public const int DITHER_TEXTURE_SIZE = 32;

        public DrawingContext()
        {
            STOLON.Debug.Log(">[s]initialising drawing context");
            _spriteBatch = new SpriteBatch(STOLON.Instance.GraphicsDevice);
            _graphics = STOLON.Instance.GraphicsDevice;

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired(STOLON.Instance.DesiredDimensions);
            _rt2 = GetDesired(STOLON.Instance.DesiredDimensions);

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, STOLON.Instance.DesiredDimensions.Y, 0);

            _effects = new Dictionary<string, GameEffect>();
            GameEffect[] tempEffects = STOLON.Scan<GameEffect>();
            STOLON.Debug.Log(">searching for effects");
            foreach (GameEffect effect in tempEffects)
            {
                STOLON.Debug.Log($"found effect with name \"{effect.Effect.Name}\".");
                _effects.Add(effect.Effect.Name["Effects\\".Length..], effect);
            }
            STOLON.Debug.Success();

            _ditherAtlas = Texture2DAtlas.Create("dither_tile", STOLON.Textures["UI\\dither_sheet-128"], DITHER_TEXTURE_SIZE, DITHER_TEXTURE_SIZE);

            STOLON.Debug.Success();
        }

        private RenderTarget2D GetVirtual() => new RenderTarget2D(_graphics, STOLON.V_WIDTH, STOLON.V_HEIGHT);
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

            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, _invertYMatrix);
            _spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        public void DrawArea(Rectangle destinationRectangle, Color color)
            => Draw(STOLON.Textures.Pixel, destinationRectangle, color: color);
        public void Draw(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(texture, GetDestinationRectangle(texture, position, scale), sourceRectangle, color, rotation, origin, effects, layerDepth);
        public void Draw(Texture2D texture, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(texture, GetDestinationRectangle(texture, position, scale), sourceRectangle, color, rotation, origin, effects, layerDepth);
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            _spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color ?? Color.White, rotation, origin ?? Vector2.Zero, InvertY(effects), layerDepth);
        }

        private Rectangle GetDestinationRectangle(Texture2D texture, Vector2 position, float scale)
            => GetDestinationRectangle(texture, position, new Vector2(scale));
        private Rectangle GetDestinationRectangle(Texture2D texture, Vector2 position, Vector2? scale = null)
            => new Rectangle(position.ToPoint(), (texture.Bounds.Size.ToVector2() * (scale ?? Vector2.One)).ToPoint());
        private SpriteEffects InvertY(SpriteEffects effect) => effect ^ SpriteEffects.FlipVertically; // I don't think this completelly works.
        private Rectangle? TranslateSourceRectangle(Rectangle? sourceRectangle)
            => sourceRectangle == null ? null : new Rectangle(sourceRectangle.Value.Location + new Point(0, sourceRectangle.Value.Height), sourceRectangle.Value.Size);

        public void DrawString(Font2D font, string text, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => DrawString(font, text, position, new Vector2(scale), rotation, origin, color, effects, layerDepth);
        public void DrawString(Font2D font, string text, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => _spriteBatch.DrawString(font, text, position, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale * font.Scale, InvertY(effects), layerDepth);

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawLine(x1, y1, x2, y2, color, thickness, layerDepth);
        public void DrawLine(Vector2 point1, Vector2 point2, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawLine(point1, point2, color, thickness, layerDepth);
        public void DrawRectangle(RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0f)
            => _spriteBatch.DrawRectangle(rectangle, color, thickness, layerDepth);

        public void DrawEntity(Entity entity, int res, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => DrawEntity(entity.Profile, res, position, scale, rotation, origin, effects, layerDepth, drawMode);
        public void DrawEntity(Entity entity, int res, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => DrawEntity(entity.Profile, res, position, scale, rotation, origin, effects, layerDepth, drawMode);
        public void DrawEntity(EntityProfile entityProfile, int res, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
            => DrawEntity(entityProfile, res, position, new Vector2(scale), rotation, origin, effects, layerDepth, drawMode);
        public void DrawEntity(EntityProfile entityProfile, int res, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, EntityDrawMode drawMode = EntityDrawMode.None)
        {
            void DrawEntity(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            {
                if (drawMode == EntityDrawMode.WithBackground) DrawArea(new RectangleF(position, new Vector2(res) * scale).ToRectangle(), Color.Black);
                Draw(texture, position, scale, rotation, origin, sourceRectangle, color, effects, layerDepth);
            }
            Rectangle sourceRec;
            Texture2D? texture;
            if (entityProfile.TryGetMipmap(res, out texture))
                DrawEntity(texture!, position, scale, rotation, origin, null, null, effects, layerDepth);
            else
            {
                switch (res)
                {
                    case 128:
                        texture = entityProfile.Mipmaps[512];
                        sourceRec = new Rectangle(entityProfile.Focus - new Point(128), new Size(256, 256));
                        scale *= 0.25f;
                        DrawEntity(texture, position + (drawMode == EntityDrawMode.Menu ? entityProfile.MenuOffset : Point.Zero).ToVector2(), scale, rotation, origin, sourceRec, null, effects, layerDepth);
                        break;
                    default: throw new Exception();
                }
            }
        }
        public void DrawSymbolNotation(string symbolNotationStr, Rectangle bounds)
        {
            DrawArea(bounds, Color.Black);
            DrawRectangle(bounds, Color.White, UserInterface.LINE_WIDTH);
            Vector2 dimensions = STOLON.Fonts.Medium.FastMeasure(symbolNotationStr);
            Vector2 scale = Vector2.One;
            if (dimensions.X > bounds.Width - 10) scale = new Vector2(0.8f, 1);
            dimensions *= scale;
            DrawString(STOLON.Fonts.Medium, symbolNotationStr, Centering.Center(dimensions.ToPoint(), bounds).PixelLock(), scale: scale);
        }

        public void DrawDither(Vector2 position, Point dimensions, float multiplierCoefficient, Color? color = null)
            => DrawDither(position, dimensions, (int)(Math.Clamp(multiplierCoefficient, 0.000001f, 0.999999f) * DITHER_FRAME_COUNT), color);
        public void DrawDither(Vector2 position, Point dimensions, int frame, Color? color = null)
        {
            if (dimensions.X % DITHER_TEXTURE_SIZE != 0 || dimensions.Y % DITHER_TEXTURE_SIZE != 0) throw new ArgumentOutOfRangeException(nameof(dimensions));
            if (frame < 0 || frame >= DITHER_FRAME_COUNT) throw new ArgumentOutOfRangeException(nameof(frame));
            for (int ox = 0; ox < dimensions.X; ox += _ditherAtlas[frame].Width)
                for (int oy = 0; oy < dimensions.Y; oy += _ditherAtlas[frame].Height)
                {
                    Size size = new Size(Math.Min(_ditherAtlas[frame].Width, dimensions.X - ox), Math.Min(_ditherAtlas[frame].Height, dimensions.Y - oy));
                    Draw(_ditherAtlas[frame].Texture, new Rectangle((int)position.X + ox, (int)position.Y + oy, size.Width, size.Height), sourceRectangle: new Rectangle(_ditherAtlas[frame].Bounds.Location, size), color: color ?? Color.White);
                }
        }

        public void DrawElement(UIElementDrawData drawData)
        {
            if (drawData.Source == null) throw new InvalidOperationException();
            if (drawData.Hide) return;
            DrawString(drawData.Font, drawData.Text, drawData.Position);
            if (drawData.DrawRectangle) DrawRectangle(drawData.Rectangle, Color.White, 1f);
        }

        public void Draw(IGraphic graphic)
        {
            graphic.Draw(this);
        }


        //public void DrawStringOutline(GameFont font, string text, Vector2 position, int marginX, int marginY, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, bool background = false, int lineWidth = UserInterface.LINE_WIDTH)
        //{
        //    Point nameDimensions = font.FastMeasure(text).ToPoint();
        //    Rectangle bounds = new Rectangle(position.ToPoint(), nameDimensions + new Point((int)(marginX * 2f), (int)(marginY * 2f)));
        //    Vector2 calcPos = (Centering.Center(nameDimensions, bounds) + new Vector2(1, 0)).PixelLock();
        //    DrawRectangle(bounds, color ?? Color.White, lineWidth);
        //    DrawString(font, text, calcPos, color: color, effects: effects, layerDepth: layerDepth);
        //}


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
