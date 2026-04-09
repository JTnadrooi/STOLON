using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public enum ScalingMethod
    {
        None,
        Integer,
        NearestNeighbour,
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class DrawingContext : IDisposable
    {
        public ReadOnlyDictionary<string, Shader> Shaders { get; }

        /// <summary>
        /// Gets the <see cref="Matrix"/> used for inverting coordinates. Drawing is also done using the <see cref="SpriteEffects.FlipVertically"/> sprite effect.<br/>
        /// Used for transforming <see cref="MouseState.Position"/> and passed to secondary <see cref="SpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, Matrix?)"/> calls. 
        /// When using it for <see cref="SpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, Matrix?)"/>, don't forget to use <see cref="SpriteEffects.FlipVertically"/>.
        /// </summary>
        public Matrix InvertYMatrix => _invertYMatrix;

        /// <summary>
        /// Note everything drawn to this will be inverted. Please use the extension methods or call <see cref="InvertY(SpriteEffects)"/> on the input <see cref="SpriteEffects"/> enum.
        /// </summary>
        public SpriteBatch SpriteBatch { get; }

        public bool IsSpriteBatchStarted => _isSpriteBatchActive;

        public ScalingMethod ScalingMethod { get; }

        public float Scale { get; private set; }

        internal Vector2 GameWindowDrawOffsetWithCorrectedY { get; private set; }

        #region BATCH_PROPERTIES

        private BlendState? _blendState;
        public BlendState? BlendState
        {
            get => _blendState;
            set
            {
                if (_blendState != value)
                {
                    _blendState = value;
                    UpdateDrawingParameters();
                }
            }
        }

        private SamplerState? _samplerState;
        public SamplerState? SamplerState
        {
            get => _samplerState;
            set
            {
                if (_samplerState != value)
                {
                    _samplerState = value;
                    UpdateDrawingParameters();
                }
            }
        }

        private DepthStencilState? _depthStencilState;
        public DepthStencilState? DepthStencilState
        {
            get => _depthStencilState;
            set
            {
                if (_depthStencilState != value)
                {
                    _depthStencilState = value;
                    UpdateDrawingParameters();
                }
            }
        }

        private RasterizerState? _rasterizerState;

        private Matrix? _transformMatrix;
        public Matrix? TransformMatrix
        {
            get => _transformMatrix;
            set
            {
                if (_transformMatrix != value)
                {
                    _transformMatrix = value;
                    UpdateDrawingParameters();
                }
            }
        }

        private SpriteSortMode _sortMode;
        public SpriteSortMode SortMode
        {
            get => _sortMode;
            set
            {
                if (_sortMode != value)
                {
                    _sortMode = value;
                    UpdateDrawingParameters();
                }
            }
        }

        private Rectangle? _scissorArea;
        public Rectangle? ScissorArea
        {
            get => _scissorArea;
            set
            {
                if (_scissorArea != value)
                {
                    _scissorArea = value;
                    UpdateDrawingParameters();
                }
            }
        }

        #endregion

        private static readonly RasterizerState s_scissorRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        private static readonly RasterizerState s_defaultRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = false
        };

        private Matrix _invertYMatrix;

        private Texture2DAtlas _ditherAtlas;
        private Texture2D _screenshotCache;
        private bool _screenshotPending;
        private bool _disposedValue;
        private readonly GraphicsDevice _graphics;
        private readonly Dictionary<string, Shader> _shaderDict;
        private RenderTarget2D _vrt1;
        private RenderTarget2D _vrt2;
        private RenderTarget2D _rt1;
        private RenderTarget2D _rt2;
        private bool _isSpriteBatchActive;
        private readonly SamplerState _defaultSamplerState;

        public const int DITHER_FRAME_COUNT = 5;
        public const int DITHER_TEXTURE_SIZE = 32;

        private readonly IRichLogger _logger;
        private readonly IConfiguration _config;
        private readonly IFont2DCollection _fonts;
        private readonly ITexture2DCollection _textures;
        private readonly IEnumerable<Shader> _shaders;
        private readonly IInputManager _input;

        public DrawingContext(
            IRichLogger logger,
            IConfiguration config,
            IFont2DCollection fonts,
            ITexture2DCollection textures,
            IInputManager input,
            IEnumerable<Shader> shaders)
        {
            _logger = logger;
            _config = config;
            _fonts = fonts;
            _textures = textures;
            _shaders = shaders;
            _input = input;

            _logger.Log(">[s]initialising drawing context");

            SpriteBatch = new SpriteBatch(STOLON.Instance.GraphicsDevice);

            _graphics = STOLON.Instance.GraphicsDevice;

            _vrt1 = GetVirtual();
            _vrt2 = GetVirtual();
            _rt1 = GetDesired(STOLON.Instance.DesiredDimensions);
            _rt2 = GetDesired(STOLON.Instance.DesiredDimensions);

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, STOLON.Instance.DesiredDimensions.Y, 0);
            _defaultSamplerState = SamplerState.PointClamp;

            _shaderDict = new Dictionary<string, Shader>();
            Shaders = _shaderDict.AsReadOnly();

            _logger.Log(">searching for effects");
            foreach (Shader shader in _shaders)
            {
                _logger.Log($"found effect with name '{shader.Effect.Name}'.");
                _shaderDict.Add(shader.Effect.Name, shader);
            }
            _logger.Success();

            _ditherAtlas = Texture2DAtlas.Create("dither_tile", _textures["UI\\dither_sheet-128"], DITHER_TEXTURE_SIZE, DITHER_TEXTURE_SIZE);
            _screenshotCache = new Texture2D(STOLON.Instance.GraphicsDevice, STOLON.VWidth, STOLON.VHeight);

            if (!_config.GetBool("graphics.crt.enable")) DisableShader("Effects\\crt.mgfx");
            ScalingMethod = Enum.Parse<ScalingMethod>(_config.GetString("graphics.scaling_method").Replace("_", string.Empty), true);
            Scale = STOLON.Instance.DesiredDimensions.X / STOLON.VWidth;

            SetDrawingParameters(); // just sets defaults, doesnt do anything with the spritebatch if its not started yet. (it isn't right now)

            _logger.Success();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RenderTarget2D GetVirtual() => new RenderTarget2D(_graphics, STOLON.VWidth, STOLON.VHeight);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RenderTarget2D GetDesired(Point res) => new RenderTarget2D(_graphics, res.X, res.Y);

        private void UpdateDrawingParameters()
        {
            bool isSpriteBatchInitiallyActive = _isSpriteBatchActive; // because it will not be active after the EndBatch call so this has to be stored for a sec.

            if (isSpriteBatchInitiallyActive) EndBatch();

            if (_scissorArea.HasValue) // I cannot put this before the EndBatch call for reasons unknown.
            {
                SpriteBatch.GraphicsDevice.ScissorRectangle = _scissorArea.Value;
                _rasterizerState = s_scissorRasterizerState;
            }
            else
            {
                SpriteBatch.GraphicsDevice.ScissorRectangle = STOLON.Instance.GetVirtualBounds();
                _rasterizerState = s_defaultRasterizerState;
            }

            if (isSpriteBatchInitiallyActive) BeginBatch();

            // if not started, the next begin call will handle it.
        }

        ///// <summary>
        ///// Reapplies drawing parameters normally passed to the <see cref="SpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, Matrix?)"/> method.
        ///// Call this after the <see cref="SpriteBatch.End"/> call of a secondary spritebatch when using it with differing drawing parameters.
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void ReApplyDrawingParameters() // why this is needed is still beyond me.
        //{
        //    UpdateDrawingParameters();
        //}

        public void SetDrawingParameters( // for bulk changes.
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState? blendState = null,
            SamplerState? samplerState = null,
            DepthStencilState? depthStencilState = null,
            Rectangle? scissorArea = null,
            Matrix? transformMatrix = null,
            bool forceUpdate = false)
        {
            bool changed = false;

            samplerState ??= SamplerState.PointClamp;

            if (_sortMode != sortMode)
            {
                _sortMode = sortMode;
                changed = true;
            }

            if (_blendState != blendState)
            {
                _blendState = blendState;
                changed = true;
            }

            if (_samplerState != samplerState)
            {
                _samplerState = samplerState;
                changed = true;
            }

            if (_depthStencilState != depthStencilState)
            {
                _depthStencilState = depthStencilState;
                changed = true;
            }

            if (_scissorArea != scissorArea)
            {
                _scissorArea = scissorArea;
                changed = true;
            }

            if (_transformMatrix != transformMatrix)
            {
                _transformMatrix = transformMatrix;
                changed = true;
            }

            if (changed || forceUpdate)
            {
                UpdateDrawingParameters();
            }
        }

        public void UpdateResolution()
        {
            Point newRes = STOLON.Instance.DesiredDimensions;
            float scaleX = (float)newRes.X / STOLON.VWidth;
            float scaleY = (float)newRes.Y / STOLON.VHeight;

            switch (ScalingMethod)
            {
                case ScalingMethod.None:
                    newRes = STOLON.Instance.GetVirtualDimensions();
                    break;
                case ScalingMethod.Integer:
                    newRes = new Point((int)(scaleX) * STOLON.VWidth, (int)(scaleY) * STOLON.VHeight);
                    break;
                case ScalingMethod.NearestNeighbour:
                    newRes = new Point((int)(scaleX * STOLON.VWidth), (int)(scaleY * STOLON.VHeight));
                    break;
                default:
                    throw new InvalidOperationException("Unknown scaling method.");
            }

            _invertYMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, newRes.Y, 0);

            _rt1.Dispose();
            _rt1 = GetDesired(newRes);
            _rt2.Dispose();
            _rt2 = GetDesired(newRes);

            foreach (Shader effect in _shaderDict.Values.Where(e => !e.IsVirtual))
            {
                effect.UpdateResolution(newRes);
            }

            Scale = newRes.X / (float)STOLON.VWidth;

            _logger.Log($"updated fx pipeline res with new scale '{Scale}'");
        }

        public void RegisterDraw<TElement>(TElement element, in Rectangle hitbox) where TElement : class, IDrawable
#pragma warning disable CS0612 
            => _input.RegisterDraw(element, hitbox);
#pragma warning restore CS0612

        public void DisableShader(string name)
        {
            if (!_shaderDict[name].IsEnabled)
            {
                _logger.Log($"effect '{name}' already disabled.");
                return;
            }
            _shaderDict[name].IsEnabled = false;
            _logger.Log($"disabled effect with name '{name}'.");
        }

        public bool IsEnabled(string name) => _shaderDict[name].IsEnabled;

        public void EnableShader(string name)
        {
            if (_shaderDict[name].IsEnabled)
            {
                _logger.Log($"effect '{name}' already enabled.");
                return;
            }
            _shaderDict[name].IsEnabled = true;
            _logger.Log($"enabled effect with name '{name}'.");
        }

        #region SCREENSHOT

        public void Screenshot()
        {
            _screenshotPending = true;
            _logger.Log("screenshot request submitted.");
        }

        private string ScreenshotFrom(RenderTarget2D virtualFinal)
        {
            _logger.Log(">attempting screenshot.");

            STOLON.Instance.GraphicsDevice.SetRenderTarget(null);

            Directory.CreateDirectory("Screenshots");

            _logger.Log(">getting screenshot file index.");

            int screenshotIndex = 0;
            for (; true; screenshotIndex++)
                if (!File.Exists($"Screenshots\\sl_screenshot{screenshotIndex}.png")) break;

            _logger.Log("<found avalible with id: " + screenshotIndex);

            string path = $"Screenshots\\sl_screenshot{screenshotIndex}.png";

            _logger.Log(">reading and flipping screentexture data.");
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
            _logger.Success();

            _logger.Log(">saving screentexture to file.");
            using (FileStream stream = File.Create(path)) _screenshotCache.SaveAsPng(stream, STOLON.VWidth, STOLON.VHeight);
            _logger.Success();

            _logger.Success();
            return path;
        }

        #endregion

        #region SCENE_START_END

        public void BeginScene()
        {
            _graphics.SetRenderTarget(_vrt1);
            _graphics.Clear(STOLON.Color2);
            BeginBatch();
        }

        public void EndScene()
        {
            SpriteBatch.End();

            RenderTarget2D finalVTarget = _vrt1;

            foreach (Shader shader in _shaderDict.Values.Where(e => e.IsVirtual && e.IsEnabled)) // apply virtual effects.
            {
                _graphics.SetRenderTarget(_vrt2);
                _graphics.Clear(Color.LightSeaGreen);

                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Effect);
                SpriteBatch.Draw(_vrt1, Vector2.Zero, Color.White);
                SpriteBatch.End();

                finalVTarget = _vrt2;
                (_vrt1, _vrt2) = (_vrt2, _vrt1);
            }

            _graphics.SetRenderTarget(_rt1); // draw and upscale to normal sized rt.
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            SpriteBatch.Draw(finalVTarget, new Rectangle(Point.Zero, _rt1.Bounds.Size), Color.White);
            SpriteBatch.End();

            RenderTarget2D finalTarget = _rt1;

            foreach (Shader shader in _shaderDict.Values.Where(e => !e.IsVirtual && e.IsEnabled)) // apply normal effects.
            {
                _graphics.SetRenderTarget(_rt2);
                _graphics.Clear(Color.LightSeaGreen);

                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Effect);
                SpriteBatch.Draw(_rt1, Vector2.Zero, Color.White);
                SpriteBatch.End();

                finalTarget = _rt2;
                (_rt1, _rt2) = (_rt2, _rt1);
            }

            int offsetX = (STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferWidth - finalTarget.Width) / 2;
            int offsetY = (STOLON.Instance.GraphicsDeviceManager.PreferredBackBufferHeight - finalTarget.Height) / -2; // because this does not get inverted
            GameWindowDrawOffsetWithCorrectedY = new Vector2(offsetX, -offsetY);

            _graphics.SetRenderTarget(null);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, _defaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, _invertYMatrix);
            SpriteBatch.Draw(finalTarget, new Vector2(offsetX, offsetY), Color.White);
            SpriteBatch.End();

            _isSpriteBatchActive = false;

            if (_screenshotPending)
            {
                ScreenshotFrom(finalVTarget);
                _screenshotPending = false;
            }
        }

        #endregion

        public void EndBatch()
        {
            SpriteBatch.End();
            _isSpriteBatchActive = false;
        }

        public void BeginBatch()
        {
            //if (_spritebatchStarted) _spriteBatch.End();

            SpriteBatch.Begin(SortMode, BlendState, SamplerState, DepthStencilState, _rasterizerState, null, TransformMatrix);
            _isSpriteBatchActive = true;
        }


        public SpriteEffects InvertY(SpriteEffects effect) => effect ^ SpriteEffects.FlipVertically;

        #region DRAW_FUNCTIONS

        public void DrawPoint(Vector2 position, Color? color = null, int size = 1)
        {
            SpriteBatch.DrawPoint(position, color ?? Color.White, size);
        }

        public void DrawArea(Rectangle destinationRectangle, Color? color = null)
            => Draw(_textures.Pixel, destinationRectangle, color: color);

        public void Draw(Texture2D texture, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(texture, position, new Vector2(scale), rotation, origin, sourceRectangle, color, effects, layerDepth);
        public void Draw(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            SpriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale, InvertY(effects), layerDepth);
        }
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color ?? Color.White, 0f, Vector2.Zero, InvertY(effects), layerDepth);
        }

        public void Draw(Texture2DRegion region, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(region, position, new Vector2(scale), rotation, origin, color, effects, layerDepth);
        public void Draw(Texture2DRegion region, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(region.Texture, position, scale, rotation, origin, region.Bounds, color, effects, layerDepth);
        public void Draw(Texture2DRegion region, Rectangle destinationRectangle, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => Draw(region.Texture, destinationRectangle, region.Bounds, color, effects, layerDepth);

        //public void Draw(Texture2DRegion texture, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        //    => Draw(texture, position, new Vector2(scale), rotation, origin, sourceRectangle, color, effects, layerDepth);
        //public void Draw(Texture2DRegion texture, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        //{
        //    SpriteBatch.Draw(texture, position, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale, InvertY(effects), layerDepth, null);
        //}
        //public void Draw(Texture2DRegion texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        //{
        //    Draw(texture, ResolveFromDestinationRectangle(destinationRectangle, texture.Bounds, out Vector2 scale), scale, 0, null, sourceRectangle, color, effects, layerDepth); // FIX
        //}

        //private Vector2 ResolveFromDestinationRectangle(in Rectangle destinationRectangle, in Rectangle textureBounds, out Vector2 scale)
        //{
        //    scale = new Vector2(destinationRectangle.Width / (float)textureBounds.Width, destinationRectangle.Height / (float)textureBounds.Height);

        //    return destinationRectangle.Location.ToVector2();
        //}

        private Rectangle GetDestinationRectangle(Texture2D texture, Vector2 position, float scale)
            => GetDestinationRectangle(texture, position, new Vector2(scale));
        private Rectangle GetDestinationRectangle(Texture2D texture, Vector2 position, Vector2? scale = null)
            => new Rectangle(position.ToPoint(), (texture.Bounds.Size.ToVector2() * (scale ?? Vector2.One)).ToPoint());
        private Rectangle? TranslateSourceRectangle(Rectangle? sourceRectangle)
            => sourceRectangle == null ? null : new Rectangle(sourceRectangle.Value.Location + new Point(0, sourceRectangle.Value.Height), sourceRectangle.Value.Size);

        public void DrawLine(Line line, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawLine(line.Start.X, line.Start.Y, line.End.X, line.End.Y, color, thickness, layerDepth);
        public void DrawLine(Vector2 point1, Vector2 point2, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawLine(point1.X, point1.Y, point2.X, point2.X, color, thickness, layerDepth);
        public void DrawLine(float x1, float y1, float x2, float y2, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => SpriteBatch.DrawLine(x1, y1, x2, y2, color ?? Color.White, thickness, layerDepth);

        public void DrawVerticalLine(Vector2 point1, float amountUp = 1000f, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawVerticalLine(point1.X, point1.Y, amountUp, color, thickness, layerDepth);
        public void DrawVerticalLine(float x1, float y1, float amountUp = 1000f, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawLine(x1, y1, x1, y1 + amountUp, color, thickness, layerDepth);

        public void DrawHorizontalLine(Vector2 point1, float amountLeft = 1000f, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawHorizontalLine(point1.X, point1.Y, amountLeft, color, thickness, layerDepth);
        public void DrawHorizontalLine(float x1, float y1, float amountLeft = 1000f, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => DrawLine(x1, y1, x1 + amountLeft, y1, color, thickness, layerDepth);

        public void DrawLine(Line line, Color color, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => SpriteBatch.DrawLine(line.Start.X, line.Start.Y, line.End.X, line.End.Y, color, thickness, layerDepth);

        public void DrawRectangle(Rectangle rectangle, Color? color = null, float thickness = Interface.LineWidth, float layerDepth = 0f)
            => SpriteBatch.DrawRectangle(rectangle, color ?? Color.White, thickness, layerDepth);
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

        #endregion


        //public void DrawStringOutline(GameFont font, string text, Vector2 position, int marginX, int marginY, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, bool background = false, int lineWidth = UserInterface.LINE_WIDTH)
        //{
        //    Point nameDimensions = font.FastMeasure(text).ToPoint();
        //    Rectangle bounds = new Rectangle(position.ToPoint(), nameDimensions + new Point((int)(marginX * 2f), (int)(marginY * 2f)));
        //    Vector2 calcPos = (Centering.Center(nameDimensions, bounds) + new Vector2(1, 0)).PixelLock();
        //    DrawRectangle(bounds, color ?? Color.White, lineWidth);
        //    DrawString(font, text, calcPos, color: color, effects: effects, layerDepth: layerDepth);
        //}

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                SpriteBatch.Dispose();
                _vrt1.Dispose();
                _vrt2.Dispose();
                _rt1.Dispose();
                _rt2.Dispose();
                foreach (Shader shader in _shaderDict.Values) (shader as IDisposable)?.Dispose();
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
