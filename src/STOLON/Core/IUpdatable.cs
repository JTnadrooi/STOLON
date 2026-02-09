namespace STOLON
{
    /// <summary>
    /// Provides drawing functionality. Any class that can implement this interface should implement it.
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>
        /// Updates this <see cref="IUpdatable"/> instance.
        /// </summary>
        void Update(int elapsedMilliseconds);
    }
}
