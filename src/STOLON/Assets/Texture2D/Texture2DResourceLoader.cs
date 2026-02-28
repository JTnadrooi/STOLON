
namespace STOLON
{
    [Dependency(ServiceLifetime.Transient)]
    public class Texture2DResourceLoader : SequentialResourceLoader<Texture2D>
    {
        public Texture2DResourceLoader(IRichLogger logger) : base(logger)
        {
        }

        public override string[] GetItems() => Directory.GetFiles("Textures", "*.png", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Textures\\".Length..^".png".Length];

        public override Texture2D LoadItem(string item)
        {
            using FileStream fileStream = new FileStream(item, FileMode.Open);

            Texture2D result = Texture2D.FromStream(STOLON.Instance.GraphicsDevice, fileStream);

            result.Name = GetId(item);

            return result;
        }
    }
}
