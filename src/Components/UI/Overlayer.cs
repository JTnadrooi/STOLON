using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Betwixt;
using MonoGame.Extended;


using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using MonoGame.Extended.Tweening;
using System.Reflection;
using System.Linq;



namespace STOLON
{
    public class OverlayEngine : GameComponent
    {
        private Dictionary<string, IOverlay> _overlays;
        private List<string> _initialized;

        public OverlayEngine() : base(GameEnvironment.Instance)
        {
            _overlays = new Dictionary<string, IOverlay>();
            _initialized = new List<string>();

            STOLON.Debug.Log(">searching for overlays");
            IOverlay[] overlays = STOLON.Scan<IOverlay>();
            foreach (IOverlay overlay in overlays)
            {
                STOLON.Debug.Log($"found overlay with id \"{overlay.Id}\".");
                AddOverlay(overlay);
            }
            STOLON.Debug.Success();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            STOLON.Debug.Log(">adding overlay of id " + overlay.Id + ".");
            _overlays.Add(overlay.Id, overlay);
            STOLON.Debug.Success();
        }

        public void RemoveOverlay(string overlayId)
        {
            STOLON.Debug.Log(">removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            _overlays.Remove(overlayId);
            STOLON.Debug.Success();
        }

        public void Activate(string overlayId, params object?[] args)
        {

            if (!_initialized.Contains(overlayId))
            {
                STOLON.Debug.Log(">[s]activating overlay of id " + overlayId + ".");
                _overlays[overlayId].Initialize(this, args);
                _initialized.Add(overlayId);
                STOLON.Debug.Success();
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
                STOLON.Debug.Log(">deactivating overlay of id " + overlayId + ".");
                _overlays[overlayId].Reset();
                STOLON.Debug.Success();
            }
            _initialized.Remove(overlayId);
        }

        public override void Update(int elapsedMiliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < _initialized.Count; i++) // for all initialized overlays
            {
                overlay = _overlays[_initialized[i]];
                overlay.Update(elapsedMiliseconds);
                if (overlay.Ended)
                {
                    STOLON.Debug.Log(">deactivating and resetting ended overlay of id " + overlay.Id + ".");
                    Deactivate(overlay.Id);
                    STOLON.Debug.Success();
                }
            }
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            for (int i = 0; i < _initialized.Count; i++)
            {
                _overlays[_initialized[i]].Draw(drawingContext, elapsedMiliseconds);
            }
            base.Draw(drawingContext, elapsedMiliseconds);
        }

        public static OverlayEngine Engine => STOLON.Environment.Overlayer;
    }

    public interface IOverlay
    {
        public void Initialize(OverlayEngine overlayer, params object?[] args);
        public void Update(int elapsedMiliseconds);
        public void Draw(DrawingContext drawingContext, int elapsedMiliseconds);
        public void Reset();

        public string Id { get; }
        public bool Ended { get; }
    }
    public class LoadOverlay : IOverlay
    {
        public string Id => "loading";
        public bool Ended { get; private set; }

        private GameTexture lineTexture;

        private float _rotation;
        private float _rotationSpeed;
        private Vector2 _pos;
        private float _scale;

        public LoadOverlay()
        {
            lineTexture = STOLON.Textures.GetReference("loading1");
            _rotation = 0f;
            _scale = 0.20f;
            _rotationSpeed = 40f;

            _pos = new Vector2(STOLON.V_WIDTH, STOLON.V_HEIGHT) + new Vector2(-lineTexture.Width, -lineTexture.Height) * _scale;

        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            _pos = (Vector2)((args.Length > 0 ? args[0] : null) ?? _pos);
        }

        public void Reset()
        {

        }

        public void Update(int elapsedMiliseconds)
        {
            _rotation += _rotationSpeed;
        }

        public void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            drawingContext.Draw(lineTexture, _pos, _scale, _rotation / 360f, new Vector2(lineTexture.Width / 2f, lineTexture.Height / 2f));
            //drawingContext.DrawCircle(pos, scale * lineTexture.Width * 0.8f, 15, Color.White, 2);
        }
    }
    public class TransitionDitherOverlay : IOverlay
    {
        public string Id => "transitionDither";

        public bool Ended => _ended;

        private GameTexture _ditherTexture;
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

        public TransitionDitherOverlay()
        {
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


        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            STOLON.Audio.Play(STOLON.Audio.Library["randomize4"]);
        }

        public void ResetTexture()
        {
            _ditherTexture = new GameTexture(_graphicsDevice, _width, _height);
            _pixelData = new Color[_width * _height];
            for (int i = 0; i < _pixelData.Length; i++) _pixelData[i] = Color.White;
            _ditherTexture.SetColorData(_pixelData);
        }

        public void Reset()
        {
            _tweener.Reset();
            ResetTexture();
        }

        public void Update(int elapsedMiliseconds)
        {
            int removedPixels = 0;
            int dullPixels = 0;

            _tweener.Update(elapsedMiliseconds / 1000f);

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
            _ditherTexture.SetColorData(_pixelData);
        }

        public void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            drawingContext.Draw(_ditherTexture, Vector2.Zero, (float)_resolution);
        }
    }
    public class TransitionOverlay : IOverlay
    {
        public string Id => "transition";

        public bool Ended { get; private set; }

        private Rectangle _area;
        private OverlayEngine _overlayer;
        private Tweener<float> _tweener;
        private string _text;

        private Rectangle _drawArea;
        private float _heightCoefficient;
        private Vector2 _textPos;
        private Action _action;

        private bool _hasHitMax;

        public TransitionOverlay()
        {
            _area = Rectangle.Empty;
            _drawArea = Rectangle.Empty;
            _overlayer = null!; // I know I know
            _tweener = null!; // yupyup
            Ended = false;
            _text = string.Empty;
            _textPos = Vector2.Zero;
            _action = () => { };
        }

        public void Update(int elapsedMiliseconds)
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

            _tweener.Update(elapsedMiliseconds / 1000f);

            _heightCoefficient = _tweener.Value;

            _drawArea = new Rectangle(_area.Location, new Point(_area.Width, (int)(desiredHeight * _heightCoefficient)));
            _textPos = Centering.MiddleXY(STOLON.Fonts[STOLON.SMALL_FONT_ID].FastMeasure(_text).ToPoint(), _drawArea, new Vector2(TextSizeMod));
            _textPos = new Vector2(_textPos.X, Math.Min(_textPos.Y, _drawArea.Height - STOLON.Fonts[STOLON.SMALL_FONT_ID].Dimensions.Y * TextSizeMod));

            Centering.OnPixel(ref _textPos);
        }

        public void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            drawingContext.DrawArea(_drawArea, Color.Black);
            drawingContext.DrawRectangle(_drawArea, Color.White);
            drawingContext.DrawString(STOLON.Fonts[STOLON.SMALL_FONT_ID], _text, _textPos, TextSizeMod);
        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
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
