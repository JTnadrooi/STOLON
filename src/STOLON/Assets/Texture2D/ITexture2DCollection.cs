
namespace STOLON
{
    public interface ITexture2DCollection : IResourceCollection<Texture2D>
    {
        Texture2D Pixel { get; }
    }
}