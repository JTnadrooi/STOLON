using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace STOLON
{
    public enum ScalingMethod
    {
        None,
        Integer,
        NearestNeighbour,
    }

    public class DrawingContext : IDisposable
    {
        private bool _disposedValue;

        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Dictionary<string, Shader> _shaders;

        private RenderTarget2D _vrt1;
        private RenderTarget2D _vrt2;
        private RenderTarget2D _rt1;
        private RenderTarget2D _rt2;

        private Matrix _invertYMatrix;

        public ReadOnlyDictionary<string, Shader> Shaders => _shaders.AsReadOnly();
        public Matrix InvertYMatrix => _invertYMatrix;
        /// <summary>
        /// Note everything drawn to this will be inverted. Please use the extension methods or call <see cref="InvertY(SpriteEffects)"/> on the input <see cref="SpriteEffects"/> enum.
        /// </summary>
        public SpriteBatch SpriteBatch => _spriteBatch;
        public bool SpriteBatchStarted => _spritebatchStarted;

        public ScalingMethod ScalingMethod { get; }

        private Texture2DAtlas _ditherAtlas;
        private Texture2D _screenshotCache;
        private bool _screenshotPending;

        private bool _spritebatchStarted;
        private bool _scissorEnabled;

        //RasterizerState _scissorRasterizerState;
        public const int DITHER_FRAME_COUNT = 5;
        public const int DITHER_TEXTURE_SIZE = 32;

        private SamplerState _samplerState;

        public DrawingContext()
        {
            STOLON.Debug.Log(">[s]initialising drawing context");
            _spriteBatch = new SpriteBatch(STOLON.Instance.GraphicsDevice);
            _graphics = STOLON.Instance.GraphicsDevice;
            //_scissorRasterizerState = new RasterizerState() { ScissorTestEnable = true };

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired(STOLON.Instance.DesiredDimensions);
            _rt2 = GetDesired(STOLON.Instance.DesiredDimensions);

            _samplerState = SamplerState.PointClamp;

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, STOLON.Instance.DesiredDimensions.Y, 0);

            _shaders = new Dictionary<string, Shader>();
            Shader[] tempShaders = STOLON.Scan<Shader>();
            STOLON.Debug.Log(">searching for effects");
            foreach (Shader shader in tempShaders)
            {
                STOLON.Debug.Log($"found effect with name '{shader.Effect.Name}'.");
                _shaders.Add(shader.Effect.Name, shader);
            }
            STOLON.Debug.Success();

            _ditherAtlas = Texture2DAtlas.Create("dither_tile", STOLON.Textures["UI\\dither_sheet-128"], DITHER_TEXTURE_SIZE, DITHER_TEXTURE_SIZE);
            _screenshotCache = new Texture2D(STOLON.Instance.GraphicsDevice, STOLON.V_WIDTH, STOLON.V_HEIGHT);

            if (!STOLON.Config.GetBool("graphics.crt.enable")) DisableShader("Effects\\crt.mgfx");
            ScalingMethod = Enum.Parse<ScalingMethod>(STOLON.Config.GetString("graphics.scaling_method").Replace("_", string.Empty), true);

            STOLON.Debug.Success();
        }

        private RenderTarget2D GetVirtual() => new RenderTarget2D(_graphics, STOLON.V_WIDTH, STOLON.V_HEIGHT);
        private RenderTarget2D GetDesired(Point res) => new RenderTarget2D(_graphics, res.X, res.Y);
        public void UpdateResolution()
        {
            Point newRes = STOLON.Instance.DesiredDimensions;

            float scaleX = (float)newRes.X / STOLON.V_WIDTH;
            float scaleY = (float)newRes.Y / STOLON.V_HEIGHT;

            switch (ScalingMethod)
            {
                case ScalingMethod.None:
                    newRes = STOLON.Instance.GetVirtualDimensions();
                    break;
                case ScalingMethod.Integer:
                    int integerScaleX = (int)scaleX;
                    int integerScaleY = (int)scaleY;
                    newRes.X = integerScaleX * STOLON.V_WIDTH;
                    newRes.Y = integerScaleY * STOLON.V_HEIGHT;
                    break;
                case ScalingMethod.NearestNeighbour:
                    newRes.X = (int)(scaleX * STOLON.V_WIDTH);
                    newRes.Y = (int)(scaleY * STOLON.V_HEIGHT);
                    break;
                default: throw new Exception();
            }

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, newRes.Y, 0);

            _rt1.Dispose();
            _rt1 = GetDesired(newRes);
            _rt2.Dispose();
            _rt2 = GetDesired(newRes);

            foreach (Shader effect in _shaders.Values.Where(e => !e.IsVirtual))
            {
                effect.UpdateResolution(newRes);
            }

            STOLON.Debug.Log($"updated fx pipeline res with new scale '{(newRes.ToVector2() / new Vector2(STOLON.V_WIDTH, STOLON.V_HEIGHT))}'");
        }

        public void DisableShader(string name)
        {
            if (!_shaders[name].IsEnabled)
            {
                STOLON.Debug.Log($"effect '{name}' already disabled.");
                return;
            }
            _shaders[name].IsEnabled = false;
            STOLON.Debug.Log($"disabled effect with name '{name}'.");
        }
        public bool IsEnabled(string name) => _shaders[name].IsEnabled;
        public void EnableShader(string name)
        {
            if (_shaders[name].IsEnabled)
            {
                STOLON.Debug.Log($"effect '{name}' already enabled.");
                return;
            }
            _shaders[name].IsEnabled = true;
            STOLON.Debug.Log($"enabled effect with name '{name}'.");
        }

        #region SCREENSHOT

        public void Screenshot()
        {
            _screenshotPending = true;
            STOLON.Debug.Log("screenshot request submitted.");
        }

        private string ScreenshotFrom(RenderTarget2D virtualFinal)
        {
            STOLON.Debug.Log(">attempting screenshot.");

            STOLON.Instance.GraphicsDevice.SetRenderTarget(null);

            Directory.CreateDirectory("Screenshots");

            STOLON.Debug.Log(">getting screenshot file index.");

            int screenshotIndex = 0;
            for (; true; screenshotIndex++)
                if (!File.Exists($"Screenshots\\sl_screenshot{screenshotIndex}.png")) break;

            STOLON.Debug.Log("<found avalible with id: " + screenshotIndex);

            string path = $"Screenshots\\sl_screenshot{screenshotIndex}.png";

            STOLON.Debug.Log(">reading and flipping screentexture data.");
            Color[] data = new Color[virtualFinal.Width * virtualFinal.Height];
            virtualFinal.GetData(data);

            Color[] rowBuffer = new Color[virtualFinal.Width];
            for (int y = 0; y < virtualFinal.Height / 2; y++)
            {
                int topIndex = y * virtualFinal.Width;
                int bottomIndex = (virtualFinal.Height - y - 1) * virtualFinal.Width;

                Array.Copy(data, topIndex, rowBuffer, 0, virtualFinal.Width);
                Array.Copy(data, bottomIndex, data, topIndex, virtualFinal.Width);
                Array.Copy(rowBuffer, 0, data, bottomIndex, virtualFinal.Width);
            }

            _screenshotCache.SetData(data);
            STOLON.Debug.Success();

            STOLON.Debug.Log(">saving screentexture to file.");
            using (FileStream stream = File.Create(path)) _screenshotCache.SaveAsPng(stream, STOLON.V_WIDTH, STOLON.V_HEIGHT);
            STOLON.Debug.Success();

            STOLON.Debug.Success();
            return path;
        }

        #endregion

        #region SCENE_START_END

        public void BeginScene()
        {
            _graphics.SetRenderTarget(_vrt1);
            _graphics.Clear(STOLON.Instance.Color2);
            BeginBatch();
        }

        public void EndScene()
        {
            _spriteBatch.End();

            RenderTarget2D finalVTarget = _vrt1;

            foreach (Shader shader in _shaders.Values.Where(e => e.IsVirtual && e.IsEnabled)) // apply virtual effects.
            {
                _graphics.SetRenderTarget(_vrt2);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _samplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Effect);
                _spriteBatch.Draw(_vrt1, Vector2.Zero, Color.White);
                _spriteBatch.End();

                finalVTarget = _vrt2;
                (_vrt1, _vrt2) = (_vrt2, _vrt1);
            }

            _graphics.SetRenderTarget(_rt1); // draw and upscale to normal sized rt.
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _samplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            _spriteBatch.Draw(finalVTarget, new Rectangle(Point.Zero, _rt1.Bounds.Size), Color.White);
            _spriteBatch.End();

            RenderTarget2D finalTarget = _rt1;

            foreach (Shader shader in _shaders.Values.Where(e => !e.IsVirtual && e.IsEnabled)) // apply normal effects.
            {
                _graphics.SetRenderTarget(_rt2);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _samplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Effect);
                _spriteBatch.Draw(_rt1, Vector2.Zero, Color.White);
                _spriteBatch.End();

                finalTarget = _rt2;
                (_rt1, _rt2) = (_rt2, _rt1);
            }

            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _samplerState, DepthStencilState.None, RasterizerState.CullNone, null, _invertYMatrix);
            _spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spritebatchStarted = false;

            if (_screenshotPending)
            {
                ScreenshotFrom(finalVTarget);
                _screenshotPending = false;
            }
        }

        #endregion

        public void EndBatch()
        {
            _spriteBatch.End();
            _spritebatchStarted = false;
        }

        public void BeginBatch(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Matrix? transformMatrix = null)
        {
            //if (_spritebatchStarted) _spriteBatch.End();
            _spriteBatch.Begin(sortMode, blendState, samplerState ?? _samplerState, depthStencilState, rasterizerState, null, transformMatrix);
            _spritebatchStarted = true;
        }
        private readonly RasterizerState _scissorRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        private readonly RasterizerState _defaultRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = false
        };
        public void ResetScissorArea()
        {
            if (!_scissorEnabled) return;

            EndBatch();

            _spriteBatch.GraphicsDevice.ScissorRectangle = STOLON.Instance.GetVirtualBounds();
            BeginBatch(rasterizerState: _defaultRasterizerState);
            _scissorEnabled = false;
        }
        public void SetScissorArea(Rectangle newArea)
        {
            EndBatch();

            _spriteBatch.GraphicsDevice.ScissorRectangle = newArea;
            BeginBatch(rasterizerState: _scissorRasterizerState);
            _scissorEnabled = true;
        }
        public SpriteEffects InvertY(SpriteEffects effect) => effect ^ SpriteEffects.FlipVertically;

        #region DRAW_FUNCTIONS

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
        private Rectangle? TranslateSourceRectangle(Rectangle? sourceRectangle)
            => sourceRectangle == null ? null : new Rectangle(sourceRectangle.Value.Location + new Point(0, sourceRectangle.Value.Height), sourceRectangle.Value.Size);

        public void DrawLine(Vector2 point1, Vector2 point2, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => DrawLine(point1.X, point1.Y, point2.X, point2.X, color, thickness, layerDepth);
        public void DrawLine(float x1, float y1, float x2, float y2, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => _spriteBatch.DrawLine(x1, y1, x2, y2, color ?? Color.White, thickness, layerDepth);

        public void DrawVerticalLine(Vector2 point1, float amountUp = 1000f, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => DrawVerticalLine(point1.X, point1.Y, amountUp, color, thickness, layerDepth);
        public void DrawVerticalLine(float x1, float y1, float amountUp = 1000f, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => DrawLine(x1, y1, x1, y1 + amountUp, color, thickness, layerDepth);

        public void DrawHorizontalLine(Vector2 point1, float amountLeft = 1000f, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => DrawHorizontalLine(point1.X, point1.Y, amountLeft, color, thickness, layerDepth);
        public void DrawHorizontalLine(float x1, float y1, float amountLeft = 1000f, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => DrawLine(x1, y1, x1 + amountLeft, y1, color, thickness, layerDepth);

        public void DrawLine(Line line, Color color, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => _spriteBatch.DrawLine(line.Start.X, line.Start.Y, line.End.X, line.End.Y, color, thickness, layerDepth);

        public void DrawRectangle(Rectangle rectangle, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
            => _spriteBatch.DrawRectangle(rectangle, color ?? Color.White, thickness, layerDepth);
        //public void DrawRectangle(RectangleF rectangle, Color? color = null, float thickness = Interface.LINE_WIDTH, float layerDepth = 0f)
        //    => _spriteBatch.DrawRectangle(rectangle, color, thickness, layerDepth);

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

        public void Draw(IGraphic graphic)
        {
            graphic.Draw(this);
        }

        #endregion


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
                foreach (Shader shader in _shaders.Values) (shader as IDisposable)?.Dispose();
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
