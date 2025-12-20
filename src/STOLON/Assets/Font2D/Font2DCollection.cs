namespace STOLON
{
    public class Font2DCollection : ResourceCollection<Font2D>
    {
        public Font2D Small => this["smollerMono"];
        public Font2D Medium => this["welcome"];

        public Font2DCollection() : base(new Font2DResourceLoader()) { }
    }
}
