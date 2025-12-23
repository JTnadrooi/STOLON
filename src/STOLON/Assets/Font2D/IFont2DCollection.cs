namespace STOLON
{
    public interface IFont2DCollection : IResourceCollection<Font2D>
    {
        Font2D Medium { get; }
        Font2D Small { get; }
    }
}