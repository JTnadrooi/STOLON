using MonoGame.Extended.BitmapFonts;

namespace STOLON
{
    [Dependency(ServiceLifetime.Transient)]
    public class Font2DResourceLoader : SequentialResourceLoader<Font2D>
    {
        public Font2DResourceLoader(IRichLogger logger) : base(logger)
        {
        }

        public override string[] GetItems() => Directory.GetFiles("Fonts", "*.fnt", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Fonts\\".Length..^".fnt".Length];

        public override Font2D LoadItem(string item)
        {
            using FileStream fileStream = new FileStream(item, FileMode.Open);
            return BitmapFont.FromStream(STOLON.Instance.GraphicsDevice, fileStream, item);
        }
    }
}
