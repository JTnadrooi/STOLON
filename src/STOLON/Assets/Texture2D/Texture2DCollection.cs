namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public class Texture2DCollection : ResourceCollection<Texture2D>, ITexture2DCollection
    {
        private Texture2D? _pixel;

        public Texture2D Pixel => _pixel ?? throw new Exception();

        public Texture2DCollection(IResourceLoader<Texture2D> loader) : base(loader) { }

        public override void LoadResources()
        {
            _pixel = new Texture2D(STOLON.Instance.GraphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
            base.LoadResources();
        }

        public override void UnloadResources()
        {
            _pixel.Dispose();
            base.UnloadResources();
        }
    }
}
