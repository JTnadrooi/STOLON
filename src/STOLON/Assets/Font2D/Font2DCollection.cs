namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public class Font2DCollection : ResourceCollection<Font2D>, IFont2DCollection
    {
        public Font2D Small => this["smollerMono"];
        public Font2D Medium => this["welcome"];

        public Font2DCollection(IResourceLoader<Font2D> loader) : base(loader) { }
    }
}
