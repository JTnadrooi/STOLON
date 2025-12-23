
namespace STOLON
{
    public class Texture2DResourceLoader : SequentialResourceLoader<Texture2D>, ITransientDependency
    {
        public Texture2DResourceLoader(IRichLogger logger) : base(logger)
        {
        }

        public override string[] GetItems() => Directory.GetFiles("Textures", "*.png", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Textures\\".Length..^".png".Length];

        public override Texture2D LoadItem(string item)
        {
            using FileStream fileStream = new FileStream(item, FileMode.Open);
            return Texture2D.FromStream(STOLON.Instance.GraphicsDevice, fileStream);
        }
    }
}
