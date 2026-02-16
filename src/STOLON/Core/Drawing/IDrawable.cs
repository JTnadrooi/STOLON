namespace STOLON
{
    /// <summary>
    /// Provides drawing functionality. Any class that can implement this interface should implement it.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Draws this <see cref="IDrawable"/> instance.
        /// </summary>
        void Draw(DrawingContext drawingContext);
    }
}
