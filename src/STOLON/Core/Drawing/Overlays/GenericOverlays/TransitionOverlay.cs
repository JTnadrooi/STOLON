using Betwixt;



namespace STOLON
{
    public class TransitionOverlay : IOverlay
    {
        public string Id => "transition";

        public bool IsFinished { get; private set; }

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
            IsFinished = false;
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
            IsFinished = _hasHitMax && _heightCoefficient < 0.001f;

            _tweener.Update(elapsedMilliseconds / 1000f);

            _heightCoefficient = _tweener.Value;

            _drawArea = new Rectangle(_area.Location, new Point(_area.Width, (int)(desiredHeight * _heightCoefficient)));
            _textPos = Centering.Center((_fonts.Small.FastMeasure(_text) * TextSizeMod).ToPoint(), _drawArea);
            _textPos = new Vector2(_textPos.X, Math.Min(_textPos.Y, _drawArea.Height - _fonts.Small.Dimensions.Y * TextSizeMod));

            NumberHelper.OnPixel(ref _textPos);
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawArea(_drawArea, Color.Black);
            drawingContext.DrawRectangle(_drawArea, Color.White);
            drawingContext.DrawString(_fonts.Small, _text, _textPos, TextSizeMod);
        }

        public void Initialize(OverlayManager overlayer, params object?[] args)
        {
            IsFinished = false;
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
