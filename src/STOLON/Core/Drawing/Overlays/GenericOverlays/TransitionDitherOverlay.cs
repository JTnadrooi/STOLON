using Betwixt;



namespace STOLON
{
    public class TransitionDitherOverlay : IOverlay
    {
        public string Id => "transition_dither";

        public bool IsFinished => _ended;

        private Texture2D _ditherTexture;
        private const int FramePixelsToRemove = 11150 / Resolution; // Number of pixels to turn transparent each frame
        private const int Resolution = 2;
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
            this._resolution = Resolution;
            _random = new Random();


            _tweener = new Tweener<float>(1, FramePixelsToRemove, 5f, Ease.Expo.In);
            _height = STOLON.VHeight / Resolution;
            _width = STOLON.VWidth / Resolution;

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
}
