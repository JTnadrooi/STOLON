namespace STOLON
{
    public class LoadOverlay : IOverlay
    {
        public string Id => "loading";
        public bool IsFinished { get; private set; }

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

            _pos = new Vector2(STOLON.VWidth, STOLON.VHeight) + new Vector2(-lineTexture.Width, -lineTexture.Height) * _scale;

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
}
