using Betwixt;
using System.Numerics;



namespace STOLON
{
    public class OverlayManager : IComponent, IOverlayManager, ISingletonDependency
    {
        private Dictionary<string, IOverlay> _overlayDict;
        private List<string> _initialized;

        private readonly IRichLogger _logger;
        private readonly IEnumerable<IOverlay> _overlays;

        public OverlayManager(IRichLogger logger, IEnumerable<IOverlay> overlays)
        {
            _logger = logger;
            _overlays = overlays;

            _overlayDict = new Dictionary<string, IOverlay>();
            _initialized = new List<string>();

            _logger.Log(">searching for overlays");
            foreach (IOverlay overlay in overlays)
            {
                _logger.Log($"found overlay with id '{overlay.Id}\".");
                AddOverlay(overlay);
            }
            _logger.Success();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            _logger.Log(">adding overlay of id " + overlay.Id + ".");
            _overlayDict.Add(overlay.Id, overlay);
            _logger.Success();
        }

        public void RemoveOverlay(string overlayId)
        {
            _logger.Log(">removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            _overlayDict.Remove(overlayId);
            _logger.Success();
        }

        public void Activate(string overlayId, params object?[] args)
        {

            if (!_initialized.Contains(overlayId))
            {
                _logger.Log(">[s]activating overlay of id " + overlayId + ".");
                _overlayDict[overlayId].Initialize(this, args);
                _initialized.Add(overlayId);
                _logger.Success();
            }
        }

        public bool IsActive(IOverlay overlay) => IsActive(overlay.Id);
        public bool IsActive(string overlayId)
        {
            return _initialized.Contains(overlayId);
        }

        public void Deactivate(string overlayId) // ensure
        {
            if (_initialized.Contains(overlayId))
            {
                _logger.Log(">deactivating overlay of id " + overlayId + ".");
                _overlayDict[overlayId].Reset();
                _logger.Success();
            }
            _initialized.Remove(overlayId);
        }

        public void Update(int elapsedMilliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < _initialized.Count; i++) // for all initialized overlays
            {
                overlay = _overlayDict[_initialized[i]];
                overlay.Update(elapsedMilliseconds);
                if (overlay.Ended)
                {
                    _logger.Log(">deactivating and resetting ended overlay of id " + overlay.Id + ".");
                    Deactivate(overlay.Id);
                    _logger.Success();
                }
            }
        }
        public void Draw(DrawingContext drawingContext)
        {
            for (int i = 0; i < _initialized.Count; i++)
            {
                _overlayDict[_initialized[i]].Draw(drawingContext);
            }
        }
    }

    public interface IOverlay
    {
        public void Initialize(OverlayManager overlayer, params object?[] args);
        public void Update(int elapsedMilliseconds);
        public void Draw(DrawingContext drawingContext);
        public void Reset();

        public string Id { get; }
        public bool Ended { get; }
    }

    public class LoadOverlay : IOverlay
    {
        public string Id => "loading";
        public bool Ended { get; private set; }

        private Texture2D lineTexture;

        private float _rotation;
        private float _rotationSpeed;
        private Vector2 _pos;
        private float _scale;

        public LoadOverlay(ITexture2DCollection textures)
        {
            lineTexture = textures.GetReference("loading1");
            _rotation = 0f;
            _scale = 0.20f;
            _rotationSpeed = 40f;

            _pos = new Vector2(STOLON.V_WIDTH, STOLON.V_HEIGHT) + new Vector2(-lineTexture.Width, -lineTexture.Height) * _scale;

        }

        public void Initialize(OverlayManager overlayer, params object?[] args)
        {
            _pos = (Vector2)((args.Length > 0 ? args[0] : null) ?? _pos);
        }

        public void Reset()
        {

        }

        public void Update(int elapsedMilliseconds)
        {
            _rotation += _rotationSpeed;
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(lineTexture, _pos, _scale, _rotation / 360f, new Vector2(lineTexture.Width / 2f, lineTexture.Height / 2f));
            //drawingContext.DrawCircle(pos, scale * lineTexture.Width * 0.8f, 15, Color.White, 2);
        }
    }

    public class TransitionDitherOverlay : IOverlay
    {
        public string Id => "transition_dither";

        public bool Ended => _ended;

        private Texture2D _ditherTexture;
        private const int FRAME_PIXELS_TO_REMOVE = 11150 / RESOLUTION; // Number of pixels to turn transparent each frame
        private const int RESOLUTION = 2;
        private Color[] _pixelData; // Holds the pixel data for the dither texture
        private Random _random;
        private GraphicsDevice _graphicsDevice;
        private bool _ended;
        private int _resolution;
        private int _width;
        private int _height;
        private Tweener<float> _tweener;

        private readonly ICachedAudioResourceCollection _audio;
        private readonly IAudioEngine _audioEngine;

        public TransitionDitherOverlay(ICachedAudioResourceCollection audio, IAudioEngine audioEngine)
        {
            _audio = audio;
            _audioEngine = audioEngine;

            this._graphicsDevice = STOLON.Instance.GraphicsDevice;
            this._resolution = RESOLUTION;
            _random = new Random();


            _tweener = new Tweener<float>(1, FRAME_PIXELS_TO_REMOVE, 5f, Ease.Expo.In);
            _height = STOLON.V_HEIGHT / RESOLUTION;
            _width = STOLON.V_WIDTH / RESOLUTION;

            _ditherTexture = null!;
            _pixelData = null!;
            ResetTexture();
        }


        public void Initialize(OverlayManager overlayer, params object?[] args)
        {
            _audioEngine.Play(_audio["randomize_4"]);
        }

        public void ResetTexture()
        {
            _ditherTexture = new Texture2D(_graphicsDevice, _width, _height);
            _pixelData = new Color[_width * _height];
            for (int i = 0; i < _pixelData.Length; i++) _pixelData[i] = Color.White;
            _ditherTexture.SetData(_pixelData);
        }

        public void Reset()
        {
            _tweener.Reset();
            ResetTexture();
        }

        public void Update(int elapsedMilliseconds)
        {
            int removedPixels = 0;
            int dullPixels = 0;

            _tweener.Update(elapsedMilliseconds / 1000f);

            while (removedPixels < _tweener.Value)
            {
                int index = _random.Next(_pixelData.Length);
                if (_pixelData[index] == Color.White)
                {
                    _pixelData[index] = Color.Transparent;
                    removedPixels++;
                }
                else dullPixels++;
                if (dullPixels > 100000)
                {
                    _ended = true;
                    return;
                }
            }
            _ditherTexture.SetData(_pixelData);
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(_ditherTexture, Vector2.Zero, (float)_resolution);
        }
    }

    public class TransitionOverlay : IOverlay
    {
        public string Id => "transition";

        public bool Ended { get; private set; }

        private Rectangle _area;
        private OverlayManager _overlayer;
        private Tweener<float> _tweener;
        private string _text;

        private Rectangle _drawArea;
        private float _heightCoefficient;
        private Vector2 _textPos;
        private Action _action;

        private bool _hasHitMax;

        private readonly IFont2DCollection _fonts;

        public TransitionOverlay(IFont2DCollection fonts)
        {
            _fonts = fonts;

            _area = Rectangle.Empty;
            _drawArea = Rectangle.Empty;
            _overlayer = null!; // I know I know
            _tweener = null!; // yupyup
            Ended = false;
            _text = string.Empty;
            _textPos = Vector2.Zero;
            _action = () => { };
        }

        public void Update(int elapsedMilliseconds)
        {
            int desiredHeight = _area.Height;
            if (!_hasHitMax && _heightCoefficient > 0.999f)
            {
                _tweener = new Tweener<float>(1f, 0f, Duration / 1000f / 2, Ease.Sine.In);
                _action();
                _tweener.Start();
                _hasHitMax = true;
            }
            Ended = _hasHitMax && _heightCoefficient < 0.001f;

            _tweener.Update(elapsedMilliseconds / 1000f);

            _heightCoefficient = _tweener.Value;

            _drawArea = new Rectangle(_area.Location, new Point(_area.Width, (int)(desiredHeight * _heightCoefficient)));
            _textPos = Centering.Center((_fonts.Small.FastMeasure(_text) * TextSizeMod).ToPoint(), _drawArea);
            _textPos = new Vector2(_textPos.X, Math.Min(_textPos.Y, _drawArea.Height - _fonts.Small.Dimensions.Y * TextSizeMod));

            Centering.OnPixel(ref _textPos);
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawArea(_drawArea, Color.Black);
            drawingContext.DrawRectangle(_drawArea, Color.White);
            drawingContext.DrawString(_fonts.Small, _text, _textPos, TextSizeMod);
        }

        public void Initialize(OverlayManager overlayer, params object?[] args)
        {
            Ended = false;
            _tweener = new Tweener<float>(0f, 1f, Duration / 1000f / 2, Ease.Sine.Out);
            _area = (Rectangle)((args.Length > 0 ? args[0] : null) ?? STOLON.Instance.GetVirtualBounds());
            _text = (string)(args[2] ?? string.Empty);
            _action = (Action)(args[1]! ?? _action);

            _tweener.Start();

            this._overlayer = overlayer;
        }

        public void Reset()
        {
            _tweener.Stop();
            _tweener.Reset();
            _hasHitMax = false;
            _heightCoefficient = 0f;
        }

        public static int Duration => 4000;
        public static float TextSizeMod => 3;
    }
}
